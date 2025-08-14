module FSharpFHIR.Cli

open System
open FSharpFHIR.Validator

/// Basic validator CLI without external dependencies
[<EntryPoint>]
let main argv =
    let args = Array.toList argv
    match args with
    | "validate" :: "--file" :: file :: _ ->
        let errors = Validator.validateFile file
        if List.isEmpty errors then
            printfn "Resource is valid"
            0
        else
            printfn "Validation errors:"
            errors |> List.iter (printfn "- %s")
            1
    | _ ->
        printfn "Usage: validate --file <path>"
        1
