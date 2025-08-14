module FSharpFHIR.Cli

open FSharpFHIR.ResourceValidation

let run (args: string[]) =
    match args with
    | [| "validate"; file |] ->
        if isValid file then
            printfn "Validation succeeded"
            0
        else
            printfn "Validation failed"
            1
    | _ ->
        printfn "Usage: validate <file>"
        1
