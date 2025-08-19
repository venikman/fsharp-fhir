open System
open System.IO
open System.Text.Json
open Fli
open FSharpFHIR
open FSharpFHIR.Validator
open FSharpFHIR.Visualizer
open FSharpFHIR.QueryEngine
open FSharpFHIR.InteractiveRepl

// Helper function to execute external commands using Fli when needed
let executeCommand (program: string) (args: string list) =
    cli {
        Exec program
        Arguments args
    }
    |> Command.execute

let validateFile (filePath: string) =
    try
        if not (File.Exists(filePath)) then
            printfn "Error: File '%s' not found" filePath
            1
        else
            let json = File.ReadAllText(filePath)
            let errors = Validator.validate json
            if errors.IsEmpty then
                printfn "✓ Valid FHIR resource"
                0
            else
                printfn "✗ Validation failed:"
                errors |> List.iter (printfn "  - %s")
                1
    with
    | ex ->
        printfn "Error: %s" ex.Message
        1

let visualizeFile (filePath: string) (outputPath: string option) =
    try
        let json = File.ReadAllText(filePath)
        let visualization = Visualizer.visualizeJson json Visualizer.VisualizationFormat.Tree
        let visualizationText = String.concat "\n" visualization
        
        match outputPath with
        | Some path ->
            File.WriteAllText(path, visualizationText)
            printfn "Visualization saved to: %s" path
            
            // Open the file with default application
            executeCommand (if Environment.OSVersion.Platform = PlatformID.Win32NT then "notepad.exe" else "open") [path] |> ignore
        | None ->
            printfn "%s" visualizationText
        0
    with
    | ex ->
        printfn "Error: %s" ex.Message
        1

let queryFile (filePath: string) (queryStr: string) (interactive: bool) (format: string) : int =
    try
        let json = File.ReadAllText(filePath)
        
        if interactive then
            InteractiveRepl.run (Some filePath)
            0
        else
            let queryResult = Query.run queryStr json
            match queryResult with
            | Result.Ok result ->
                match result with
                | FSharpFHIR.QueryEngine.QueryResult.Single element ->
                    printfn "%s" element
                    0
                | FSharpFHIR.QueryEngine.QueryResult.Multiple elements ->
                    elements |> List.iter (printfn "%s")
                    0
                | FSharpFHIR.QueryEngine.QueryResult.Empty ->
                    printfn "null"
                    0
            | Result.Error msg ->
                printfn "Query error: %s" msg
                1
    with
    | ex ->
        printfn "Error: %s" ex.Message
        1

// Function to check if external tools are available using Fli
let checkExternalTool (toolName: string) =
    try
        let result = 
            cli {
                Exec "which"
                Arguments [toolName]
            }
            |> Command.execute
        
        result.ExitCode = 0
    with
    | _ -> false

let printHelp () =
    printfn "FSharp FHIR CLI Tool (powered by Fli)"
    printfn ""
    printfn "Usage:"
    printfn "  fsharp-fhir validate <file>                    - Validate FHIR resource"
    printfn "  fsharp-fhir visualize <file> [--output <path>] - Visualize FHIR resource"
    printfn "  fsharp-fhir query <file> <query> [options]     - Query FHIR resource"
    printfn "  fsharp-fhir tools                              - Check available external tools"
    printfn ""
    printfn "Query Options:"
    printfn "  --interactive, -i    - Start interactive REPL mode"
    printfn "  --format <format>    - Output format (json, compact, pretty, file)"
    printfn ""
    printfn "Examples:"
    printfn "  fsharp-fhir validate patient.json"
    printfn "  fsharp-fhir visualize patient.json --output patient.txt"
    printfn "  fsharp-fhir query patient.json '.name[0].given[0]'"
    printfn "  fsharp-fhir query patient.json '' --interactive"
    printfn "  fsharp-fhir query patient.json '.name' --format file"

let checkTools () =
    printfn "Checking external tools availability:"
    
    let tools = ["git"; "dotnet"; "code"; "notepad"; "open"]
    
    for tool in tools do
        let available = checkExternalTool tool
        let status = if available then "✓" else "✗"
        printfn "  %s %s" status tool
    
    0

[<EntryPoint>]
let main args =
    match args with
    | [||] ->
        printHelp()
        0
    | [|"validate"; filePath|] ->
        validateFile filePath
    | [|"visualize"; filePath|] ->
        visualizeFile filePath None
    | [|"visualize"; filePath; "--output"; outputPath|] ->
        visualizeFile filePath (Some outputPath)
    | [|"query"; filePath; queryStr|] ->
        queryFile filePath queryStr false "pretty"
    | [|"query"; filePath; queryStr; "--format"; format|] ->
        queryFile filePath queryStr false format
    | [|"query"; filePath; queryStr; "--interactive"|] ->
        queryFile filePath queryStr true "pretty"
    | [|"query"; filePath; queryStr; "-i"|] ->
        queryFile filePath queryStr true "pretty"
    | [|"query"; filePath; ""; "--interactive"|] ->
        queryFile filePath "" true "pretty"
    | [|"query"; filePath; ""; "-i"|] ->
        queryFile filePath "" true "pretty"
    | [|"tools"|] ->
        checkTools()
    | [|"--help"|] | [|"-h"|] | [|"help"|] ->
        printHelp()
        0
    | _ ->
        printfn "Invalid arguments. Use --help for usage information."
        printHelp()
        1
