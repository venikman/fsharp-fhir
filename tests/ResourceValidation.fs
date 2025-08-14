module FSharpFHIR.ResourceValidation

open System.IO
open System.Text.Json

let patientSamplePath =
    Path.Combine(__SOURCE_DIRECTORY__, "..", "examples", "patient.json")

let isValid (path: string) =
    let json = File.ReadAllText path
    use doc = JsonDocument.Parse json
    let root = doc.RootElement
    let mutable resourceType = Unchecked.defaultof<JsonElement>
    let mutable id = Unchecked.defaultof<JsonElement>
    let hasResourceType = root.TryGetProperty("resourceType", &resourceType)
    let hasId = root.TryGetProperty("id", &id)
    hasResourceType && hasId && resourceType.GetString() = "Patient"
