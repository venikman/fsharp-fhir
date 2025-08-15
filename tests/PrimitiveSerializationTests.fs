module FSharpFHIR.Tests.PrimitiveSerializationTests

open Xunit
open System.Text.Json

/// Helpers for round-trip serialization using System.Text.Json.
module Primitive =
    /// Serialize a value to JSON and deserialize it back.
    let roundTrip<'a> (value: 'a) =
        let json = JsonSerializer.Serialize<'a> value
        JsonSerializer.Deserialize<'a> json

/// Verifies that strings serialize and deserialize without loss.
[<Fact>]
let ``string roundtrips`` () =
    let v = "hello"
    let result = Primitive.roundTrip v
    Assert.Equal(v, result)

/// Verifies that integers serialize and deserialize without loss.
[<Fact>]
let ``int roundtrips`` () =
    let v = 42
    let result = Primitive.roundTrip v
    Assert.Equal(v, result)

/// Verifies that booleans serialize and deserialize without loss.
[<Fact>]
let ``boolean roundtrips`` () =
    let v = true
    let result = Primitive.roundTrip v
    Assert.Equal(v, result)
