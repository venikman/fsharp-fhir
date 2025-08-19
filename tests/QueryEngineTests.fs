module QueryEngineTests

open Expecto
open System.Text.Json
open FSharpFHIR.QueryEngine
open TestHelpers

let parseTests =
    testList "QueryParser Tests" [
        test "Parse identity query" {
            let result = QueryParser.parse "."
            match result with
            | Ok Identity -> ()
            | _ -> failwith "Expected Identity"
        }
        
        test "Parse property access" {
            let result = QueryParser.parse ".name"
            match result with
            | Ok (Property "name") -> ()
            | _ -> failwith "Expected Property 'name'"
        }
        
        test "Parse nested property access" {
            let result = QueryParser.parse ".name.family"
            match result with
            | Ok (Pipe(Property "name", Property "family")) -> ()
            | _ -> failwith "Expected nested property access"
        }
        
        test "Parse array index" {
            let result = QueryParser.parse ".[0]"
            match result with
            | Ok (Index 0) -> ()
            | _ -> failwith "Expected Index 0"
        }
        
        test "Parse array access with expression" {
            let result = QueryParser.parse ".name[0]"
            match result with
            | Ok (ArrayAccess(Property "name", 0)) -> ()
            | _ -> failwith "Expected ArrayAccess"
        }
        
        test "Parse length function" {
            let result = QueryParser.parse ".name | length"
            match result with
            | Ok (Pipe(Property "name", Length)) -> ()
            | _ -> failwith "Expected length function"
        }
        
        test "Parse invalid query returns error" {
            let result = QueryParser.parse "invalid[["
            match result with
            | Error _ -> ()
            | _ -> failwith "Expected parse error"
        }
    ]

let executionTests =
    testList "QueryExecutor Tests" [
        test "Execute identity on JSON" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let result = QueryExecutor.executeOnElement Identity doc.RootElement
            match result with
            | Single json -> 
                Expect.isTrue (json.Contains("Patient")) "Should contain Patient"
            | _ -> failwith "Expected Single result"
        }
        
        test "Execute property access" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let result = QueryExecutor.executeOnElement (Property "id") doc.RootElement
            match result with
            | Single "\"example\"" -> ()
            | _ -> failwith "Expected id 'example'"
        }
        
        test "Execute nested property access" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let expr = Pipe(Property "name", ArrayAccess(Identity, 0))
            let result = QueryExecutor.executeOnElement expr doc.RootElement
            match result with
            | Single json -> 
                Expect.isTrue (json.Contains("Chalmers")) "Should contain family name"
            | _ -> failwith "Expected Single result with name"
        }
        
        test "Execute array index" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let expr = Pipe(Property "name", Index 0)
            let result = QueryExecutor.executeOnElement expr doc.RootElement
            match result with
            | Single json -> 
                Expect.isTrue (json.Contains("official")) "Should contain use 'official'"
            | _ -> failwith "Expected Single result"
        }
        
        test "Execute length on array" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let expr = Pipe(Property "name", Length)
            let result = QueryExecutor.executeOnElement expr doc.RootElement
            match result with
            | Single "1" -> ()
            | _ -> failwith "Expected length 1"
        }
        
        test "Execute length on string" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let expr = Pipe(Property "gender", Length)
            let result = QueryExecutor.executeOnElement expr doc.RootElement
            match result with
            | Single "4" -> () // "male" has 4 characters
            | _ -> failwith "Expected length 4"
        }
        
        test "Execute on non-existent property returns Empty" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let result = QueryExecutor.executeOnElement (Property "nonexistent") doc.RootElement
            match result with
            | Empty -> ()
            | _ -> failwith "Expected Empty result"
        }
    ]

let filterTests =
    testList "Filter and Condition Tests" [
        test "Execute has condition" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let condition = Has "id"
            let result = QueryExecutor.evaluateCondition condition doc.RootElement
            Expect.isTrue result "Should have 'id' property"
        }
        
        test "Execute has condition on missing property" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let condition = Has "missing"
            let result = QueryExecutor.evaluateCondition condition doc.RootElement
            Expect.isFalse result "Should not have 'missing' property"
        }
        
        test "Execute contains condition" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let condition = Contains(Property "gender", JsonSerializer.SerializeToElement("male"))
            let result = QueryExecutor.evaluateCondition condition doc.RootElement
            Expect.isTrue result "Gender should contain 'male'"
        }
        
        test "Execute equals condition" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let condition = Equals(Property "gender", JsonSerializer.SerializeToElement("male"))
            let result = QueryExecutor.evaluateCondition condition doc.RootElement
            Expect.isTrue result "Gender should equal 'male'"
        }
        
        test "Execute not equals condition" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let condition = NotEquals(Property "gender", JsonSerializer.SerializeToElement("female"))
            let result = QueryExecutor.evaluateCondition condition doc.RootElement
            Expect.isTrue result "Gender should not equal 'female'"
        }
        
        test "Execute is empty condition on empty array" {
            let emptyArrayJson = "[]"
            use doc = JsonDocument.Parse(emptyArrayJson)
            let condition = IsEmptyCondition Identity
            let result = QueryExecutor.evaluateCondition condition doc.RootElement
            Expect.isTrue result "Empty array should be empty"
        }
        
        test "Execute is null condition" {
            let nullJson = "null"
            use doc = JsonDocument.Parse(nullJson)
            let condition = IsNullCondition Identity
            let result = QueryExecutor.evaluateCondition condition doc.RootElement
            Expect.isTrue result "Null should be null"
        }
    ]

let aggregationTests =
    testList "Aggregation Tests" [
        test "Execute sum on numeric array" {
            let numbersJson = "[1, 2, 3, 4, 5]"
            use doc = JsonDocument.Parse(numbersJson)
            let result = QueryExecutor.executeOnElement Sum doc.RootElement
            match result with
            | Single "15" -> ()
            | _ -> failwith "Expected sum 15"
        }
        
        test "Execute min on numeric array" {
            let numbersJson = "[5, 2, 8, 1, 9]"
            use doc = JsonDocument.Parse(numbersJson)
            let result = QueryExecutor.executeOnElement Min doc.RootElement
            match result with
            | Single "1" -> ()
            | _ -> failwith "Expected min 1"
        }
        
        test "Execute max on numeric array" {
            let numbersJson = "[5, 2, 8, 1, 9]"
            use doc = JsonDocument.Parse(numbersJson)
            let result = QueryExecutor.executeOnElement Max doc.RootElement
            match result with
            | Single "9" -> ()
            | _ -> failwith "Expected max 9"
        }
        
        test "Execute average on numeric array" {
            let numbersJson = "[2, 4, 6, 8]"
            use doc = JsonDocument.Parse(numbersJson)
            let result = QueryExecutor.executeOnElement Average doc.RootElement
            match result with
            | Single "5" -> ()
            | _ -> failwith "Expected average 5"
        }
        
        test "Execute count on array" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let expr = Pipe(Property "name", Count)
            let result = QueryExecutor.executeOnElement expr doc.RootElement
            match result with
            | Single "1" -> ()
            | _ -> failwith "Expected count 1"
        }
    ]

let stringOperationTests =
    testList "String Operation Tests" [
        test "Execute startswith" {
            let stringJson = "\"Hello World\""
            use doc = JsonDocument.Parse(stringJson)
            let result = QueryExecutor.executeOnElement (StartsWith "Hello") doc.RootElement
            match result with
            | Single "true" -> ()
            | _ -> failwith "Expected true for startswith"
        }
        
        test "Execute endswith" {
            let stringJson = "\"Hello World\""
            use doc = JsonDocument.Parse(stringJson)
            let result = QueryExecutor.executeOnElement (EndsWith "World") doc.RootElement
            match result with
            | Single "true" -> ()
            | _ -> failwith "Expected true for endswith"
        }
        
        test "Execute split" {
            let stringJson = "\"a,b,c\""
            use doc = JsonDocument.Parse(stringJson)
            let result = QueryExecutor.executeOnElement (Split ",") doc.RootElement
            match result with
            | Multiple parts -> 
                Expect.equal parts.Length 3 "Should have 3 parts"
                Expect.equal parts.[0] "\"a\"" "First part should be 'a'"
            | _ -> failwith "Expected Multiple result"
        }
    ]

let sortingTests =
    testList "Sorting Tests" [
        test "Execute sort on string array" {
            let arrayJson = "[\"c\", \"a\", \"b\"]"
            use doc = JsonDocument.Parse(arrayJson)
            let result = QueryExecutor.executeOnElement Sort doc.RootElement
            match result with
            | Multiple sorted -> 
                Expect.equal sorted.[0] "\"a\"" "First should be 'a'"
                Expect.equal sorted.[1] "\"b\"" "Second should be 'b'"
                Expect.equal sorted.[2] "\"c\"" "Third should be 'c'"
            | _ -> failwith "Expected Multiple result"
        }
        
        test "Execute sort on number array" {
            let arrayJson = "[3, 1, 2]"
            use doc = JsonDocument.Parse(arrayJson)
            let result = QueryExecutor.executeOnElement Sort doc.RootElement
            match result with
            | Multiple sorted -> 
                Expect.equal sorted.[0] "1" "First should be 1"
                Expect.equal sorted.[1] "2" "Second should be 2"
                Expect.equal sorted.[2] "3" "Third should be 3"
            | _ -> failwith "Expected Multiple result"
        }
    ]

let performanceTests =
    testList "Performance Tests" [
        test "Query execution performance" {
            let (_, time) = measureTime (fun () ->
                use doc = JsonDocument.Parse(samplePatientJson)
                for _ in 1..1000 do
                    QueryExecutor.executeOnElement (Property "id") doc.RootElement |> ignore
            )
            Expect.isLessThan time 1000L "Should execute 1000 queries in less than 1 second"
        }
        
        test "Complex query performance" {
            let (_, time) = measureTime (fun () ->
                use doc = JsonDocument.Parse(samplePatientJson)
                let complexExpr = Pipe(Property "name", Pipe(ArrayAccess(Identity, 0), Property "family"))
                for _ in 1..100 do
                    QueryExecutor.executeOnElement complexExpr doc.RootElement |> ignore
            )
            Expect.isLessThan time 500L "Should execute 100 complex queries in less than 500ms"
        }
    ]

let errorHandlingTests =
    testList "Error Handling Tests" [
        test "Invalid JSON handling" {
            let result = QueryEngine.run ".id" "invalid json"
            match result with
            | Error _ -> ()
            | _ -> failwith "Expected error for invalid JSON"
        }
        
        test "Array access out of bounds" {
            use doc = JsonDocument.Parse("[1, 2, 3]")
            let result = QueryExecutor.executeOnElement (Index 10) doc.RootElement
            match result with
            | Empty -> ()
            | _ -> failwith "Expected Empty for out of bounds access"
        }
        
        test "Property access on non-object" {
            use doc = JsonDocument.Parse("42")
            let result = QueryExecutor.executeOnElement (Property "test") doc.RootElement
            match result with
            | Empty -> ()
            | _ -> failwith "Expected Empty for property access on number"
        }
    ]

let integrationTests =
    testList "Integration Tests" {
        test "End-to-end query execution" {
            let query = ".name[0].family"
            let result = QueryEngine.run query samplePatientJson
            match result with
            | Ok (QueryResult.Single family) -> 
                Expect.equal family "\"Chalmers\"" "Should extract family name"
            | _ -> failwith "Expected successful family name extraction"
        }
        
        test "Interactive query execution" {
            use doc = JsonDocument.Parse(samplePatientJson)
            let result = QueryEngine.runInteractive ".gender" doc.RootElement
            match result with
            | Ok (QueryResult.Single gender) -> 
                Expect.equal gender "\"male\"" "Should extract gender"
            | _ -> failwith "Expected successful gender extraction"
        }
        
        test "Complex filtering query" {
            let patientsJson = """[
                {"resourceType": "Patient", "id": "1", "active": true, "gender": "male"},
                {"resourceType": "Patient", "id": "2", "active": false, "gender": "female"},
                {"resourceType": "Patient", "id": "3", "active": true, "gender": "male"}
            ]"""
            let query = "map(select(.active == true))"
            let result = QueryEngine.run query patientsJson
            match result with
            | Ok (QueryResult.Multiple filtered) -> 
                Expect.equal filtered.Length 2 "Should filter to 2 active patients"
            | _ -> failwith "Expected successful filtering"
        }
    }

let allTests =
    testList "QueryEngine Tests" [
        parseTests
        executionTests
        filterTests
        aggregationTests
        stringOperationTests
        sortingTests
        performanceTests
        errorHandlingTests
        integrationTests
    ]