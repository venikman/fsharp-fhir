namespace FSharpFHIR.Creator

open System
open FSharpFHIR
open Charm

module Program =
    /// Command that creates a simple Patient resource as JSON
    let patientCommand =
        cmd "patient" {
            desc "Create a minimal Patient resource"
            opt "name" { desc "Patient name" }
            opt "gender" { desc "Gender (male|female|other|unknown)" }
            opt "birthDate" { desc "Birth date (YYYY-MM-DD)" }
            opt "out" { desc "Output file (defaults to stdout)" }
            run (fun ctx ->
                let name = ctx?name |> Option.ofObj |> Option.map string
                let gender = ctx?gender |> Option.ofObj |> Option.map string
                let birth = ctx?birthDate |> Option.ofObj |> Option.map string
                let outFile = ctx?out |> Option.ofObj |> Option.map string

                // Build minimal Patient JSON
                let parts =
                    [ Some "\"resourceType\":\"Patient\"";
                      name |> Option.map (fun n -> $"\"name\":[{{\"text\":\"{n}\"}}]");
                      gender |> Option.map (fun g -> $"\"gender\":\"{g}\"");
                      birth |> Option.map (fun b -> $"\"birthDate\":\"{b}\"") ]
                    |> List.choose id
                let json = "{" + String.Join(",", parts) + "}"

                match outFile with
                | Some path ->
                    System.IO.File.WriteAllText(path, json)
                    printfn "Wrote Patient resource to %s" path
                    0
                | None ->
                    printfn "%s" json
                    0)
        }

    /// Root create command
    let createCommand =
        cmd "create" {
            desc "Create FHIR resources"
            cmd patientCommand
        }

    [<EntryPoint>]
    let main argv =
        run createCommand argv
