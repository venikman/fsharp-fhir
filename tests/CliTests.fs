module FSharpFHIR.Tests.CliTests

open Expecto
open System.IO
open FSharpFHIR.Cli
open FSharpFHIR.ResourceValidation

[<Tests>]
let tests =
    testList "cli validation" [
        testCase "validate command returns success" <| fun _ ->
            let exitCode = run [| "validate"; patientSamplePath |]
            Expect.equal exitCode 0 "successful validation should return 0"

        testCase "validate command returns failure for invalid JSON" <| fun _ ->
            let temp = Path.GetTempFileName()
            File.WriteAllText(temp, "{}")
            let exitCode = run [| "validate"; temp |]
            File.Delete(temp)
            Expect.equal exitCode 1 "invalid json should fail"
    ]
