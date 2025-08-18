namespace FSharpFHIR

open System
open System.Text.Json

module Validator =
    /// Basic validation that only checks for required resourceType property
    let validateBasic (json:string) : string list =
        try
            use doc = JsonDocument.Parse(json)
            let root = doc.RootElement
            if root.TryGetProperty("resourceType", ref Unchecked.defaultof<JsonElement>) then
                []
            else
                ["Missing required 'resourceType' property"]
        with ex ->
            ["Invalid JSON: " + ex.Message]

    /// Enhanced validation that parses JSON into F# FHIR types
    let validate (json:string) : string list =
        match JsonSerialization.tryParseResource json with
        | Ok (resourceType, _) -> 
            [] // Successfully parsed into F# type
        | Error errors -> 
            errors

    /// Validate a JSON file containing FHIR resource with enhanced validation
    let validateFile (path:string) : string list =
        if not (IO.File.Exists path) then
            [sprintf "File '%s' does not exist" path]
        else
            let json = IO.File.ReadAllText path
            validate json

    /// Get information about a successfully parsed resource
    let getResourceInfo (json:string) : Result<string * string, string list> =
        match JsonSerialization.tryParseResource json with
        | Ok (resourceType, resource) ->
            let info = 
                match resourceType with
                | "Patient" ->
                    // We know this is a Patient based on the resource type
                    match resource with
                    | :? Patient as patient ->
                        let nameInfo = 
                            patient.name 
                            |> List.tryHead 
                            |> Option.bind (fun n -> n.text)
                            |> Option.defaultValue "No name specified"
                        sprintf "Patient resource with name: %s" nameInfo
                    | _ -> "Patient resource"
                | "Observation" ->
                    // We know this is an Observation based on the resource type
                    match resource with
                    | :? Observation as observation ->
                        let statusStr = 
                            match observation.status with
                            | ObservationStatus.Registered -> "registered"
                            | ObservationStatus.Preliminary -> "preliminary"  
                            | ObservationStatus.Final -> "final"
                            | ObservationStatus.Amended -> "amended"
                        sprintf "Observation resource with status: %s" statusStr
                    | _ -> "Observation resource"
                | _ -> sprintf "%s resource" resourceType
            Ok (resourceType, info)
        | Error errors ->
            Error errors
