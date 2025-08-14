module FSharpFHIR.Tests.PrimitiveSerializationTests

open Xunit
open System.Text.Json

module Primitive =
    let roundTrip<'a> (value: 'a) =
        let json = JsonSerializer.Serialize<'a> value
        JsonSerializer.Deserialize<'a> json

[<Fact>]
let ``string roundtrips`` () =
    let v = "hello"
    let result = Primitive.roundTrip v
    Assert.Equal(v, result)

[<Fact>]
let ``int roundtrips`` () =
    let v = 42
    let result = Primitive.roundTrip v
    Assert.Equal(v, result)

[<Fact>]
let ``boolean roundtrips`` () =
    let v = true
    let result = Primitive.roundTrip v
    Assert.Equal(v, result)
