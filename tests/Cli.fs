module FSharpFHIR.Cli

open FSharpFHIR.ResourceValidation

/// Parses CLI arguments and runs the appropriate command.
/// Currently only supports `validate <file>` which validates the provided
/// JSON file and returns a process exit code.
let run (args: string[]) =
    match args with
    | [| "validate"; file |] ->
        if isValid file then
            printfn "Validation succeeded"
            0 // exit success
        else
            printfn "Validation failed"
            1 // exit failure
    | _ ->
        printfn "Usage: validate <file>"
        1 // exit failure for incorrect usage
