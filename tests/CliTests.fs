module FSharpFHIR.Tests.CliTests

open Xunit
open System.IO
open FSharpFHIR.Cli
open FSharpFHIR.ResourceValidation

[<Fact>]
let ``validate command returns success`` () =
    let exitCode = run [| "validate"; patientSamplePath |]
    Assert.Equal(0, exitCode)

[<Fact>]
let ``validate command returns failure for invalid JSON`` () =
    let temp = Path.GetTempFileName()
    File.WriteAllText(temp, "{}");
    let exitCode = run [| "validate"; temp |]
    File.Delete(temp)
    Assert.Equal(1, exitCode)
