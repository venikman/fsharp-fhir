module FSharpFHIR.Cli

open System
open FSharpFHIR.Validator

/// Enhanced validator CLI that tests FHIR types
[<EntryPoint>]
let main argv =
    let args = Array.toList argv
    match args with
    | "validate" :: "--file" :: file :: rest ->
        let verbose = List.contains "--verbose" rest
        let errors = Validator.validateFile file
        if List.isEmpty errors then
            printfn "✓ Resource is valid"
            if verbose then
                match IO.File.ReadAllText file |> Validator.getResourceInfo with
                | Ok (resourceType, info) ->
                    printfn "  Resource type: %s" resourceType
                    printfn "  Details: %s" info
                | Error _ -> ()
            0
        else
            printfn "✗ Validation errors:"
            errors |> List.iter (printfn "  - %s")
            1
    | "validate" :: "--help" :: _ ->
        printfn "Usage: validate --file <path> [--verbose]"
        printfn ""
        printfn "Validates a FHIR resource JSON file against F# type definitions."
        printfn ""
        printfn "Options:"
        printfn "  --file <path>    Path to the JSON file to validate"
        printfn "  --verbose        Show additional information about valid resources"
        printfn "  --help           Show this help message"
        0
    | [] | ["validate"] ->
        printfn "Usage: validate --file <path> [--verbose]"
        printfn "Use 'validate --help' for more information."
        1
    | _ ->
        printfn "Usage: validate --file <path> [--verbose]"
        printfn "Use 'validate --help' for more information."
        1
