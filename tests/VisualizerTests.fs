module VisualizerTests

open Expecto
open FSharpFHIR.Visualizer
open FSharpFHIR.Types
open TestHelpers
open System.IO
open System

[<Tests>]
let visualizerTests =
    testList "Visualizer Tests" [
        
        testList "Color Module Tests" [
            test "Colors should have proper ANSI codes" {
                Expect.equal Colors.reset "\u001b[0m" "Reset should have correct ANSI code"
                Expect.equal Colors.bold "\u001b[1m" "Bold should have correct ANSI code"
                Expect.equal Colors.dim "\u001b[2m" "Dim should have correct ANSI code"
                Expect.isNonEmpty Colors.property "Property color should not be empty"
                Expect.isNonEmpty Colors.stringValue "String value color should not be empty"
                Expect.isNonEmpty Colors.numberValue "Number value color should not be empty"
            }
        ]
        
        testList "TreeChars Module Tests" [
            test "TreeChars should have proper Unicode characters" {
                Expect.equal TreeChars.vertical "│" "Vertical should be correct Unicode"
                Expect.equal TreeChars.branch "├─" "Branch should be correct Unicode"
                Expect.equal TreeChars.lastBranch "└─" "Last branch should be correct Unicode"
                Expect.equal TreeChars.space "  " "Space should be two spaces"
                Expect.equal TreeChars.continuation "│ " "Continuation should be correct"
            }
        ]
        
        testList "Formatting Helper Tests" [
            test "formatString should wrap string in quotes and colors" {
                let result = formatString "test"
                Expect.stringContains result "\"test\"" "Should contain quoted string"
                Expect.stringContains result Colors.stringValue "Should contain string color"
                Expect.stringContains result Colors.reset "Should contain reset code"
            }
            
            test "formatBool should format boolean with colors" {
                let trueResult = formatBool true
                let falseResult = formatBool false
                Expect.stringContains trueResult "true" "Should contain true"
                Expect.stringContains falseResult "false" "Should contain false"
                Expect.stringContains trueResult Colors.boolValue "Should contain bool color"
                Expect.stringContains trueResult Colors.reset "Should contain reset code"
            }
            
            test "formatNumber should format number with colors" {
                let result = formatNumber 42
                Expect.stringContains result "42" "Should contain number"
                Expect.stringContains result Colors.numberValue "Should contain number color"
                Expect.stringContains result Colors.reset "Should contain reset code"
            }
            
            test "formatDateTime should format date properly" {
                let date = DateTime(2023, 12, 25, 10, 30, 0)
                let result = formatDateTime date
                Expect.stringContains result "2023-12-25" "Should contain date"
                Expect.stringContains result "10:30:00" "Should contain time"
                Expect.stringContains result Colors.dateValue "Should contain date color"
            }
            
            test "formatEnum should format enum with colors" {
                let result = formatEnum "Active"
                Expect.stringContains result "Active" "Should contain enum value"
                Expect.stringContains result Colors.enumValue "Should contain enum color"
                Expect.stringContains result Colors.reset "Should contain reset code"
            }
            
            test "formatProperty should format property name with colors" {
                let result = formatProperty "name"
                Expect.stringContains result "name" "Should contain property name"
                Expect.stringContains result Colors.property "Should contain property color"
                Expect.stringContains result Colors.reset "Should contain reset code"
            }
            
            test "formatType should format type name with colors" {
                let result = formatType "Patient"
                Expect.stringContains result "Patient" "Should contain type name"
                Expect.stringContains result Colors.typeName "Should contain type color"
                Expect.stringContains result Colors.reset "Should contain reset code"
            }
        ]
        
        testList "Option Formatting Tests" [
            test "formatOption should handle Some value" {
                let result = formatOption formatString (Some "test")
                Expect.stringContains result "test" "Should contain the value"
                Expect.stringContains result Colors.stringValue "Should use formatter colors"
            }
            
            test "formatOption should handle None value" {
                let result = formatOption formatString None
                Expect.stringContains result "None" "Should contain None text"
                Expect.stringContains result Colors.none "Should use none color"
            }
        ]
        
        testList "List Formatting Tests" [
            test "formatList should handle empty list" {
                let result = formatList (fun _ -> ["item"]) [] "" false
                Expect.hasLength result 1 "Should have one line for empty list"
                Expect.stringContains result.[0] "[]" "Should show empty brackets"
            }
            
            test "formatList should format non-empty list with indices" {
                let items = ["item1"; "item2"]
                let formatter = fun item -> [item]
                let result = formatList formatter items "" false
                Expect.isGreaterThan result.Length 3 "Should have multiple lines"
                Expect.exists result (fun line -> line.Contains("2 items")) "Should show item count"
                Expect.exists result (fun line -> line.Contains("[0]")) "Should show first index"
                Expect.exists result (fun line -> line.Contains("[1]")) "Should show second index"
            }
        ]
        
        testList "HumanName Formatting Tests" [
            test "formatHumanName should format complete name" {
                let name = {
                    ``use`` = Some NameUse.Official
                    text = Some "John Doe"
                    family = Some "Doe"
                    given = ["John"; "Middle"]
                    prefix = []
                    suffix = []
                    period = None
                }
                let result = formatHumanName name
                Expect.isNonEmpty result "Should return formatted lines"
                Expect.exists result (fun line -> line.Contains("HumanName")) "Should contain type name"
                Expect.exists result (fun line -> line.Contains("use")) "Should contain use property"
                Expect.exists result (fun line -> line.Contains("John Doe")) "Should contain text"
                Expect.exists result (fun line -> line.Contains("Doe")) "Should contain family name"
            }
            
            test "formatHumanName should handle minimal name" {
                let name = {
                    ``use`` = None
                    text = None
                    family = None
                    given = []
                    prefix = []
                    suffix = []
                    period = None
                }
                let result = formatHumanName name
                Expect.isNonEmpty result "Should return formatted lines even for minimal name"
                Expect.exists result (fun line -> line.Contains("None")) "Should show None for missing values"
            }
        ]
        
        testList "Identifier Formatting Tests" [
            test "formatIdentifier should format complete identifier" {
                let identifier = {
                    ``use`` = Some IdentifierUse.Official
                    system = Some (Uri("http://example.com"))
                    value = Some "12345"
                    period = None
                    assigner = None
                }
                let result = formatIdentifier identifier
                Expect.isNonEmpty result "Should return formatted lines"
                Expect.exists result (fun line -> line.Contains("Identifier")) "Should contain type name"
                Expect.exists result (fun line -> line.Contains("use")) "Should contain use property"
                Expect.exists result (fun line -> line.Contains("system")) "Should contain system property"
                Expect.exists result (fun line -> line.Contains("12345")) "Should contain value"
            }
        ]
        
        testList "Visualization Format Tests" [
            test "Patient compact format should be single line" {
                match JsonSerialization.tryParseResource samplePatientJson with
                | Ok ("Patient", (:? Patient as patient)) ->
                    let result = formatPatient patient Compact
                    Expect.hasLength result 1 "Compact format should return single line"
                    Expect.stringContains result.[0] "Patient Resource" "Should contain resource type"
                | _ -> failtest "Should parse sample patient"
            }
            
            test "Patient tree format should be multi-line" {
                match JsonSerialization.tryParseResource samplePatientJson with
                | Ok ("Patient", (:? Patient as patient)) ->
                    let result = formatPatient patient Tree
                    Expect.isGreaterThan result.Length 5 "Tree format should return multiple lines"
                    Expect.exists result (fun line -> line.Contains("Patient Resource")) "Should contain header"
                | _ -> failtest "Should parse sample patient"
            }
            
            test "Observation compact format should be single line" {
                match JsonSerialization.tryParseResource sampleObservationJson with
                | Ok ("Observation", (:? Observation as observation)) ->
                    let result = formatObservation observation Compact
                    Expect.hasLength result 1 "Compact format should return single line"
                    Expect.stringContains result.[0] "Observation Resource" "Should contain resource type"
                | _ -> failtest "Should parse sample observation"
            }
        ]
        
        testList "Main Visualization Function Tests" [
            test "visualizeResource should handle Patient" {
                match JsonSerialization.tryParseResource samplePatientJson with
                | Ok (resourceType, resource) ->
                    let result = visualizeResource resourceType resource Tree
                    Expect.isNonEmpty result "Should return visualization"
                    Expect.exists result (fun line -> line.Contains("Patient")) "Should contain Patient type"
                | _ -> failtest "Should parse sample patient"
            }
            
            test "visualizeResource should handle Observation" {
                match JsonSerialization.tryParseResource sampleObservationJson with
                | Ok (resourceType, resource) ->
                    let result = visualizeResource resourceType resource Tree
                    Expect.isNonEmpty result "Should return visualization"
                    Expect.exists result (fun line -> line.Contains("Observation")) "Should contain Observation type"
                | _ -> failtest "Should parse sample observation"
            }
            
            test "visualizeResource should handle unsupported type" {
                let result = visualizeResource "UnsupportedType" (obj()) Tree
                Expect.hasLength result 1 "Should return single error line"
                Expect.stringContains result.[0] "Unsupported resource type" "Should indicate unsupported type"
            }
        ]
        
        testList "JSON Visualization Tests" [
            test "visualizeFromJson should include original JSON section" {
                let result = visualizeFromJson samplePatientJson Tree
                Expect.isNonEmpty result "Should return visualization"
                Expect.exists result (fun line -> line.Contains("Original JSON")) "Should contain original JSON header"
                Expect.exists result (fun line -> line.Contains("Parsed F# Structure")) "Should contain parsed structure header"
            }
            
            test "visualizeFromJson should handle parsing errors" {
                let result = visualizeFromJson invalidJson Tree
                Expect.isNonEmpty result "Should return visualization"
                Expect.exists result (fun line -> line.Contains("Parsing Errors")) "Should contain error header"
            }
            
            test "visualizeJson should handle valid JSON" {
                let result = visualizeJson samplePatientJson Tree
                Expect.isNonEmpty result "Should return visualization"
                Expect.exists result (fun line -> line.Contains("Patient")) "Should contain resource type"
            }
            
            test "visualizeJson should handle invalid JSON" {
                let result = visualizeJson invalidJson Tree
                Expect.isNonEmpty result "Should return error visualization"
                Expect.exists result (fun line -> line.Contains("Parsing Errors")) "Should contain error information"
            }
        ]
        
        testList "File Visualization Tests" [
            test "visualizeFile should fail for non-existent file" {
                let result = visualizeFile "non-existent-file.json" Tree
                Expect.hasLength result 1 "Should return single error line"
                Expect.stringContains result.[0] "File not found" "Should indicate file not found"
            }
            
            testAsync "visualizeFile should visualize existing file" {
                let tempFile = Path.GetTempFileName()
                try
                    do! File.WriteAllTextAsync(tempFile, samplePatientJson) |> Async.AwaitTask
                    let result = visualizeFile tempFile Tree
                    Expect.isNonEmpty result "Should return visualization"
                    Expect.exists result (fun line -> line.Contains("File:")) "Should contain file header"
                    Expect.exists result (fun line -> line.Contains("Patient")) "Should contain resource content"
                finally
                    if File.Exists tempFile then File.Delete tempFile
            }
        ]
        
        testList "Error Handling Tests" [
            test "visualizeJson should handle malformed JSON gracefully" {
                let malformedJson = """{"resourceType": "Patient", "malformed": }"""
                let result = visualizeJson malformedJson Tree
                Expect.isNonEmpty result "Should return error visualization"
                Expect.exists result (fun line -> line.Contains("Error") || line.Contains("Parsing Errors")) "Should contain error information"
            }
            
            test "visualizeFromJson should handle exception gracefully" {
                let result = visualizeFromJson "" Tree
                Expect.isNonEmpty result "Should return some visualization"
                // Should not throw exception
            }
        ]
        
        testList "Performance Tests" [
            test "visualization should complete within reasonable time" {
                let stopwatch = System.Diagnostics.Stopwatch.StartNew()
                let result = visualizeFromJson samplePatientJson Tree
                stopwatch.Stop()
                Expect.isLessThan stopwatch.ElapsedMilliseconds 1000L "Visualization should complete within 1 second"
                Expect.isNonEmpty result "Should return visualization"
            }
            
            test "compact format should be faster than detailed format" {
                match JsonSerialization.tryParseResource samplePatientJson with
                | Ok ("Patient", (:? Patient as patient)) ->
                    let compactTime = measureExecutionTime (fun () -> formatPatient patient Compact)
                    let detailedTime = measureExecutionTime (fun () -> formatPatient patient Detailed)
                    Expect.isLessThanOrEqual compactTime detailedTime "Compact format should be faster or equal to detailed format"
                | _ -> failtest "Should parse sample patient"
            }
        ]
        
        testList "Integration Tests" [
            test "full visualization pipeline should work end-to-end" {
                // Test the complete pipeline from JSON to visualization
                let result = visualizeFromJson samplePatientJson Tree
                
                // Should contain all expected sections
                Expect.exists result (fun line -> line.Contains("Original JSON")) "Should have original JSON section"
                Expect.exists result (fun line -> line.Contains("Parsed F# Structure")) "Should have parsed structure section"
                Expect.exists result (fun line -> line.Contains("Patient Resource")) "Should have resource header"
                
                // Should have proper structure
                Expect.isGreaterThan result.Length 10 "Should have substantial output"
            }
            
            test "different formats should produce different outputs" {
                match JsonSerialization.tryParseResource samplePatientJson with
                | Ok ("Patient", (:? Patient as patient)) ->
                    let compactResult = formatPatient patient Compact
                    let treeResult = formatPatient patient Tree
                    let detailedResult = formatPatient patient Detailed
                    
                    Expect.notEqual compactResult treeResult "Compact and tree formats should differ"
                    Expect.isLessThan compactResult.Length treeResult.Length "Compact should be shorter than tree"
                | _ -> failtest "Should parse sample patient"
            }
        ]
    ]