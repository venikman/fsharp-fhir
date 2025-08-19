module JsonSerializationTests

open Expecto
open FSharpFHIR.JsonSerialization
open FSharpFHIR.Types
open FSharpFHIR
open TestHelpers
open System.Text.Json
open System

[<Tests>]
let jsonSerializationTests =
    testList "JsonSerialization Tests" [
        
        testList "Helper Function Tests" [
            test "tryGetProperty should return Some for existing property" {
                let json = """{"name": "test"}"""
                use doc = JsonDocument.Parse(json)
                let result = tryGetProperty "name" doc.RootElement
                Expect.isSome result "Should find existing property"
            }
            
            test "tryGetProperty should return None for non-existing property" {
                let json = """{"name": "test"}"""
                use doc = JsonDocument.Parse(json)
                let result = tryGetProperty "missing" doc.RootElement
                Expect.isNone result "Should not find non-existing property"
            }
            
            test "tryGetString should return Some for string value" {
                let json = """{"name": "test"}"""
                use doc = JsonDocument.Parse(json)
                let nameElement = tryGetProperty "name" doc.RootElement
                match nameElement with
                | Some element ->
                    let result = tryGetString element
                    Expect.equal result (Some "test") "Should extract string value"
                | None -> failtest "Should find name property"
            }
            
            test "tryGetString should return None for non-string value" {
                let json = """{"count": 42}"""
                use doc = JsonDocument.Parse(json)
                let countElement = tryGetProperty "count" doc.RootElement
                match countElement with
                | Some element ->
                    let result = tryGetString element
                    Expect.isNone result "Should not extract string from number"
                | None -> failtest "Should find count property"
            }
            
            test "tryGetBoolean should return Some for boolean value" {
                let json = """{"active": true}"""
                use doc = JsonDocument.Parse(json)
                let activeElement = tryGetProperty "active" doc.RootElement
                match activeElement with
                | Some element ->
                    let result = tryGetBoolean element
                    Expect.equal result (Some true) "Should extract boolean value"
                | None -> failtest "Should find active property"
            }
            
            test "tryGetBoolean should return None for non-boolean value" {
                let json = """{"name": "test"}"""
                use doc = JsonDocument.Parse(json)
                let nameElement = tryGetProperty "name" doc.RootElement
                match nameElement with
                | Some element ->
                    let result = tryGetBoolean element
                    Expect.isNone result "Should not extract boolean from string"
                | None -> failtest "Should find name property"
            }
            
            test "tryGetDateTime should return Some for valid date string" {
                let json = """{"date": "2023-12-25T10:30:00Z"}"""
                use doc = JsonDocument.Parse(json)
                let dateElement = tryGetProperty "date" doc.RootElement
                match dateElement with
                | Some element ->
                    let result = tryGetDateTime element
                    Expect.isSome result "Should parse valid date"
                    match result with
                    | Some dt -> 
                        Expect.equal dt.Year 2023 "Should have correct year"
                        Expect.equal dt.Month 12 "Should have correct month"
                        Expect.equal dt.Day 25 "Should have correct day"
                    | None -> failtest "Should have parsed date"
                | None -> failtest "Should find date property"
            }
            
            test "tryGetDateTime should return None for invalid date string" {
                let json = """{"date": "invalid-date"}"""
                use doc = JsonDocument.Parse(json)
                let dateElement = tryGetProperty "date" doc.RootElement
                match dateElement with
                | Some element ->
                    let result = tryGetDateTime element
                    Expect.isNone result "Should not parse invalid date"
                | None -> failtest "Should find date property"
            }
            
            test "tryGetDateTime should return None for non-string value" {
                let json = """{"count": 42}"""
                use doc = JsonDocument.Parse(json)
                let countElement = tryGetProperty "count" doc.RootElement
                match countElement with
                | Some element ->
                    let result = tryGetDateTime element
                    Expect.isNone result "Should not parse date from number"
                | None -> failtest "Should find count property"
            }
        ]
        
        testList "Enum Parsing Tests" [
            test "parseAdministrativeGender should parse valid genders" {
                Expect.equal (parseAdministrativeGender "male") (Some AdministrativeGender.Male) "Should parse male"
                Expect.equal (parseAdministrativeGender "female") (Some AdministrativeGender.Female) "Should parse female"
                Expect.equal (parseAdministrativeGender "other") (Some AdministrativeGender.Other) "Should parse other"
                Expect.equal (parseAdministrativeGender "unknown") (Some AdministrativeGender.Unknown) "Should parse unknown"
            }
            
            test "parseAdministrativeGender should be case insensitive" {
                Expect.equal (parseAdministrativeGender "MALE") (Some AdministrativeGender.Male) "Should parse uppercase"
                Expect.equal (parseAdministrativeGender "Female") (Some AdministrativeGender.Female) "Should parse mixed case"
            }
            
            test "parseAdministrativeGender should return None for invalid values" {
                Expect.isNone (parseAdministrativeGender "invalid") "Should not parse invalid gender"
                Expect.isNone (parseAdministrativeGender "") "Should not parse empty string"
            }
            
            test "parseObservationStatus should parse valid statuses" {
                Expect.equal (parseObservationStatus "registered") (Some ObservationStatus.Registered) "Should parse registered"
                Expect.equal (parseObservationStatus "preliminary") (Some ObservationStatus.Preliminary) "Should parse preliminary"
                Expect.equal (parseObservationStatus "final") (Some ObservationStatus.Final) "Should parse final"
                Expect.equal (parseObservationStatus "amended") (Some ObservationStatus.Amended) "Should parse amended"
            }
            
            test "parseObservationStatus should be case insensitive" {
                Expect.equal (parseObservationStatus "FINAL") (Some ObservationStatus.Final) "Should parse uppercase"
                Expect.equal (parseObservationStatus "Preliminary") (Some ObservationStatus.Preliminary) "Should parse mixed case"
            }
            
            test "parseObservationStatus should return None for invalid values" {
                Expect.isNone (parseObservationStatus "invalid") "Should not parse invalid status"
                Expect.isNone (parseObservationStatus "") "Should not parse empty string"
            }
        ]
        
        testList "Patient Parsing Tests" [
            test "tryParsePatient should parse valid patient" {
                use doc = JsonDocument.Parse(samplePatientJson)
                let result = tryParsePatient doc.RootElement
                match result with
                | Ok patient ->
                    Expect.equal patient.resource.resource.id (Some "example") "Should have correct ID"
                    Expect.equal patient.active (Some true) "Should have correct active status"
                    Expect.isNonEmpty patient.name "Should have names"
                    Expect.equal patient.name.[0].family (Some "Chalmers") "Should have correct family name"
                    Expect.contains patient.name.[0].given "Jim" "Should have correct given name"
                | Error errors -> failtestf "Should parse valid patient: %A" errors
            }
            
            test "tryParsePatient should handle minimal patient" {
                let minimalPatient = """{"resourceType": "Patient"}"""
                use doc = JsonDocument.Parse(minimalPatient)
                let result = tryParsePatient doc.RootElement
                match result with
                | Ok patient ->
                    Expect.isNone patient.resource.resource.id "Should have no ID"
                    Expect.isNone patient.active "Should have no active status"
                    Expect.isEmpty patient.name "Should have no names"
                | Error errors -> failtestf "Should parse minimal patient: %A" errors
            }
            
            test "tryParsePatient should reject wrong resource type" {
                let wrongType = """{"resourceType": "Observation"}"""
                use doc = JsonDocument.Parse(wrongType)
                let result = tryParsePatient doc.RootElement
                match result with
                | Ok _ -> failtest "Should not parse wrong resource type"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("Expected resourceType 'Patient'")) "Should have resource type error"
            }
            
            test "tryParsePatient should reject missing resource type" {
                let noResourceType = """{"id": "test"}"""
                use doc = JsonDocument.Parse(noResourceType)
                let result = tryParsePatient doc.RootElement
                match result with
                | Ok _ -> failtest "Should not parse without resource type"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("Missing required 'resourceType' property")) "Should have missing resource type error"
            }
            
            test "tryParsePatient should parse complex names" {
                let complexNames = """
                {
                    "resourceType": "Patient",
                    "name": [
                        {
                            "family": "Doe",
                            "given": ["John", "Middle"],
                            "text": "John Middle Doe"
                        },
                        {
                            "family": "Smith",
                            "given": ["Jane"]
                        }
                    ]
                }
                """
                use doc = JsonDocument.Parse(complexNames)
                let result = tryParsePatient doc.RootElement
                match result with
                | Ok patient ->
                    Expect.hasLength patient.name 2 "Should have two names"
                    Expect.equal patient.name.[0].family (Some "Doe") "Should have first family name"
                    Expect.equal patient.name.[0].text (Some "John Middle Doe") "Should have first text"
                    Expect.hasLength patient.name.[0].given 2 "Should have two given names"
                    Expect.equal patient.name.[1].family (Some "Smith") "Should have second family name"
                | Error errors -> failtestf "Should parse complex names: %A" errors
            }
            
            test "tryParsePatient should parse gender and birthDate" {
                let patientWithDetails = """
                {
                    "resourceType": "Patient",
                    "gender": "male",
                    "birthDate": "1990-01-01T00:00:00Z"
                }
                """
                use doc = JsonDocument.Parse(patientWithDetails)
                let result = tryParsePatient doc.RootElement
                match result with
                | Ok patient ->
                    Expect.equal patient.gender (Some AdministrativeGender.Male) "Should have correct gender"
                    Expect.isSome patient.birthDate "Should have birth date"
                    match patient.birthDate with
                    | Some bd -> Expect.equal bd.Year 1990 "Should have correct birth year"
                    | None -> failtest "Should have birth date"
                | Error errors -> failtestf "Should parse patient with details: %A" errors
            }
        ]
        
        testList "Observation Parsing Tests" [
            test "tryParseObservation should parse valid observation" {
                use doc = JsonDocument.Parse(sampleObservationJson)
                let result = tryParseObservation doc.RootElement
                match result with
                | Ok observation ->
                    Expect.equal observation.resource.resource.id (Some "example") "Should have correct ID"
                    Expect.equal observation.status ObservationStatus.Final "Should have correct status"
                | Error errors -> failtestf "Should parse valid observation: %A" errors
            }
            
            test "tryParseObservation should handle minimal observation" {
                let minimalObservation = """
                {
                    "resourceType": "Observation",
                    "status": "final"
                }
                """
                use doc = JsonDocument.Parse(minimalObservation)
                let result = tryParseObservation doc.RootElement
                match result with
                | Ok observation ->
                    Expect.isNone observation.resource.resource.id "Should have no ID"
                    Expect.equal observation.status ObservationStatus.Final "Should have correct status"
                | Error errors -> failtestf "Should parse minimal observation: %A" errors
            }
            
            test "tryParseObservation should reject missing status" {
                let noStatus = """{"resourceType": "Observation"}"""
                use doc = JsonDocument.Parse(noStatus)
                let result = tryParseObservation doc.RootElement
                match result with
                | Ok _ -> failtest "Should not parse without status"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("Missing required 'status' property")) "Should have missing status error"
            }
            
            test "tryParseObservation should reject invalid status" {
                let invalidStatus = """
                {
                    "resourceType": "Observation",
                    "status": "invalid-status"
                }
                """
                use doc = JsonDocument.Parse(invalidStatus)
                let result = tryParseObservation doc.RootElement
                match result with
                | Ok _ -> failtest "Should not parse with invalid status"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("Invalid observation status")) "Should have invalid status error"
            }
            
            test "tryParseObservation should reject wrong resource type" {
                let wrongType = """{"resourceType": "Patient", "status": "final"}"""
                use doc = JsonDocument.Parse(wrongType)
                let result = tryParseObservation doc.RootElement
                match result with
                | Ok _ -> failtest "Should not parse wrong resource type"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("Expected resourceType 'Observation'")) "Should have resource type error"
            }
            
            test "tryParseObservation should parse effective date" {
                let observationWithDate = """
                {
                    "resourceType": "Observation",
                    "status": "final",
                    "effective": "2023-12-25T10:30:00Z"
                }
                """
                use doc = JsonDocument.Parse(observationWithDate)
                let result = tryParseObservation doc.RootElement
                match result with
                | Ok observation ->
                    Expect.isSome observation.effective "Should have effective date"
                    match observation.effective with
                    | Some dt -> Expect.equal dt.Year 2023 "Should have correct year"
                    | None -> failtest "Should have effective date"
                | Error errors -> failtestf "Should parse observation with date: %A" errors
            }
        ]
        
        testList "Resource Parsing Tests" [
            test "tryParseResource should parse Patient resource" {
                let result = tryParseResource samplePatientJson
                match result with
                | Ok (resourceType, resource) ->
                    Expect.equal resourceType "Patient" "Should identify as Patient"
                    Expect.isTrue (resource :? Patient) "Should be Patient type"
                | Error errors -> failtestf "Should parse Patient resource: %A" errors
            }
            
            test "tryParseResource should parse Observation resource" {
                let result = tryParseResource sampleObservationJson
                match result with
                | Ok (resourceType, resource) ->
                    Expect.equal resourceType "Observation" "Should identify as Observation"
                    Expect.isTrue (resource :? Observation) "Should be Observation type"
                | Error errors -> failtestf "Should parse Observation resource: %A" errors
            }
            
            test "tryParseResource should reject unsupported resource type" {
                let unsupportedResource = """{"resourceType": "Medication"}"""
                let result = tryParseResource unsupportedResource
                match result with
                | Ok _ -> failtest "Should not parse unsupported resource type"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("not yet supported")) "Should have unsupported type error"
            }
            
            test "tryParseResource should reject missing resource type" {
                let noResourceType = """{"id": "test"}"""
                let result = tryParseResource noResourceType
                match result with
                | Ok _ -> failtest "Should not parse without resource type"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("Missing required 'resourceType' property")) "Should have missing resource type error"
            }
            
            test "tryParseResource should handle invalid JSON" {
                let result = tryParseResource invalidJson
                match result with
                | Ok _ -> failtest "Should not parse invalid JSON"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("Invalid JSON")) "Should have JSON parsing error"
            }
            
            test "tryParseResource should handle empty string" {
                let result = tryParseResource ""
                match result with
                | Ok _ -> failtest "Should not parse empty string"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("Invalid JSON")) "Should have JSON parsing error"
            }
        ]
        
        testList "Error Handling Tests" [
            test "parsing should accumulate multiple errors" {
                let invalidObservation = """
                {
                    "resourceType": "Observation"
                }
                """
                use doc = JsonDocument.Parse(invalidObservation)
                let result = tryParseObservation doc.RootElement
                match result with
                | Ok _ -> failtest "Should not parse invalid observation"
                | Error errors -> 
                    Expect.isGreaterThan errors.Length 0 "Should have at least one error"
                    Expect.exists errors (fun e -> e.Contains("Missing required 'status' property")) "Should have status error"
            }
            
            test "parsing should handle malformed JSON gracefully" {
                let malformedJson = """{"resourceType": "Patient", "malformed": }"""
                let result = tryParseResource malformedJson
                match result with
                | Ok _ -> failtest "Should not parse malformed JSON"
                | Error errors -> 
                    Expect.exists errors (fun e -> e.Contains("Invalid JSON")) "Should have JSON error"
            }
            
            test "parsing should handle null values gracefully" {
                let nullValues = """
                {
                    "resourceType": "Patient",
                    "active": null,
                    "name": null,
                    "gender": null
                }
                """
                let result = tryParseResource nullValues
                match result with
                | Ok ("Patient", (:? Patient as patient)) ->
                    Expect.isNone patient.active "Should handle null active"
                    Expect.isEmpty patient.name "Should handle null name array"
                    Expect.isNone patient.gender "Should handle null gender"
                | _ -> failtest "Should parse patient with null values"
            }
        ]
        
        testList "Edge Cases Tests" [
            test "parsing should handle very large JSON" {
                let largeNames = 
                    [1..100] 
                    |> List.map (fun i -> sprintf "{\"family\": \"Family%d\", \"given\": [\"Given%d\"]}" i i)
                    |> String.concat ","
                let largePatient = sprintf "{\"resourceType\": \"Patient\", \"name\": [%s]}" largeNames
                
                let result = tryParseResource largePatient
                match result with
                | Ok ("Patient", (:? Patient as patient)) ->
                    Expect.hasLength patient.name 100 "Should parse all 100 names"
                | _ -> failtest "Should parse large patient"
            }
            
            test "parsing should handle empty arrays" {
                let emptyArrays = """
                {
                    "resourceType": "Patient",
                    "name": [],
                    "identifier": []
                }
                """
                let result = tryParseResource emptyArrays
                match result with
                | Ok ("Patient", (:? Patient as patient)) ->
                    Expect.isEmpty patient.name "Should handle empty name array"
                    Expect.isEmpty patient.identifier "Should handle empty identifier array"
                | _ -> failtest "Should parse patient with empty arrays"
            }
            
            test "parsing should handle special characters in strings" {
                let specialChars = """
                {
                    "resourceType": "Patient",
                    "name": [
                        {
                            "family": "O'Connor",
                            "given": ["José", "François"],
                            "text": "José François O'Connor"
                        }
                    ]
                }
                """
                let result = tryParseResource specialChars
                match result with
                | Ok ("Patient", (:? Patient as patient)) ->
                    Expect.equal patient.name.[0].family (Some "O'Connor") "Should handle apostrophe"
                    Expect.contains patient.name.[0].given "José" "Should handle accented characters"
                    Expect.contains patient.name.[0].given "François" "Should handle cedilla"
                | _ -> failtest "Should parse patient with special characters"
            }
        ]
        
        testList "Performance Tests" [
            test "parsing should complete within reasonable time" {
                let stopwatch = System.Diagnostics.Stopwatch.StartNew()
                let result = tryParseResource samplePatientJson
                stopwatch.Stop()
                Expect.isLessThan stopwatch.ElapsedMilliseconds 100L "Parsing should complete within 100ms"
                match result with
                | Ok _ -> () // Success expected
                | Error errors -> failtestf "Should parse successfully: %A" errors
            }
            
            test "multiple parsing operations should be efficient" {
                let stopwatch = System.Diagnostics.Stopwatch.StartNew()
                for _ in 1..100 do
                    let _ = tryParseResource samplePatientJson
                    ()
                stopwatch.Stop()
                Expect.isLessThan stopwatch.ElapsedMilliseconds 1000L "100 parsing operations should complete within 1 second"
            }
        ]
    ]