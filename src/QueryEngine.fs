module FSharpFHIR.QueryEngine

open System
open System.Text.Json
open System.Text.RegularExpressions

// Core types for query expressions
type QueryExpression =
    | Identity
    | Property of string
    | SafeProperty of string
    | Index of int
    | ArrayAccess of QueryExpression * int
    | Slice of int option * int option
    | Keys
    | Length
    | Pipe of QueryExpression * QueryExpression
    | Map of QueryExpression
    | Select of QueryExpression
    | SortBy of QueryExpression
    | Has of string
    | Filter of FilterCondition
    | TypeOf
    | IsSome
    | IsNone
    | IsNull
    | IsEmpty
    | Count
    | Sum
    | Average
    | Min
    | Max
    | Contains of string
    | StartsWith of string
    | EndsWith of string
    | Split of string
    | ToUpper
    | ToLower
    | IfThenElse of FilterCondition * QueryExpression * QueryExpression

and FilterCondition =
    | Exists of QueryExpression
    | Equals of QueryExpression * JsonElement
    | NotEquals of QueryExpression * JsonElement
    | GreaterThan of QueryExpression * JsonElement
    | LessThan of QueryExpression * JsonElement
    | GreaterThanOrEqual of QueryExpression * JsonElement
    | LessThanOrEqual of QueryExpression * JsonElement
    | And of FilterCondition * FilterCondition
    | Or of FilterCondition * FilterCondition
    | Not of FilterCondition
    | IsSomeCondition of QueryExpression
    | IsNoneCondition of QueryExpression
    | IsNullCondition of QueryExpression
    | IsEmptyCondition of QueryExpression
    | TypeEquals of QueryExpression * string

type QueryResult =
    | Single of string
    | Multiple of string list
    | Empty
    | Error of string

// Parser for jq-like expressions
module QueryParser =
    type Token =
        | Dot
        | Identifier of string
        | Number of int
        | String of string
        | LeftBracket
        | RightBracket
        | LeftParen
        | RightParen
        | Pipe
        | Comma
        | Colon
        | Question
        | EOF

    let tokenize (input: string) : Token list =
        let rec tokenizeRec (chars: char list) (acc: Token list) =
            match chars with
            | [] -> List.rev (EOF :: acc)
            | '.' :: rest -> tokenizeRec rest (Dot :: acc)
            | '[' :: rest -> tokenizeRec rest (LeftBracket :: acc)
            | ']' :: rest -> tokenizeRec rest (RightBracket :: acc)
            | '(' :: rest -> tokenizeRec rest (LeftParen :: acc)
            | ')' :: rest -> tokenizeRec rest (RightParen :: acc)
            | '|' :: rest -> tokenizeRec rest (Pipe :: acc)
            | ',' :: rest -> tokenizeRec rest (Comma :: acc)
            | ':' :: rest -> tokenizeRec rest (Colon :: acc)
            | '?' :: rest -> tokenizeRec rest (Question :: acc)
            | ' ' :: rest | '\t' :: rest | '\n' :: rest | '\r' :: rest -> tokenizeRec rest acc
            | '"' :: rest ->
                let rec readString (chars: char list) (acc: char list) =
                    match chars with
                    | '"' :: rest -> (String.Concat(List.rev acc), rest)
                    | c :: rest -> readString rest (c :: acc)
                    | [] -> (String.Concat(List.rev acc), [])
                let (str, remaining) = readString rest []
                tokenizeRec remaining (String str :: acc)
            | c :: rest when Char.IsDigit(c) ->
                let rec readNumber (chars: char list) (acc: char list) =
                    match chars with
                    | c :: rest when Char.IsDigit(c) -> readNumber rest (c :: acc)
                    | _ -> (Int32.Parse(String.Concat(List.rev acc)), chars)
                let (num, remaining) = readNumber (c :: rest) []
                tokenizeRec remaining (Number num :: acc)
            | c :: rest when Char.IsLetter(c) || c = '_' ->
                let rec readIdentifier (chars: char list) (acc: char list) =
                    match chars with
                    | c :: rest when Char.IsLetterOrDigit(c) || c = '_' -> readIdentifier rest (c :: acc)
                    | _ -> (String.Concat(List.rev acc), chars)
                let (id, remaining) = readIdentifier (c :: rest) []
                tokenizeRec remaining (Identifier id :: acc)
            | _ :: rest -> tokenizeRec rest acc
        tokenizeRec (input.ToCharArray() |> Array.toList) []

    let rec parseExpression (tokens: Token list) : QueryExpression * Token list =
        match tokens with
        | Dot :: Identifier prop :: rest -> (Property prop, rest)
        | Dot :: Question :: Identifier prop :: rest -> (SafeProperty prop, rest)
        | Dot :: LeftBracket :: Number idx :: RightBracket :: rest -> (Index idx, rest)
        | LeftBracket :: Number idx :: RightBracket :: rest -> (Index idx, rest)
        | Identifier "keys" :: rest -> (Keys, rest)
        | Identifier "length" :: rest -> (Length, rest)
        | Identifier "type" :: rest -> (TypeOf, rest)
        | Identifier "empty" :: rest -> (IsEmpty, rest)
        | Identifier "count" :: rest -> (Count, rest)
        | Identifier "sum" :: rest -> (Sum, rest)
        | Identifier "average" :: rest -> (Average, rest)
        | Identifier "min" :: rest -> (Min, rest)
        | Identifier "max" :: rest -> (Max, rest)
        | Identifier "has" :: LeftParen :: String prop :: RightParen :: rest -> (Has prop, rest)
        | Identifier "contains" :: LeftParen :: String str :: RightParen :: rest -> (Contains str, rest)
        | Identifier "startswith" :: LeftParen :: String str :: RightParen :: rest -> (StartsWith str, rest)
        | Identifier "endswith" :: LeftParen :: String str :: RightParen :: rest -> (EndsWith str, rest)
        | Identifier "split" :: LeftParen :: String delim :: RightParen :: rest -> (Split delim, rest)
        | Identifier "toupper" :: rest -> (ToUpper, rest)
        | Identifier "tolower" :: rest -> (ToLower, rest)
        | Identifier "issome" :: rest -> (IsSome, rest)
        | Identifier "isnone" :: rest -> (IsNone, rest)
        | Identifier "isnull" :: rest -> (IsNull, rest)
        | Identifier "isempty" :: rest -> (IsEmpty, rest)
        | _ -> (Identity, tokens)

    let parse (query: string) : Result<QueryExpression, string> =
        try
            let tokens = tokenize query
            let (expr, _) = parseExpression tokens
            Ok expr
        with
        | ex -> Result.Error $"Parse error: {ex.Message}"

// Execution engine
module QueryExecutor =
    let rec executeOnElement (expr: QueryExpression) (element: JsonElement) : QueryResult =
        match expr with
        | Identity -> Single (element.GetRawText())
        | Property prop ->
            if element.ValueKind = JsonValueKind.Object then
                let (found, value) = element.TryGetProperty(prop)
                if found then Single (value.GetRawText()) else Empty
            else Empty
        | SafeProperty prop ->
            if element.ValueKind = JsonValueKind.Object then
                let (found, value) = element.TryGetProperty(prop)
                if found then Single (value.GetRawText()) else Empty
            else Empty
        | Index idx ->
            if element.ValueKind = JsonValueKind.Array then
                let array = element.EnumerateArray() |> Seq.toArray
                if idx >= 0 && idx < array.Length then
                    Single (array.[idx].GetRawText())
                else Empty
            else Empty
        | ArrayAccess(expr, idx) ->
            match executeOnElement expr element with
            | Single result -> 
                try
                    use doc = JsonDocument.Parse(result)
                    executeOnElement (Index idx) doc.RootElement
                with
                | _ -> Error "Invalid JSON in array access"
            | _ -> Empty
        | Slice(start, end_) ->
            if element.ValueKind = JsonValueKind.Array then
                let array = element.EnumerateArray() |> Seq.toArray
                let startIdx = defaultArg start 0
                let endIdx = defaultArg end_ array.Length
                let sliced = array.[startIdx..endIdx-1] |> Array.map (fun elem -> elem.GetRawText()) |> Array.toList
                Multiple sliced
            else Empty
        | Keys ->
            if element.ValueKind = JsonValueKind.Object then
                let keys = element.EnumerateObject() |> Seq.map (fun prop -> JsonSerializer.SerializeToElement(prop.Name).GetRawText()) |> Seq.toList
                Multiple keys
            else Empty
        | Length ->
            match element.ValueKind with
            | JsonValueKind.Array -> 
                let length = element.GetArrayLength()
                JsonSerializer.SerializeToElement(length).GetRawText() |> Single
            | JsonValueKind.Object ->
                let length = element.EnumerateObject() |> Seq.length
                JsonSerializer.SerializeToElement(length).GetRawText() |> Single
            | JsonValueKind.String ->
                let length = element.GetString().Length
                JsonSerializer.SerializeToElement(length).GetRawText() |> Single
            | _ -> Empty
        | Pipe(left, right) ->
            match executeOnElement left element with
            | Single result -> 
                // Parse the result back to JsonElement for the next operation
                try
                    use doc = JsonDocument.Parse(result)
                    executeOnElement right doc.RootElement
                with
                | _ -> Error "Invalid JSON in pipe operation"
            | Multiple results ->
                let mappedResults = results |> List.map (fun r ->
                    try
                        use doc = JsonDocument.Parse(r)
                        executeOnElement right doc.RootElement
                    with
                    | _ -> Error "Invalid JSON in pipe operation") |> List.collect (function
                    | Single r -> [r]
                    | Multiple rs -> rs
                    | Empty -> []
                    | Error _ -> [])
                Multiple mappedResults
            | Empty -> Empty
            | Error msg -> Error msg
        | Map(expr) ->
            if element.ValueKind = JsonValueKind.Array then
                let results = element.EnumerateArray() |> Seq.map (executeOnElement expr) |> Seq.toList
                let mappedResults = results |> List.collect (function
                    | Single r -> [r]
                    | Multiple rs -> rs
                    | Empty -> []
                    | Error _ -> [])
                Multiple mappedResults
            else Empty
        | Select(expr) ->
            if element.ValueKind = JsonValueKind.Array then
                let filtered = element.EnumerateArray() |> Seq.filter (fun e ->
                    match executeOnElement expr e with
                    | Single result -> 
                        try
                            use doc = JsonDocument.Parse(result)
                            let elem = doc.RootElement
                            elem.ValueKind <> JsonValueKind.Null && elem.ValueKind <> JsonValueKind.False
                        with
                        | _ -> false
                    | _ -> false) |> Seq.toList
                Multiple (filtered |> List.map (fun elem -> elem.GetRawText()))
            else Empty
        | Has(prop) ->
            if element.ValueKind = JsonValueKind.Object then
                let hasProperty = element.TryGetProperty(prop) |> fst
                JsonSerializer.SerializeToElement(hasProperty).GetRawText() |> Single
            else
                JsonSerializer.SerializeToElement(false).GetRawText() |> Single
        | Filter(condition) ->
            if element.ValueKind = JsonValueKind.Array then
                let filtered = element.EnumerateArray() |> Seq.filter (evaluateCondition condition) |> Seq.toList
                Multiple (filtered |> List.map (fun elem -> elem.GetRawText()))
            else Empty
        | TypeOf ->
            let typeName = 
                match element.ValueKind with
                | JsonValueKind.String -> "string"
                | JsonValueKind.Number -> "number"
                | JsonValueKind.True | JsonValueKind.False -> "boolean"
                | JsonValueKind.Array -> "array"
                | JsonValueKind.Object -> "object"
                | JsonValueKind.Null -> "null"
                | _ -> "undefined"
            JsonSerializer.SerializeToElement(typeName).GetRawText() |> Single
        | IsSome ->
            let isSome = element.ValueKind <> JsonValueKind.Null
            JsonSerializer.SerializeToElement(isSome).GetRawText() |> Single
        | IsNone ->
            let isNone = element.ValueKind = JsonValueKind.Null
            JsonSerializer.SerializeToElement(isNone).GetRawText() |> Single
        | IsNull ->
            let isNull = element.ValueKind = JsonValueKind.Null
            JsonSerializer.SerializeToElement(isNull).GetRawText() |> Single
        | IsEmpty ->
            let isEmpty = 
                match element.ValueKind with
                | JsonValueKind.Array -> element.GetArrayLength() = 0
                | JsonValueKind.Object -> element.EnumerateObject() |> Seq.isEmpty
                | JsonValueKind.String -> element.GetString() = ""
                | JsonValueKind.Null -> true
                | _ -> false
            JsonSerializer.SerializeToElement(isEmpty).GetRawText() |> Single
        | Count ->
            if element.ValueKind = JsonValueKind.Array then
                let count = element.GetArrayLength()
                JsonSerializer.SerializeToElement(count).GetRawText() |> Single
            else Empty
        | Sum ->
            if element.ValueKind = JsonValueKind.Array then
                let sum = 
                    element.EnumerateArray()
                    |> Seq.filter (fun e -> e.ValueKind = JsonValueKind.Number)
                    |> Seq.sumBy (fun e -> e.GetDouble())
                JsonSerializer.SerializeToElement(sum).GetRawText() |> Single
            else Empty
        | Average ->
            if element.ValueKind = JsonValueKind.Array then
                let numbers = 
                    element.EnumerateArray()
                    |> Seq.filter (fun e -> e.ValueKind = JsonValueKind.Number)
                    |> Seq.toList
                if numbers.Length > 0 then
                    let avg = numbers |> List.averageBy (fun e -> e.GetDouble())
                    JsonSerializer.SerializeToElement(avg).GetRawText() |> Single
                else Empty
            else Empty
        | Min ->
            if element.ValueKind = JsonValueKind.Array then
                let numbers = 
                    element.EnumerateArray()
                    |> Seq.filter (fun e -> e.ValueKind = JsonValueKind.Number)
                    |> Seq.toList
                if numbers.Length > 0 then
                    let min = numbers |> List.minBy (fun e -> e.GetDouble())
                    Single (min.GetRawText())
                else Empty
            else Empty
        | Max ->
            if element.ValueKind = JsonValueKind.Array then
                let numbers = 
                    element.EnumerateArray()
                    |> Seq.filter (fun e -> e.ValueKind = JsonValueKind.Number)
                    |> Seq.toList
                if numbers.Length > 0 then
                    let max = numbers |> List.maxBy (fun e -> e.GetDouble())
                    Single (max.GetRawText())
                else Empty
            else Empty
        | Contains(searchStr) ->
            if element.ValueKind = JsonValueKind.String then
                let str = element.GetString()
                let contains = str.Contains(searchStr)
                JsonSerializer.SerializeToElement(contains).GetRawText() |> Single
            else
                JsonSerializer.SerializeToElement(false).GetRawText() |> Single
        | StartsWith(prefix) ->
            if element.ValueKind = JsonValueKind.String then
                let str = element.GetString()
                let startsWith = str.StartsWith(prefix)
                JsonSerializer.SerializeToElement(startsWith).GetRawText() |> Single
            else
                JsonSerializer.SerializeToElement(false).GetRawText() |> Single
        | EndsWith(suffix) ->
            if element.ValueKind = JsonValueKind.String then
                let str = element.GetString()
                let endsWith = str.EndsWith(suffix)
                JsonSerializer.SerializeToElement(endsWith).GetRawText() |> Single
            else
                JsonSerializer.SerializeToElement(false).GetRawText() |> Single
        | Split(delimiter) ->
            if element.ValueKind = JsonValueKind.String then
                let str = element.GetString()
                let parts = str.Split([|delimiter|], StringSplitOptions.None)
                let jsonArray = parts |> Array.map JsonSerializer.SerializeToElement
                JsonSerializer.SerializeToElement(jsonArray).GetRawText() |> Single
            else Empty
        | ToUpper ->
            if element.ValueKind = JsonValueKind.String then
                let str = element.GetString().ToUpper()
                JsonSerializer.SerializeToElement(str).GetRawText() |> Single
            else Empty
        | ToLower ->
            if element.ValueKind = JsonValueKind.String then
                let str = element.GetString().ToLower()
                JsonSerializer.SerializeToElement(str).GetRawText() |> Single
            else Empty
        | IfThenElse(condition, thenExpr, elseExpr) ->
            if evaluateCondition condition element then
                executeOnElement thenExpr element
            else
                executeOnElement elseExpr element
        | SortBy(expr) ->
            if element.ValueKind = JsonValueKind.Array then
                let sorted = element.EnumerateArray() |> Seq.sortBy (fun e ->
                    match executeOnElement expr e with
                    | Single result -> 
                        try
                            use doc = JsonDocument.Parse(result)
                            let elem = doc.RootElement
                            match elem.ValueKind with
                            | JsonValueKind.Number -> elem.GetDouble()
                            | JsonValueKind.String -> float (elem.GetString().GetHashCode())
                            | _ -> 0.0
                        with
                        | _ -> 0.0
                    | _ -> 0.0) |> Seq.toList
                Multiple (sorted |> List.map (fun elem -> elem.GetRawText()))
            else Empty

    and evaluateCondition (condition: FilterCondition) (element: JsonElement) : bool =
        match condition with
        | Exists(expr) ->
            match executeOnElement expr element with
            | Single _ -> true
            | Multiple [] -> false
            | Multiple _ -> true
            | Empty -> false
            | QueryResult.Error _ -> false
        | Equals(expr, value) ->
            match executeOnElement expr element with
            | Single result -> 
                try
                    result = value.GetRawText()
                with
                | _ -> false
            | _ -> false
        | NotEquals(expr, value) ->
            match executeOnElement expr element with
            | Single result -> 
                try
                    result <> value.GetRawText()
                with
                | _ -> true
            | _ -> true
        | GreaterThan(expr, value) ->
            match executeOnElement expr element with
            | Single result ->
                try
                    use doc = JsonDocument.Parse(result)
                    let elem = doc.RootElement
                    if elem.ValueKind = JsonValueKind.Number && value.ValueKind = JsonValueKind.Number then
                        elem.GetDouble() > value.GetDouble()
                    else false
                with
                | _ -> false
            | _ -> false
        | LessThan(expr, value) ->
            match executeOnElement expr element with
            | Single result ->
                try
                    use doc = JsonDocument.Parse(result)
                    let elem = doc.RootElement
                    if elem.ValueKind = JsonValueKind.Number && value.ValueKind = JsonValueKind.Number then
                        elem.GetDouble() < value.GetDouble()
                    else false
                with
                | _ -> false
            | _ -> false
        | GreaterThanOrEqual(expr, value) ->
            match executeOnElement expr element with
            | Single result ->
                try
                    use doc = JsonDocument.Parse(result)
                    let elem = doc.RootElement
                    if elem.ValueKind = JsonValueKind.Number && value.ValueKind = JsonValueKind.Number then
                        elem.GetDouble() >= value.GetDouble()
                    else false
                with
                | _ -> false
            | _ -> false
        | LessThanOrEqual(expr, value) ->
            match executeOnElement expr element with
            | Single result ->
                try
                    use doc = JsonDocument.Parse(result)
                    let elem = doc.RootElement
                    if elem.ValueKind = JsonValueKind.Number && value.ValueKind = JsonValueKind.Number then
                        elem.GetDouble() <= value.GetDouble()
                    else false
                with
                | _ -> false
            | _ -> false
        | And(left, right) ->
            evaluateCondition left element && evaluateCondition right element
        | Or(left, right) ->
            evaluateCondition left element || evaluateCondition right element
        | Not(condition) ->
            not (evaluateCondition condition element)
        | IsSomeCondition(expr) ->
            match executeOnElement expr element with
            | Single result -> 
                try
                    use doc = JsonDocument.Parse(result)
                    doc.RootElement.ValueKind <> JsonValueKind.Null
                with
                | _ -> false
            | _ -> false
        | IsNoneCondition(expr) ->
            match executeOnElement expr element with
            | Single result -> 
                try
                    use doc = JsonDocument.Parse(result)
                    doc.RootElement.ValueKind = JsonValueKind.Null
                with
                | _ -> false
            | Empty -> true
            | _ -> false
        | IsNullCondition(expr) ->
            match executeOnElement expr element with
            | Single result -> 
                try
                    use doc = JsonDocument.Parse(result)
                    doc.RootElement.ValueKind = JsonValueKind.Null
                with
                | _ -> false
            | Empty -> true
            | _ -> false
        | IsEmptyCondition(expr) ->
            match executeOnElement expr element with
            | Single result -> 
                try
                    use doc = JsonDocument.Parse(result)
                    let elem = doc.RootElement
                    match elem.ValueKind with
                    | JsonValueKind.Array -> elem.GetArrayLength() = 0
                    | JsonValueKind.Object -> elem.EnumerateObject() |> Seq.isEmpty
                    | JsonValueKind.String -> elem.GetString() = ""
                    | JsonValueKind.Null -> true
                    | _ -> false
                with
                | _ -> false
            | Multiple [] -> true
            | Empty -> true
            | _ -> false
        | TypeEquals(expr, typeName) ->
            match executeOnElement expr element with
            | Single result ->
                try
                    use doc = JsonDocument.Parse(result)
                    let elem = doc.RootElement
                    let actualType = 
                        match elem.ValueKind with
                        | JsonValueKind.String -> "string"
                        | JsonValueKind.Number -> "number"
                        | JsonValueKind.True | JsonValueKind.False -> "boolean"
                        | JsonValueKind.Array -> "array"
                        | JsonValueKind.Object -> "object"
                        | JsonValueKind.Null -> "null"
                        | _ -> "undefined"
                    actualType = typeName
                with
                | _ -> false
            | _ -> false

    let execute (expr: QueryExpression) (json: string) : QueryResult =
        try
            use document = JsonDocument.Parse(json)
            executeOnElement expr document.RootElement
        with
        | ex -> QueryResult.Error $"Execution error: {ex.Message}"

// High-level query interface
module Query =
    let run (query: string) (json: string) : Result<QueryResult, string> =
        match QueryParser.parse query with
        | Ok expr -> 
            let result = QueryExecutor.execute expr json
            match result with
            | QueryResult.Error msg -> Result.Error msg
            | _ -> Ok result
        | Result.Error msg -> Result.Error msg

    let runInteractive (query: string) (element: JsonElement) : Result<QueryResult, string> =
        match QueryParser.parse query with
        | Ok expr -> 
            let result = QueryExecutor.executeOnElement expr element
            match result with
            | QueryResult.Error msg -> Result.Error msg
            | _ -> Ok result
        | Result.Error msg -> Result.Error msg
