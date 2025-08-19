module IntegrationTests

open Expecto
open FSharpFHIR
open FSharpFHIR.Validator
open FSharpFHIR.Visualizer
open FSharpFHIR.QueryEngine
open TestHelpers
open System
open System.IO
open System.Text.Json
open System.Diagnostics

// Helper function to create temporary test files
let createTempFile content =
    let tempPath = Path.GetTempFileName()
    File.WriteAllText(tempPath, content)
    tempPath

// Helper function to clean up temporary files
let cleanupTempFile path =
    if File.Exists(path) then
        File.Delete(path)

// Helper function to run CLI commands programmatically
let runCliCommand args =
    try
        // Simulate CLI command execution by calling the main functions directly
        match args with
        | ["validate"; filePath] ->
            if not (File.Exists(filePath)) then
                (1, ["Error: File '" + filePath + "' not found"], [])
            else
                let json = File.ReadAllText(filePath)
                let errors = Validator.validate json
                if errors.IsEmpty then
                    (0, ["✓ Valid FHIR resource"], [])
                else
                    let errorLines = "✗ Validation failed:" :: (errors |> List.map (fun e -> "  - " + e))
                    (1, errorLines, [])
        
        | ["visualize"; filePath] ->
            if not (File.Exists(filePath)) then
                (1, ["Error: File '" + filePath + "' not found"], [])
            else
                let json = File.ReadAllText(filePath)
                let visualization = Visualizer.visualizeJson json Visualizer.VisualizationFormat.Tree
                (0, visualization, [])
        
        | ["query"; filePath; queryStr] ->
            if not (File.Exists(filePath)) then
                (1, ["Error: File '" + filePath + "' not found"], [])
            else
                let json = File.ReadAllText(filePath)
                try
                    let parseResult = QueryParser.parse queryStr
                    match parseResult with
                    | Ok expr ->
                        let jsonDoc = JsonDocument.Parse(json)
                        let queryResult = QueryExecutor.executeOnElement expr jsonDoc.RootElement
                        jsonDoc.Dispose()
                        match queryResult with
                        | QueryResult.Single element -> (0, [element], [])
                        | QueryResult.Multiple elements -> (0, elements, [])
                        | QueryResult.Empty -> (0, ["null"], [])
                    | Error msg -> (1, ["Query error: " + msg], [])
                with
                | ex -> (1, ["Query error: " + ex.Message], [])
        
        | _ -> (1, ["Invalid arguments"], [])
    with
    | ex -> (1, ["Error: " + ex.Message], [])

[<Tests>]
let integrationTests =
    testList "Integration Tests" [
        
        testList "CLI Validate Command Tests" [
            test "validate command with valid Patient resource should succeed" {
                let validPatient = """
                {
                    "resourceType": "Patient",
                    "id": "test-patient",
                    "active": true,
                    "name": [{
                        "use": "official",
                        "family": "Doe",
                        "given": ["John"]
                    }],
                    "gender": "male",
                    "birthDate": "1990-01-01"
                }
                """
                
                let tempFile = createTempFile validPatient
                try
                    let (exitCode, output, _) = runCliCommand ["validate"; tempFile]
                    Expect.equal exitCode 0 "Should exit with success code"
                    Expect.contains output "✓ Valid FHIR resource" "Should indicate valid resource"
                finally
                    cleanupTempFile tempFile
            }
            
            test "validate command with invalid JSON should fail" {
                let invalidJson = "{ invalid json }"
                
                let tempFile = createTempFile invalidJson
                try
                    let (exitCode, output, _) = runCliCommand ["validate"; tempFile]
                    Expect.equal exitCode 1 "Should exit with error code"
                    Expect.exists output (fun line -> line.Contains("Error:")) "Should contain error message"
                finally
                    cleanupTempFile tempFile
            }
            
            test "validate command with missing resourceType should fail" {
                let invalidResource = """
                {
                    "id": "test",
                    "active": true
                }
                """
                
                let tempFile = createTempFile invalidResource
                try
                    let (exitCode, output, _) = runCliCommand ["validate"; tempFile]
                    Expect.equal exitCode 1 "Should exit with error code"
                    Expect.contains output "✗ Validation failed:" "Should indicate validation failure"
                finally
                    cleanupTempFile tempFile
            }
            
            test "validate command with non-existent file should fail" {
                let (exitCode, output, _) = runCliCommand ["validate"; "non-existent-file.json"]
                Expect.equal exitCode 1 "Should exit with error code"
                Expect.exists output (fun line -> line.Contains("not found")) "Should indicate file not found"
            }
        ]
        
        testList "CLI Visualize Command Tests" [
            test "visualize command with valid Patient should produce output" {
                let validPatient = """
                {
                    "resourceType": "Patient",
                    "id": "test-patient",
                    "name": [{
                        "family": "Doe",
                        "given": ["John"]
                    }]
                }
                """
                
                let tempFile = createTempFile validPatient
                try
                    let (exitCode, output, _) = runCliCommand ["visualize"; tempFile]
                    Expect.equal exitCode 0 "Should exit with success code"
                    Expect.isNonEmpty output "Should produce visualization output"
                    Expect.exists output (fun line -> line.Contains("Patient")) "Should contain Patient information"
                finally
                    cleanupTempFile tempFile
            }
            
            test "visualize command with valid Observation should produce output" {
                let validObservation = """
                {
                    "resourceType": "Observation",
                    "id": "test-obs",
                    "status": "final",
                    "effectiveDateTime": "2023-12-25T10:30:00Z"
                }
                """
                
                let tempFile = createTempFile validObservation
                try
                    let (exitCode, output, _) = runCliCommand ["visualize"; tempFile]
                    Expect.equal exitCode 0 "Should exit with success code"
                    Expect.isNonEmpty output "Should produce visualization output"
                    Expect.exists output (fun line -> line.Contains("Observation")) "Should contain Observation information"
                finally
                    cleanupTempFile tempFile
            }
            
            test "visualize command with invalid JSON should fail" {
                let invalidJson = "{ invalid json }"
                
                let tempFile = createTempFile invalidJson
                try
                    let (exitCode, output, _) = runCliCommand ["visualize"; tempFile]
                    Expect.equal exitCode 1 "Should exit with error code"
                    Expect.exists output (fun line -> line.Contains("Error:")) "Should contain error message"
                finally
                    cleanupTempFile tempFile
            }
            
            test "visualize command with non-existent file should fail" {
                let (exitCode, output, _) = runCliCommand ["visualize"; "non-existent-file.json"]
                Expect.equal exitCode 1 "Should exit with error code"
                Expect.exists output (fun line -> line.Contains("not found")) "Should indicate file not found"
            }
        ]
        
        testList "CLI Query Command Tests" [
            test "query command with identity query should return original" {
                let validPatient = """
                {
                    "resourceType": "Patient",
                    "id": "test-patient",
                    "name": [{
                        "family": "Doe",
                        "given": ["John"]
                    }]
                }
                """
                
                let tempFile = createTempFile validPatient
                try
                    let (exitCode, output, _) = runCliCommand ["query"; tempFile; "."]
                    Expect.equal exitCode 0 "Should exit with success code"
                    Expect.isNonEmpty output "Should produce query output"
                    let outputStr = String.concat "\n" output
                    Expect.stringContains outputStr "Patient" "Should contain Patient data"
                finally
                    cleanupTempFile tempFile
            }
            
            test "query command with property access should return property" {
                let validPatient = """
                {
                    "resourceType": "Patient",
                    "id": "test-patient",
                    "active": true
                }
                """
                
                let tempFile = createTempFile validPatient
                try
                    let (exitCode, output, _) = runCliCommand ["query"; tempFile; ".id"]
                    Expect.equal exitCode 0 "Should exit with success code"
                    Expect.contains output "test-patient" "Should return the ID value"
                finally
                    cleanupTempFile tempFile
            }
            
            test "query command with array access should return array element" {
                let validPatient = """
                {
                    "resourceType": "Patient",
                    "name": [{
                        "family": "Doe",
                        "given": ["John"]
                    }]
                }
                """
                
                let tempFile = createTempFile validPatient
                try
                    let (exitCode, output, _) = runCliCommand ["query"; tempFile; ".name[0].family"]
                    Expect.equal exitCode 0 "Should exit with success code"
                    Expect.contains output "Doe" "Should return the family name"
                finally
                    cleanupTempFile tempFile
            }
            
            test "query command with invalid query should fail" {
                let validPatient = """
                {
                    "resourceType": "Patient",
                    "id": "test-patient"
                }
                """
                
                let tempFile = createTempFile validPatient
                try
                    let (exitCode, output, _) = runCliCommand ["query"; tempFile; "invalid query syntax"]
                    Expect.equal exitCode 1 "Should exit with error code"
                    Expect.exists output (fun line -> line.Contains("Query error:")) "Should contain query error message"
                finally
                    cleanupTempFile tempFile
            }
            
            test "query command with non-existent property should return null" {
                let validPatient = """
                {
                    "resourceType": "Patient",
                    "id": "test-patient"
                }
                """
                
                let tempFile = createTempFile validPatient
                try
                    let (exitCode, output, _) = runCliCommand ["query"; tempFile; ".nonExistentProperty"]
                    Expect.equal exitCode 0 "Should exit with success code"
                    Expect.contains output "null" "Should return null for non-existent property"
                finally
                    cleanupTempFile tempFile
            }
            
            test "query command with non-existent file should fail" {
                let (exitCode, output, _) = runCliCommand ["query"; "non-existent-file.json"; "."]
                Expect.equal exitCode 1 "Should exit with error code"
                Expect.exists output (fun line -> line.Contains("not found")) "Should indicate file not found"
            }
        ]
        
        testList "End-to-End Workflow Tests" [
            test "complete workflow: validate, visualize, and query Patient" {
                let validPatient = """
                {
                    "resourceType": "Patient",
                    "id": "workflow-test",
                    "active": true,
                    "name": [{
                        "use": "official",
                        "family": "Smith",
                        "given": ["Jane", "Marie"]
                    }],
                    "gender": "female",
                    "birthDate": "1985-03-15"
                }
                """
                
                let tempFile = createTempFile validPatient
                try
                    // Step 1: Validate
                    let (validateExitCode, validateOutput, _) = runCliCommand ["validate"; tempFile]
                    Expect.equal validateExitCode 0 "Validation should succeed"
                    Expect.contains validateOutput "✓ Valid FHIR resource" "Should be valid"
                    
                    // Step 2: Visualize
                    let (visualizeExitCode, visualizeOutput, _) = runCliCommand ["visualize"; tempFile]
                    Expect.equal visualizeExitCode 0 "Visualization should succeed"
                    Expect.isNonEmpty visualizeOutput "Should produce visualization"
                    
                    // Step 3: Query for ID
                    let (queryIdExitCode, queryIdOutput, _) = runCliCommand ["query"; tempFile; ".id"]
                    Expect.equal queryIdExitCode 0 "ID query should succeed"
                    Expect.contains queryIdOutput "workflow-test" "Should return correct ID"
                    
                    // Step 4: Query for name
                    let (queryNameExitCode, queryNameOutput, _) = runCliCommand ["query"; tempFile; ".name[0].family"]
                    Expect.equal queryNameExitCode 0 "Name query should succeed"
                    Expect.contains queryNameOutput "Smith" "Should return correct family name"
                    
                    // Step 5: Query for gender
                    let (queryGenderExitCode, queryGenderOutput, _) = runCliCommand ["query"; tempFile; ".gender"]
                    Expect.equal queryGenderExitCode 0 "Gender query should succeed"
                    Expect.contains queryGenderOutput "female" "Should return correct gender"
                    
                finally
                    cleanupTempFile tempFile
            }
            
            test "complete workflow: validate, visualize, and query Observation" {
                let validObservation = """
                {
                    "resourceType": "Observation",
                    "id": "obs-workflow-test",
                    "status": "final",
                    "effectiveDateTime": "2023-12-25T10:30:00Z",
                    "valueQuantity": {
                        "value": 180.5,
                        "unit": "cm",
                        "system": "http://unitsofmeasure.org",
                        "code": "cm"
                    }
                }
                """
                
                let tempFile = createTempFile validObservation
                try
                    // Step 1: Validate
                    let (validateExitCode, validateOutput, _) = runCliCommand ["validate"; tempFile]
                    Expect.equal validateExitCode 0 "Validation should succeed"
                    Expect.contains validateOutput "✓ Valid FHIR resource" "Should be valid"
                    
                    // Step 2: Visualize
                    let (visualizeExitCode, visualizeOutput, _) = runCliCommand ["visualize"; tempFile]
                    Expect.equal visualizeExitCode 0 "Visualization should succeed"
                    Expect.isNonEmpty visualizeOutput "Should produce visualization"
                    
                    // Step 3: Query for status
                    let (queryStatusExitCode, queryStatusOutput, _) = runCliCommand ["query"; tempFile; ".status"]
                    Expect.equal queryStatusExitCode 0 "Status query should succeed"
                    Expect.contains queryStatusOutput "final" "Should return correct status"
                    
                    // Step 4: Query for value
                    let (queryValueExitCode, queryValueOutput, _) = runCliCommand ["query"; tempFile; ".valueQuantity.value"]
                    Expect.equal queryValueExitCode 0 "Value query should succeed"
                    Expect.contains queryValueOutput "180.5" "Should return correct value"
                    
                finally
                    cleanupTempFile tempFile
            }
            
            test "error handling workflow: invalid resource through all commands" {
                let invalidResource = """
                {
                    "resourceType": "InvalidType",
                    "id": "invalid-test"
                }
                """
                
                let tempFile = createTempFile invalidResource
                try
                    // Validation should fail
                    let (validateExitCode, validateOutput, _) = runCliCommand ["validate"; tempFile]
                    Expect.equal validateExitCode 1 "Validation should fail"
                    Expect.contains validateOutput "✗ Validation failed:" "Should indicate validation failure"
                    
                    // Visualization should still work (it doesn't validate)
                    let (visualizeExitCode, visualizeOutput, _) = runCliCommand ["visualize"; tempFile]
                    Expect.equal visualizeExitCode 0 "Visualization should succeed even with invalid resource"
                    
                    // Query should work on the JSON structure
                    let (queryExitCode, queryOutput, _) = runCliCommand ["query"; tempFile; ".resourceType"]
                    Expect.equal queryExitCode 0 "Query should succeed on JSON structure"
                    Expect.contains queryOutput "InvalidType" "Should return the resource type"
                    
                finally
                    cleanupTempFile tempFile
            }
        ]
        
        testList "Performance Integration Tests" [
            test "large Patient resource processing should complete in reasonable time" {
                let largePatient = """
                {
                    "resourceType": "Patient",
                    "id": "large-patient",
                    "active": true,
                    "name": [
                        {"use": "official", "family": "Family1", "given": ["Given1"]},
                        {"use": "official", "family": "Family2", "given": ["Given2"]},
                        {"use": "official", "family": "Family3", "given": ["Given3"]},
                        {"use": "official", "family": "Family4", "given": ["Given4"]},
                        {"use": "official", "family": "Family5", "given": ["Given5"]}
                    ],
                    "gender": "male",
                    "birthDate": "1990-01-01"
                }
                """
                
                let tempFile = createTempFile largePatient
                try
                    let stopwatch = Diagnostics.Stopwatch.StartNew()
                    
                    // Test validation performance
                    let (validateExitCode, _, _) = runCliCommand ["validate"; tempFile]
                    let validateTime = stopwatch.ElapsedMilliseconds
                    stopwatch.Restart()
                    
                    // Test visualization performance
                    let (visualizeExitCode, _, _) = runCliCommand ["visualize"; tempFile]
                    let visualizeTime = stopwatch.ElapsedMilliseconds
                    stopwatch.Restart()
                    
                    // Test query performance
                    let (queryExitCode, _, _) = runCliCommand ["query"; tempFile; ".name | length"]
                    let queryTime = stopwatch.ElapsedMilliseconds
                    
                    stopwatch.Stop()
                    
                    Expect.equal validateExitCode 0 "Large resource validation should succeed"
                    Expect.equal visualizeExitCode 0 "Large resource visualization should succeed"
                    Expect.equal queryExitCode 0 "Large resource query should succeed"
                    
                    // Performance expectations (adjust as needed)
                    Expect.isLessThan validateTime 5000L "Validation should complete within 5 seconds"
                    Expect.isLessThan visualizeTime 5000L "Visualization should complete within 5 seconds"
                    Expect.isLessThan queryTime 5000L "Query should complete within 5 seconds"
                    
                finally
                    cleanupTempFile tempFile
            }
        ]
        
        testList "Module Integration Tests" [
            test "Validator and Visualizer integration" {
                let validPatient = """
                {
                    "resourceType": "Patient",
                    "id": "integration-test",
                    "name": [{
                        "family": "Integration",
                        "given": ["Test"]
                    }]
                }
                """
                
                // Test that a resource that passes validation also visualizes correctly
                let errors = Validator.validate validPatient
                Expect.isEmpty errors "Should be valid for integration test"
                
                let visualization = Visualizer.visualizeJson validPatient Visualizer.VisualizationFormat.Tree
                Expect.isNonEmpty visualization "Should produce visualization output"
                Expect.exists visualization (fun line -> line.Contains("Patient")) "Should contain Patient information"
            }
            
            test "QueryEngine and Validator integration" {
                let validObservation = """
                {
                    "resourceType": "Observation",
                    "id": "query-integration",
                    "status": "final"
                }
                """
                
                // Test that a resource that passes validation also queries correctly
                let errors = Validator.validate validObservation
                Expect.isEmpty errors "Should be valid for query integration test"
                
                let parseResult = QueryParser.parse ".status"
                match parseResult with
                | Ok expr ->
                    let jsonDoc = JsonDocument.Parse(validObservation)
                    let queryResult = QueryExecutor.executeOnElement expr jsonDoc.RootElement
                    jsonDoc.Dispose()
                    match queryResult with
                    | QueryResult.Single result -> 
                        Expect.stringContains result "final" "Should query the status correctly"
                    | _ -> failtest "Query should succeed and return single result"
                | Error msg -> failtest ("Query parsing failed: " + msg)
            }
            
            test "All modules integration with complex resource" {
                let complexPatient = """
                {
                    "resourceType": "Patient",
                    "id": "complex-integration",
                    "active": true,
                    "name": [{
                        "use": "official",
                        "family": "Complex",
                        "given": ["Integration", "Test"]
                    }],
                    "gender": "other",
                    "birthDate": "1995-06-20",
                    "identifier": [{
                        "use": "official",
                        "system": "http://example.com/ids",
                        "value": "12345"
                    }]
                }
                """
                
                // Step 1: Validate
                let errors = Validator.validate complexPatient
                Expect.isEmpty errors "Complex patient should be valid"
                
                // Step 2: Visualize
                let visualization = Visualizer.visualizeJson complexPatient Visualizer.VisualizationFormat.Tree
                Expect.isNonEmpty visualization "Should produce visualization"
                Expect.exists visualization (fun line -> line.Contains("Complex")) "Should contain family name"
                
                // Step 3: Query various properties
                let executeQuery queryStr expectedValue description =
                    let parseResult = QueryParser.parse queryStr
                    match parseResult with
                    | Ok expr ->
                        let jsonDoc = JsonDocument.Parse(complexPatient)
                        let queryResult = QueryExecutor.executeOnElement expr jsonDoc.RootElement
                        jsonDoc.Dispose()
                        match queryResult with
                        | QueryResult.Single result -> Expect.stringContains result expectedValue description
                        | _ -> failtest (description + " should succeed")
                    | Error msg -> failtest ("Query parsing failed: " + msg)
                
                executeQuery ".id" "complex-integration" "Should query ID correctly"
                executeQuery ".name[0].family" "Complex" "Should query name correctly"
                executeQuery ".gender" "other" "Should query gender correctly"
                executeQuery ".identifier[0].value" "12345" "Should query identifier correctly"
            }
        ]
    ]