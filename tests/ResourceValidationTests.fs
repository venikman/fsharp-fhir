module FSharpFHIR.Tests.ResourceValidationTests

open Expecto
open FSharpFHIR.ResourceValidation

[<Tests>]
let tests =
    testList "resource validation" [
        testCase "patient example is valid" <| fun _ ->
            Expect.isTrue (isValid patientSamplePath) "sample patient should validate"
    ]
