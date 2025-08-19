namespace FSharpFHIR

open System
open System.IO
open System.Text.Json
open FSharpFHIR.QueryEngine

module InteractiveRepl =
    
    type ReplCommand =
        | Help
        | Exit
        | Clear
        | History
        | Stats
        | Load of string
        | Save of string
        | Query of string
        | Unknown of string
    
    type ReplState = {
        History: string list
        CurrentData: JsonElement option
        CurrentFile: string option
        QueryCount: int
        StartTime: DateTime
    }
    
    let private parseCommand (input: string) : ReplCommand =
        let trimmed = input.Trim()
        if trimmed.StartsWith(":") then
            let parts = trimmed.Substring(1).Split([|' '|], 2, StringSplitOptions.RemoveEmptyEntries)
            match parts with
            | [|"help"|] | [|"h"|] -> Help
            | [|"exit"|] | [|"quit"|] | [|"q"|] -> Exit
            | [|"clear"|] | [|"cls"|] -> Clear
            | [|"history"|] | [|"hist"|] -> History
            | [|"stats"|] | [|"statistics"|] -> Stats
            | [|"load"; file|] -> Load file
            | [|"save"; file|] -> Save file
            | _ -> Unknown trimmed
        else
            Query trimmed
    
    let private showHelp () =
        printfn "\033[1;36mFHIR Query REPL Commands:\033[0m"
        printfn "  \033[33m:help\033[0m, \033[33m:h\033[0m          Show this help message"
        printfn "  \033[33m:exit\033[0m, \033[33m:quit\033[0m, \033[33m:q\033[0m   Exit the REPL"
        printfn "  \033[33m:clear\033[0m, \033[33m:cls\033[0m        Clear the screen"
        printfn "  \033[33m:history\033[0m, \033[33m:hist\033[0m      Show query history"
        printfn "  \033[33m:stats\033[0m                Show session statistics"
        printfn "  \033[33m:load <file>\033[0m          Load JSON file for querying"
        printfn "  \033[33m:save <file>\033[0m          Save last query result to file"
        printfn ""
        printfn "\033[1mQuery Examples:\033[0m"
        printfn "  \033[32m.\033[0m               Show entire document"
        printfn "  \033[32m.name\033[0m           Get name property"
        printfn "  \033[32m.name?.given\033[0m     Safe navigation to given names"
        printfn "  \033[32m.identifier[]\033[0m    Get all identifiers"
        printfn "  \033[32m.identifier[0]\033[0m   Get first identifier"
        printfn "  \033[32m.name | length\033[0m   Count name entries"
        printfn "  \033[32m.name | keys\033[0m     Get property names"
        printfn "  \033[32m.observation | select(.status == \"final\")\033[0m"
        printfn "  \033[32m.value | typeof\033[0m   Get type information"
        printfn "  \033[32m.birthDate | is_some\033[0m  Check if value exists"
        printfn "  \033[32m.identifier | map(.value)\033[0m  Extract values"
        printfn "  \033[32m.name | contains(\"John\")\033[0m  String search"
        printfn ""
        printfn "\033[1mF# Type-Aware Features:\033[0m"
        printfn "  \033[32mtypeof\033[0m          Get JSON type (string, number, boolean, array, object, null)"
        printfn "  \033[32mis_some\033[0m         Check if value is not null"
        printfn "  \033[32mis_none\033[0m         Check if value is null"
        printfn "  \033[32mis_empty\033[0m        Check if array/object/string is empty"
        printfn "  \033[32mcount\033[0m           Count elements in array/object"
        printfn "  \033[32msum\033[0m             Sum numbers in array"
        printfn "  \033[32mavg\033[0m             Average numbers in array"
        printfn "  \033[32mmin/max\033[0m         Min/max numbers in array"
        printfn ""
    
    let private showHistory (history: string list) =
        printfn "\033[1;36mQuery History:\033[0m"
        history
        |> List.rev
        |> List.iteri (fun i query -> printfn "  \033[33m%d:\033[0m %s" (i + 1) query)
        printfn ""
    
    let private showStats (state: ReplState) =
        let duration = DateTime.Now - state.StartTime
        printfn "\033[1;36mSession Statistics:\033[0m"
        printfn "  \033[33mQueries executed:\033[0m %d" state.QueryCount
        printfn "  \033[33mSession duration:\033[0m %s" (duration.ToString(@"hh\:mm\:ss"))
        printfn "  \033[33mCurrent file:\033[0m %s" (state.CurrentFile |> Option.defaultValue "<none>")
        printfn "  \033[33mData loaded:\033[0m %s" (if state.CurrentData.IsSome then "Yes" else "No")
        printfn ""
    
    let private loadFile (filePath: string) (state: ReplState) : ReplState =
        try
            if not (File.Exists(filePath)) then
                printfn "\033[31mError: File '%s' not found\033[0m" filePath
                state
            else
                let json = File.ReadAllText(filePath)
                let document = JsonDocument.Parse(json)
                printfn "\033[32mLoaded file: %s\033[0m" filePath
                { state with CurrentData = Some document.RootElement; CurrentFile = Some filePath }
        with
        | ex ->
            printfn "\033[31mError loading file: %s\033[0m" ex.Message
            state
    
    let private saveResult (filePath: string) (result: QueryResult) =
        try
            let output = 
                match result with
                | Single element -> element
                | Multiple elements -> 
                    "[" + String.Join(",", elements) + "]"
                | Empty -> "null"
                | Error msg -> sprintf "{\"error\": \"%s\"}" msg
            
            File.WriteAllText(filePath, output)
            printfn "\033[32mResult saved to: %s\033[0m" filePath
        with
        | ex ->
            printfn "\033[31mError saving file: %s\033[0m" ex.Message
    
    let private executeQuery (query: string) (data: JsonElement) : QueryResult * TimeSpan =
        let stopwatch = System.Diagnostics.Stopwatch.StartNew()
        let result = 
            match Query.runInteractive query data with
            | Ok queryResult -> queryResult
            | Result.Error msg -> QueryResult.Error msg
        stopwatch.Stop()
        (result, stopwatch.Elapsed)
    
    let private formatResult (result: QueryResult) (executionTime: TimeSpan) =
        match result with
        | Single element ->
            try
                use doc = JsonDocument.Parse(element)
                let jsonElement = doc.RootElement
                let formatted = 
                    if jsonElement.ValueKind = JsonValueKind.String then
                        sprintf "\"\033[32m%s\033[0m\"" (jsonElement.GetString())
                    elif jsonElement.ValueKind = JsonValueKind.Number then
                        sprintf "\033[33m%s\033[0m" (jsonElement.GetRawText())
                    elif jsonElement.ValueKind = JsonValueKind.True || jsonElement.ValueKind = JsonValueKind.False then
                        sprintf "\033[35m%s\033[0m" (jsonElement.GetRawText())
                    elif jsonElement.ValueKind = JsonValueKind.Null then
                        "\033[90mnull\033[0m"
                    else
                        jsonElement.GetRawText()
                printfn "%s" formatted
            with
            | _ -> printfn "%s" element
            printfn "\033[90m(executed in %dms)\033[0m" (int executionTime.TotalMilliseconds)
        
        | Multiple elements ->
            printfn "\033[36m[\033[0m"
            elements |> List.iteri (fun i element ->
                let formatted = 
                    try
                        use doc = JsonDocument.Parse(element)
                        let jsonElement = doc.RootElement
                        if jsonElement.ValueKind = JsonValueKind.String then
                            sprintf "  \"\033[32m%s\033[0m\"" (jsonElement.GetString())
                        elif jsonElement.ValueKind = JsonValueKind.Number then
                            sprintf "  \033[33m%s\033[0m" (jsonElement.GetRawText())
                        elif jsonElement.ValueKind = JsonValueKind.True || jsonElement.ValueKind = JsonValueKind.False then
                            sprintf "  \033[35m%s\033[0m" (jsonElement.GetRawText())
                        elif jsonElement.ValueKind = JsonValueKind.Null then
                            "  \033[90mnull\033[0m"
                        else
                            "  " + jsonElement.GetRawText()
                    with
                    | _ -> "  " + element
                let separator = if i < elements.Length - 1 then "," else ""
                printfn "%s%s" formatted separator
            )
            printfn "\033[36m]\033[0m"
            printfn "\033[90m(%d items, executed in %dms)\033[0m" elements.Length (int executionTime.TotalMilliseconds)
        
        | Empty ->
            printfn "\033[90mnull\033[0m"
            printfn "\033[90m(no results, executed in %dms)\033[0m" (int executionTime.TotalMilliseconds)
        
        | Error msg ->
            printfn "\033[31mError: %s\033[0m" msg
            printfn "\033[90m(failed in %dms)\033[0m" (int executionTime.TotalMilliseconds)
    
    let private getPrompt (state: ReplState) =
        let fileIndicator = 
            match state.CurrentFile with
            | Some file -> sprintf "\033[90m[%s]\033[0m " (Path.GetFileName(file))
            | None -> "\033[90m[no file]\033[0m "
        sprintf "%s\033[1;34mfhir>\033[0m " fileIndicator
    
    let private clearScreen () =
        Console.Clear()
        printfn "\033[1;36mFHIR Query REPL - Interactive Mode\033[0m"
        printfn "Type \033[33m:help\033[0m for available commands\n"
    
    let run (initialFile: string option) =
        let mutable state = {
            History = []
            CurrentData = None
            CurrentFile = None
            QueryCount = 0
            StartTime = DateTime.Now
        }
        
        // Load initial file if provided
        state <- 
            match initialFile with
            | Some file -> loadFile file state
            | None -> state
        
        clearScreen()
        
        if state.CurrentData.IsNone then
            printfn "\033[33mNo data loaded. Use \033[1m:load <file>\033[0m to load a JSON file.\033[0m\n"
        
        let mutable keepRunning = true
        let mutable lastResult = Empty
        
        while keepRunning do
            printf "%s" (getPrompt state)
            let input = Console.ReadLine()
            
            if String.IsNullOrWhiteSpace(input) then
                () // Skip empty input
            else
                match parseCommand input with
                | Help -> showHelp()
                
                | Exit -> 
                    printfn "\033[36mGoodbye!\033[0m"
                    keepRunning <- false
                
                | Clear -> clearScreen()
                
                | History -> showHistory state.History
                
                | Stats -> showStats state
                
                | Load file -> 
                    state <- loadFile file state
                
                | Save file ->
                    saveResult file lastResult
                
                | Unknown cmd ->
                    printfn "\033[31mUnknown command: %s\033[0m" cmd
                    printfn "Type \033[33m:help\033[0m for available commands"
                
                | Query query ->
                    match state.CurrentData with
                    | None ->
                        printfn "\033[31mNo data loaded. Use \033[1m:load <file>\033[0m to load a JSON file.\033[0m"
                    | Some data ->
                        let (result, executionTime) = executeQuery query data
                        formatResult result executionTime
                        lastResult <- result
                        state <- { state with 
                                     History = query :: state.History
                                     QueryCount = state.QueryCount + 1 }
                        printfn ""
        
        () // End of run function