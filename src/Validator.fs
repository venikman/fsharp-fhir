namespace FSharpFHIR

open System
open System.Text.Json

module Validator =
    /// Validate FHIR resource given as JSON string. Returns list of error messages.
    let validate (json:string) : string list =
        try
            use doc = JsonDocument.Parse(json)
            let root = doc.RootElement
            if root.TryGetProperty("resourceType", ref Unchecked.defaultof<JsonElement>) then
                []
            else
                ["Missing required 'resourceType' property"]
        with ex ->
            ["Invalid JSON: " + ex.Message]

    /// Validate a JSON file containing FHIR resource
    let validateFile (path:string) : string list =
        if not (IO.File.Exists path) then
            [sprintf "File '%s' does not exist" path]
        else
            let json = IO.File.ReadAllText path
            validate json
