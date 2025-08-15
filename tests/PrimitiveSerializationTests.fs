module FSharpFHIR.Tests.PrimitiveSerializationTests

open Expecto
open System.Text.Json

/// Helpers for round-trip serialization using System.Text.Json.
module Primitive =
    /// Serialize a value to JSON and deserialize it back.
    let roundTrip<'a> (value: 'a) =
        let json = JsonSerializer.Serialize<'a> value
        JsonSerializer.Deserialize<'a> json

[<Tests>]
let tests =
    testList "primitive roundtrip" [
        testCase "string roundtrips" <| fun _ ->
            let v = "hello"
            let result = Primitive.roundTrip v
            Expect.equal result v "string should roundtrip"

        testCase "int roundtrips" <| fun _ ->
            let v = 42
            let result = Primitive.roundTrip v
            Expect.equal result v "int should roundtrip"

        testCase "boolean roundtrips" <| fun _ ->
            let v = true
            let result = Primitive.roundTrip v
            Expect.equal result v "boolean should roundtrip"
    ]
