module FSharpFHIR.Cli

open System
open System.CommandLine
open FSharpFHIR.Validator

/// Enhanced validator CLI that tests FHIR types
[<EntryPoint>]
let main argv =
    // Create the root command
    let rootCommand = RootCommand("Validates a FHIR resource JSON file against F# type definitions.")
    
    // Create the validate command
    let validateCommand = Command("validate", "Validate a FHIR resource JSON file")
    
    // Create options
    let fileOption = Option<string>("--file", "Path to the JSON file to validate")
    fileOption.IsRequired <- true
    
    let verboseOption = Option<bool>("--verbose", "Show additional information about valid resources")
    verboseOption.SetDefaultValue(false)
    
    // Add options to the validate command
    validateCommand.AddOption(fileOption)
    validateCommand.AddOption(verboseOption)
    
    // Set the handler for the validate command
    validateCommand.SetHandler(Action<string, bool>(fun file verbose ->
        let errors = Validator.validateFile file
        if List.isEmpty errors then
            printfn "✓ Resource is valid"
            if verbose then
                match IO.File.ReadAllText file |> Validator.getResourceInfo with
                | Ok (resourceType, info) ->
                    printfn "  Resource type: %s" resourceType
                    printfn "  Details: %s" info
                | Error _ -> ()
            Environment.Exit(0)
        else
            printfn "✗ Validation errors:"
            errors |> List.iter (printfn "  - %s")
            Environment.Exit(1)
    ), fileOption, verboseOption)
    
    // Add the validate command to the root command
    rootCommand.AddCommand(validateCommand)
    
    // Invoke the command
    rootCommand.Invoke(argv)
