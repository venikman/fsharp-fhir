namespace FSharpFHIR.Cli

open System
open FSharpFHIR
open FSharpFHIR.Validator
open Charm

module Program =
    /// Command that validates a JSON file containing a FHIR resource
    let validateCommand =
        cmd "validate" {
            desc "Validate a FHIR resource provided as a JSON file"
            opt "file" {
                desc "Path to the JSON resource file"
            }
            run (fun ctx ->
                match ctx?file with
                | :? string as file ->
                    let errors = Validator.validateFile file
                    if List.isEmpty errors then
                        printfn "Resource is valid"
                        0
                    else
                        printfn "Validation errors:"
                        errors |> List.iter (printfn "- %s")
                        1
                | _ ->
                    printfn "--file option is required"
                    1)
        }

    [<EntryPoint>]
    let main argv =
        run validateCommand argv
