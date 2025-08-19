module ValidatorTests

open Expecto
open FSharpFHIR.Validator
open TestHelpers
open System.IO
open System.Text.Json

[<Tests>]
let validatorTests =
    testList "Validator Tests" [
        
        testList "Basic Validation Tests" [
            test "validateBasic should pass for valid JSON with resourceType" {
                let json = """{"resourceType": "Patient", "id": "test"}"""
                let result = validateBasic json
                Expect.isEmpty result "Should have no validation errors"
            }
            
            test "validateBasic should fail for JSON without resourceType" {
                let json = """{"id": "test", "name": "John"}"""
                let result = validateBasic json
                Expect.hasLength result 1 "Should have one validation error"
                Expect.stringContains result.[0] "Missing required 'resourceType' property" "Should contain correct error message"
            }
            
            test "validateBasic should fail for invalid JSON" {
                let json = """{invalid json}"""
                let result = validateBasic json
                Expect.hasLength result 1 "Should have one validation error"
                Expect.stringContains result.[0] "Invalid JSON" "Should contain JSON error message"
            }
            
            test "validateBasic should pass for empty resourceType" {
                let json = """{"resourceType": "", "id": "test"}"""
                let result = validateBasic json
                Expect.isEmpty result "Should have no validation errors even with empty resourceType"
            }
        ]
        
        testList "Enhanced Validation Tests" [
            test "validate should pass for valid Patient resource" {
                let result = validate samplePatientJson
                Expect.isEmpty result "Should have no validation errors for valid Patient"
            }
            
            test "validate should pass for valid Observation resource" {
                let result = validate sampleObservationJson
                Expect.isEmpty result "Should have no validation errors for valid Observation"
            }
            
            test "validate should fail for invalid JSON structure" {
                let result = validate invalidJson
                Expect.isNonEmpty result "Should have validation errors for invalid JSON"
            }
            
            test "validate should fail for missing required fields" {
                let json = """{"resourceType": "Patient"}"""
                let result = validate json
                Expect.isNonEmpty result "Should have validation errors for incomplete Patient"
            }
            
            test "validate should fail for unknown resource type" {
                let json = """{"resourceType": "UnknownResource", "id": "test"}"""
                let result = validate json
                Expect.isNonEmpty result "Should have validation errors for unknown resource type"
            }
            
            test "validate should handle malformed JSON gracefully" {
                let json = """{"resourceType": "Patient", "active": "not-a-boolean"}"""
                let result = validate json
                Expect.isNonEmpty result "Should have validation errors for type mismatch"
            }
        ]
        
        testList "File Validation Tests" [
            test "validateFile should fail for non-existent file" {
                let result = validateFile "non-existent-file.json"
                Expect.hasLength result 1 "Should have one validation error"
                Expect.stringContains result.[0] "does not exist" "Should contain file not found error"
            }
            
            testAsync "validateFile should validate existing valid file" {
                let tempFile = Path.GetTempFileName()
                try
                    do! File.WriteAllTextAsync(tempFile, samplePatientJson) |> Async.AwaitTask
                    let result = validateFile tempFile
                    Expect.isEmpty result "Should have no validation errors for valid file"
                finally
                    if File.Exists tempFile then File.Delete tempFile
            }
            
            testAsync "validateFile should fail for file with invalid content" {
                let tempFile = Path.GetTempFileName()
                try
                    do! File.WriteAllTextAsync(tempFile, invalidJson) |> Async.AwaitTask
                    let result = validateFile tempFile
                    Expect.isNonEmpty result "Should have validation errors for invalid file content"
                finally
                    if File.Exists tempFile then File.Delete tempFile
            }
        ]
        
        testList "Resource Info Tests" [
            test "getResourceInfo should return Patient info for valid Patient" {
                match getResourceInfo samplePatientJson with
                | Ok (resourceType, info) ->
                    Expect.equal resourceType "Patient" "Should identify resource type as Patient"
                    Expect.stringContains info "Patient resource" "Should contain Patient in info"
                    Expect.stringContains info "name" "Should contain name information"
                | Error _ -> failtest "Should successfully parse valid Patient"
            }
            
            test "getResourceInfo should return Observation info for valid Observation" {
                match getResourceInfo sampleObservationJson with
                | Ok (resourceType, info) ->
                    Expect.equal resourceType "Observation" "Should identify resource type as Observation"
                    Expect.stringContains info "Observation resource" "Should contain Observation in info"
                    Expect.stringContains info "status" "Should contain status information"
                | Error _ -> failtest "Should successfully parse valid Observation"
            }
            
            test "getResourceInfo should fail for invalid JSON" {
                match getResourceInfo invalidJson with
                | Ok _ -> failtest "Should not successfully parse invalid JSON"
                | Error errors -> Expect.isNonEmpty errors "Should return validation errors"
            }
            
            test "getResourceInfo should handle unknown resource types" {
                let json = """{"resourceType": "CustomResource", "id": "test"}"""
                match getResourceInfo json with
                | Ok (resourceType, info) ->
                    Expect.equal resourceType "CustomResource" "Should identify custom resource type"
                    Expect.stringContains info "CustomResource resource" "Should contain custom resource type in info"
                | Error _ -> failtest "Should handle unknown resource types gracefully"
            }
        ]
        
        testList "Edge Cases and Error Handling" [
            test "validate should handle empty string" {
                let result = validate ""
                Expect.isNonEmpty result "Should have validation errors for empty string"
            }
            
            test "validate should handle null-like JSON" {
                let json = "null"
                let result = validate json
                Expect.isNonEmpty result "Should have validation errors for null JSON"
            }
            
            test "validate should handle array JSON" {
                let json = "[{\"resourceType\": \"Patient\"}]"
                let result = validate json
                Expect.isNonEmpty result "Should have validation errors for array JSON"
            }
            
            test "validate should handle very large JSON" {
                let largeJson = 
                    let baseJson = samplePatientJson.TrimEnd([|'}'; ' '; '\n'; '\r'|])
                    let largeField = String.replicate 1000 "x"
                    sprintf "%s, \"largeField\": \"%s\"}" baseJson largeField
                let result = validate largeJson
                // Should either pass or fail gracefully, not crash
                Expect.isTrue true "Should handle large JSON without crashing"
            }
            
            test "validateBasic should handle deeply nested JSON" {
                let nestedJson = """{"resourceType": "Patient", "nested": {"level1": {"level2": {"level3": "value"}}}}"""
                let result = validateBasic nestedJson
                Expect.isEmpty result "Should handle deeply nested JSON"
            }
        ]
        
        testList "Performance Tests" [
            test "validate should complete within reasonable time" {
                let stopwatch = System.Diagnostics.Stopwatch.StartNew()
                let result = validate samplePatientJson
                stopwatch.Stop()
                Expect.isLessThan stopwatch.ElapsedMilliseconds 1000L "Validation should complete within 1 second"
            }
            
            test "validateBasic should be faster than enhanced validation" {
                let json = samplePatientJson
                
                let basicTime = measureExecutionTime (fun () -> validateBasic json)
                let enhancedTime = measureExecutionTime (fun () -> validate json)
                
                Expect.isLessThan basicTime enhancedTime "Basic validation should be faster than enhanced validation"
            }
        ]
    ]