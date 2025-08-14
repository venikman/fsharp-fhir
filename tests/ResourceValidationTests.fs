module FSharpFHIR.Tests.ResourceValidationTests

open Xunit
open FSharpFHIR.ResourceValidation

[<Fact>]
let ``patient example is valid`` () =
    Assert.True(isValid patientSamplePath)
