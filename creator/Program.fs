module FSharpFHIR.Creator

open System

/// Simple CLI for generating minimal Patient resources
[<EntryPoint>]
let main argv =
    let rec parse opts args =
        match args with
        | "--name" :: v :: rest -> parse (("name", v)::opts) rest
        | "--gender" :: v :: rest -> parse (("gender", v)::opts) rest
        | "--birthDate" :: v :: rest -> parse (("birthDate", v)::opts) rest
        | "--out" :: v :: rest -> parse (("out", v)::opts) rest
        | _ -> Map.ofList opts

    let argsList = Array.toList argv
    match argsList with
    | "create" :: "patient" :: tail ->
        let opts = parse [] tail
        let name = Map.tryFind "name" opts
        let gender = Map.tryFind "gender" opts
        let birth = Map.tryFind "birthDate" opts
        let outFile = Map.tryFind "out" opts

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
            0
    | _ ->
        printfn "Usage: create patient [--name <name>] [--gender <gender>] [--birthDate <YYYY-MM-DD>] [--out <file>]"
        1
