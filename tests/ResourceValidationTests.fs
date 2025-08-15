module FSharpFHIR.Tests.ResourceValidationTests

open Xunit
open FSharpFHIR.ResourceValidation

/// Ensures the bundled sample patient resource passes validation.
[<Fact>]
let ``patient example is valid`` () =
    Assert.True(isValid patientSamplePath)
