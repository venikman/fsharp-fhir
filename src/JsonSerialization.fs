namespace FSharpFHIR

open System
open System.IO
open System.Text.Json

module JsonSerialization =
    
    /// Helper to safely get a JSON property value
    let tryGetProperty (propertyName: string) (element: JsonElement) =
        let mutable value = Unchecked.defaultof<JsonElement>
        if element.TryGetProperty(propertyName, &value) then
            Some value
        else
            None
    
    /// Helper to safely get a string value from JSON
    let tryGetString (element: JsonElement) =
        if element.ValueKind = JsonValueKind.String then
            Some (element.GetString())
        else
            None
    
    /// Helper to safely get a boolean value from JSON
    let tryGetBoolean (element: JsonElement) =
        if element.ValueKind = JsonValueKind.True || element.ValueKind = JsonValueKind.False then
            Some (element.GetBoolean())
        else
            None
    
    /// Helper to safely get a datetime value from JSON
    let tryGetDateTime (element: JsonElement) =
        if element.ValueKind = JsonValueKind.String then
            match DateTime.TryParse(element.GetString()) with
            | true, dt -> Some dt
            | false, _ -> None
        else
            None
    
    /// Parse administrative gender from string
    let parseAdministrativeGender (gender: string) =
        match gender.ToLowerInvariant() with
        | "male" -> Some AdministrativeGender.Male
        | "female" -> Some AdministrativeGender.Female  
        | "other" -> Some AdministrativeGender.Other
        | "unknown" -> Some AdministrativeGender.Unknown
        | _ -> None
        
    /// Parse observation status from string
    let parseObservationStatus (status: string) =
        match status.ToLowerInvariant() with
        | "registered" -> Some ObservationStatus.Registered
        | "preliminary" -> Some ObservationStatus.Preliminary
        | "final" -> Some ObservationStatus.Final
        | "amended" -> Some ObservationStatus.Amended
        | _ -> None
    
    /// Try to parse an Observation resource from JSON
    let tryParseObservation (element: JsonElement) : Result<Observation, string list> =
        let errors = ResizeArray<string>()
        
        // Check resource type
        match tryGetProperty "resourceType" element with
        | Some rt when (tryGetString rt) = Some "Observation" -> ()
        | Some _ -> errors.Add("Expected resourceType 'Observation'")
        | None -> errors.Add("Missing required 'resourceType' property")
        
        // Parse basic resource properties
        let id = tryGetProperty "id" element |> Option.bind tryGetString
        let meta = None // Simplified for now
        let language = tryGetProperty "language" element |> Option.bind tryGetString
        
        let resource = {
            id = id
            meta = meta
            language = language
        }
        
        let domainResource = {
            resource = resource
            text = None // Simplified for now
            contained = []
            extension = []
            modifierExtension = []
        }
        
        // Parse required status field
        let status = 
            match tryGetProperty "status" element |> Option.bind tryGetString with
            | Some statusStr ->
                match parseObservationStatus statusStr with
                | Some status -> Some status
                | None -> 
                    errors.Add(sprintf "Invalid observation status: '%s'" statusStr)
                    None
            | None ->
                errors.Add("Missing required 'status' property")
                None
        
        // Parse optional fields
        let code = None // Simplified for now - would parse CodeableConcept
        let subject = None // Simplified for now - would parse Reference
        let effective = tryGetProperty "effective" element |> Option.bind tryGetDateTime
        let value = None // Simplified for now - would parse ElementValue
        
        if errors.Count > 0 then
            Error (errors |> Seq.toList)
        else
            match status with
            | Some validStatus ->
                let observation = {
                    resource = domainResource
                    identifier = [] // Simplified for now
                    status = validStatus
                    code = code
                    subject = subject
                    effective = effective
                    value = value
                }
                Ok observation
            | None ->
                Error ["Invalid observation status"]
    
    /// Try to parse a Patient resource from JSON
    let tryParsePatient (element: JsonElement) : Result<Patient, string list> =
        let errors = ResizeArray<string>()
        
        // Check resource type
        match tryGetProperty "resourceType" element with
        | Some rt when (tryGetString rt) = Some "Patient" -> ()
        | Some _ -> errors.Add("Expected resourceType 'Patient'")
        | None -> errors.Add("Missing required 'resourceType' property")
        
        // Parse basic resource properties
        let id = tryGetProperty "id" element |> Option.bind tryGetString
        let meta = None // Simplified for now
        let language = tryGetProperty "language" element |> Option.bind tryGetString
        
        let resource = {
            id = id
            meta = meta
            language = language
        }
        
        let domainResource = {
            resource = resource
            text = None // Simplified for now
            contained = []
            extension = []
            modifierExtension = []
        }
        
        // Parse Patient-specific properties
        let active = tryGetProperty "active" element |> Option.bind tryGetBoolean
        
        // Parse name array
        let names = 
            match tryGetProperty "name" element with
            | Some nameArray when nameArray.ValueKind = JsonValueKind.Array ->
                nameArray.EnumerateArray()
                |> Seq.map (fun nameObj ->
                    let family = tryGetProperty "family" nameObj |> Option.bind tryGetString
                    let given = 
                        match tryGetProperty "given" nameObj with
                        | Some givenArray when givenArray.ValueKind = JsonValueKind.Array ->
                            givenArray.EnumerateArray()
                            |> Seq.choose tryGetString
                            |> Seq.toList
                        | _ -> []
                    let text = tryGetProperty "text" nameObj |> Option.bind tryGetString
                    {
                        ``use`` = None // Simplified for now
                        text = text
                        family = family
                        given = given
                    }
                )
                |> Seq.toList
            | _ -> []
        
        // Parse gender
        let gender = 
            tryGetProperty "gender" element 
            |> Option.bind tryGetString
            |> Option.bind parseAdministrativeGender
        
        // Parse birthDate
        let birthDate = tryGetProperty "birthDate" element |> Option.bind tryGetDateTime
        
        if errors.Count > 0 then
            Error (errors |> Seq.toList)
        else
            let patient = {
                resource = domainResource
                identifier = [] // Simplified for now
                active = active
                name = names
                gender = gender
                birthDate = birthDate
            }
            Ok patient
    
    /// Validate and parse any supported FHIR resource from JSON
    let tryParseResource (json: string) : Result<string * obj, string list> =
        try
            use doc = JsonDocument.Parse(json)
            let root = doc.RootElement
            
            match tryGetProperty "resourceType" root |> Option.bind tryGetString with
            | Some "Patient" ->
                match tryParsePatient root with
                | Ok patient -> Ok ("Patient", box patient)
                | Error errors -> Error errors
            | Some "Observation" ->
                match tryParseObservation root with
                | Ok observation -> Ok ("Observation", box observation)
                | Error errors -> Error errors
            | Some resourceType ->
                Error [sprintf "Resource type '%s' is not yet supported for full validation" resourceType]
            | None ->
                Error ["Missing required 'resourceType' property"]
        with ex ->
            Error ["Invalid JSON: " + ex.Message]