namespace FSharpFHIR

open System
open System.Text.Json
open System.Text

module Visualizer =
    
    /// ANSI color codes for syntax highlighting
    module Colors =
        let reset = "\u001b[0m"
        let bold = "\u001b[1m"
        let dim = "\u001b[2m"
        
        // Property names and structure
        let property = "\u001b[36m"      // Cyan
        let typeName = "\u001b[35m"      // Magenta
        let keyword = "\u001b[33m"       // Yellow
        
        // Values
        let stringValue = "\u001b[32m"   // Green
        let numberValue = "\u001b[34m"   // Blue
        let boolValue = "\u001b[31m"     // Red
        let dateValue = "\u001b[36m"     // Cyan
        let enumValue = "\u001b[35m"     // Magenta
        
        // Structure
        let bracket = "\u001b[37m"       // White
        let optional = "\u001b[90m"      // Gray
        let none = "\u001b[90m"          // Gray
        
    /// Unicode box drawing characters for tree structure
    module TreeChars =
        let vertical = "│"
        let branch = "├─"
        let lastBranch = "└─"
        let space = "  "
        let continuation = "│ "
        
    /// Visualization format options
    type VisualizationFormat =
        | Tree
        | Compact
        | Detailed
        
    /// Helper to format optional values
    let formatOption (formatter: 'T -> string) (opt: 'T option) =
        match opt with
        | Some value -> formatter value
        | None -> $"{Colors.none}None{Colors.reset}"
        
    /// Helper to format lists with proper indexing
    let formatList (formatter: 'T -> string list) (items: 'T list) (prefix: string) (isLast: bool) =
        match items with
        | [] -> [$"{prefix}{Colors.bracket}[]{Colors.reset}"]
        | _ ->
            let header = $"{prefix}{Colors.bracket}[{Colors.reset} {Colors.dim}({items.Length} items){Colors.reset}"            
            let itemLines = 
                items
                |> List.mapi (fun i item ->
                    let isLastItem = i = items.Length - 1
                    let space = TreeChars.space
                    let continuation = TreeChars.continuation
                    let lastBranch = TreeChars.lastBranch
                    let branch = TreeChars.branch
                    let itemPrefix = if isLast then space else continuation
                    let itemBranch = if isLastItem then lastBranch else branch
                    let fullPrefix = prefix + itemPrefix + itemBranch
                    let indexLine = $"{fullPrefix}{Colors.dim}[{i}]{Colors.reset}"
                    let contentPrefix = prefix + itemPrefix + (if isLastItem then space else continuation)
                    indexLine :: (formatter item |> List.map (fun line -> contentPrefix + line))
                )
                |> List.concat
            let footer = $"{prefix}{Colors.bracket}]{Colors.reset}"
            header :: itemLines @ [footer]
            
    /// Format a string value with proper escaping and coloring
    let formatString (value: string) =
        $"{Colors.stringValue}\"{value}\"{Colors.reset}"
        
    /// Format a boolean value
    let formatBool (value: bool) =
        $"{Colors.boolValue}{value}{Colors.reset}"
        
    /// Format a number value
    let formatNumber (value: obj) =
        $"{Colors.numberValue}{value}{Colors.reset}"
        
    /// Format a DateTime value
    let formatDateTime (value: DateTime) =
        let dateStr = value.ToString("yyyy-MM-dd HH:mm:ss")
        $"{Colors.dateValue}{dateStr}{Colors.reset}"
        
    /// Format an enum value
    let formatEnum (value: obj) =
        $"{Colors.enumValue}{value}{Colors.reset}"
        
    /// Format property name
    let formatProperty (name: string) =
        $"{Colors.property}{name}{Colors.reset}"
        
    /// Format type name
    let formatType (typeName: string) =
        $"{Colors.typeName}{typeName}{Colors.reset}"
        
    /// Format HumanName
    let formatHumanName (name: HumanName) =
        let useValue = formatOption formatEnum name.``use``
        let textValue = formatOption formatString name.text
        let familyValue = formatOption formatString name.family
        let givenValues = String.Join(", ", name.given |> List.map formatString)
        let useProp = formatProperty "use"
        let textProp = formatProperty "text"
        let familyProp = formatProperty "family"
        let givenProp = formatProperty "given"
        let givenFormatted = $"{Colors.bracket}[{Colors.reset}{givenValues}{Colors.bracket}]{Colors.reset}"
        let branch = TreeChars.branch
        let lastBranch = TreeChars.lastBranch
        [
            formatType "HumanName"
            branch + useProp + ": " + useValue
            branch + textProp + ": " + textValue
            branch + familyProp + ": " + familyValue
            lastBranch + givenProp + ": " + givenFormatted
        ]
        
    /// Format Identifier
    let formatIdentifier (identifier: Identifier) =
        let useValue = formatOption formatEnum identifier.``use``
        let systemValue = formatOption (fun uri -> formatString (uri.ToString())) identifier.system
        let valueValue = formatOption formatString identifier.value
        let useProp = formatProperty "use"
        let systemProp = formatProperty "system"
        let valueProp = formatProperty "value"
        let branch = TreeChars.branch
        let lastBranch = TreeChars.lastBranch
        [
            formatType "Identifier"
            branch + useProp + ": " + useValue
            branch + systemProp + ": " + systemValue
            lastBranch + valueProp + ": " + valueValue
        ]
        
    /// Format Resource base properties
    let formatResource (resource: Resource) (prefix: string) =
        let idValue = formatOption formatString resource.id
        let metaValue = formatOption (fun _ -> "Meta") resource.meta
        let languageValue = formatOption formatString resource.language
        let idProp = formatProperty "id"
        let metaProp = formatProperty "meta"
        let languageProp = formatProperty "language"
        let branch = TreeChars.branch
        let lastBranch = TreeChars.lastBranch
        [
            prefix + branch + idProp + ": " + idValue
            prefix + branch + metaProp + ": " + metaValue
            prefix + lastBranch + languageProp + ": " + languageValue
        ]
        
    /// Format DomainResource
    let formatDomainResource (domainResource: DomainResource) (prefix: string) =
        let continuation = TreeChars.continuation
        let resourceLines = formatResource domainResource.resource (prefix + continuation)
        let textValue = formatOption (fun _ -> "Narrative") domainResource.text
        let containedCount = domainResource.contained.Length
        let extensionCount = domainResource.extension.Length
        let modifierExtensionCount = domainResource.modifierExtension.Length
        let domainResourceType = formatType "DomainResource"
        let resourceProp = formatProperty "resource"
        let textProp = formatProperty "text"
        let containedProp = formatProperty "contained"
        let extensionProp = formatProperty "extension"
        let modifierExtensionProp = formatProperty "modifierExtension"
        let branch = TreeChars.branch
        let lastBranch = TreeChars.lastBranch
        [
            prefix + domainResourceType
            prefix + branch + resourceProp + ":"
        ] @ resourceLines @
        [
            prefix + branch + textProp + ": " + textValue
            prefix + branch + containedProp + ": " + Colors.bracket + "[" + string containedCount + " items]" + Colors.reset
            prefix + branch + extensionProp + ": " + Colors.bracket + "[" + string extensionCount + " items]" + Colors.reset
            prefix + lastBranch + modifierExtensionProp + ": " + Colors.bracket + "[" + string modifierExtensionCount + " items]" + Colors.reset
        ]
        
    /// Format Patient resource
    let formatPatient (patient: Patient) (format: VisualizationFormat) =
        let patientType = formatType "Patient"
        let header = Colors.bold + patientType + " Resource" + Colors.reset
        let continuation = TreeChars.continuation
        let baseLines = formatDomainResource patient.resource continuation
        
        let activeValue = formatOption formatBool patient.active
        let genderValue = formatOption formatEnum patient.gender
        let birthDateValue = formatOption formatDateTime patient.birthDate
        let identifierProp = formatProperty "identifier"
        let activeProp = formatProperty "active"
        let nameProp = formatProperty "name"
        let genderProp = formatProperty "gender"
        let birthDateProp = formatProperty "birthDate"
        let identifierLines = formatList formatIdentifier patient.identifier "" false
        let nameLines = formatList formatHumanName patient.name "" false
        let branch = TreeChars.branch
        let lastBranch = TreeChars.lastBranch
        let patientSpecificLines = 
            [
                branch + identifierProp + ":"
            ] @ identifierLines @
            [
                branch + activeProp + ": " + activeValue
                branch + nameProp + ":"
            ] @ nameLines @
            [
                branch + genderProp + ": " + genderValue
                lastBranch + birthDateProp + ": " + birthDateValue
            ]
        
        match format with
        | Compact -> 
            let nameText = 
                patient.name 
                |> List.tryHead 
                |> Option.bind (fun n -> n.text)
                |> Option.defaultValue "No name"
            let genderText = 
                patient.gender 
                |> Option.map (fun g -> g.ToString())
                |> Option.defaultValue "Unknown"
            let nameFormatted = formatString nameText
            let genderFormatted = formatEnum genderText
            [$"{header} - {nameFormatted}, {genderFormatted}"]
        | Tree | Detailed ->
            header :: baseLines @ patientSpecificLines
            
    /// Format Observation resource  
    let formatObservation (observation: Observation) (format: VisualizationFormat) =
        let observationType = formatType "Observation"
        let header = Colors.bold + observationType + " Resource" + Colors.reset
        let continuation = TreeChars.continuation
        let baseLines = formatDomainResource observation.resource continuation
        
        let statusValue = formatEnum observation.status
        let codeValue = formatOption (fun _ -> "CodeableConcept") observation.code
        let subjectValue = formatOption (fun _ -> "Reference") observation.subject
        let effectiveValue = formatOption formatDateTime observation.effective
        let valueValue = formatOption (fun _ -> "ElementValue") observation.value
        let identifierProp = formatProperty "identifier"
        let statusProp = formatProperty "status"
        let codeProp = formatProperty "code"
        let subjectProp = formatProperty "subject"
        let effectiveProp = formatProperty "effective"
        let valueProp = formatProperty "value"
        let identifierLines = formatList formatIdentifier observation.identifier "" false
        let branch = TreeChars.branch
        let lastBranch = TreeChars.lastBranch
        let observationSpecificLines = 
            [
                branch + identifierProp + ":"
            ] @ identifierLines @
            [
                branch + statusProp + ": " + statusValue
                branch + codeProp + ": " + codeValue
                branch + subjectProp + ": " + subjectValue
                branch + effectiveProp + ": " + effectiveValue
                lastBranch + valueProp + ": " + valueValue
            ]
        
        match format with
        | Compact ->
            let statusText = observation.status.ToString()
            let statusFormatted = formatEnum statusText
            let compactLine = $"{header} - Status: {statusFormatted}"
            [compactLine]
        | Tree | Detailed ->
            header :: baseLines @ observationSpecificLines
            
    /// Main visualization function
    let visualizeResource (resourceType: string) (resource: obj) (format: VisualizationFormat) =
        match resourceType, resource with
        | "Patient", (:? Patient as patient) ->
            formatPatient patient format
        | "Observation", (:? Observation as observation) ->
            formatObservation observation format
        | _ ->
            [$"{Colors.bold}Unsupported resource type: {formatType resourceType}{Colors.reset}"]
            
    /// Visualize a FHIR resource from JSON with both original and parsed views
    let visualizeFromJson (json: string) (format: VisualizationFormat) =
        let lines = ResizeArray<string>()
        
        // Add original JSON section
        lines.Add($"{Colors.bold}📄 Original JSON:{Colors.reset}")
        lines.Add("")
        
        // Pretty print JSON with basic formatting
        let jsonLines = 
            json.Split('\n')
            |> Array.map (fun line -> $"{Colors.dim}{line.Trim()}{Colors.reset}")
        lines.AddRange(jsonLines)
        lines.Add("")
        
        // Add parsed F# structure section
        lines.Add($"{Colors.bold}🔍 Parsed F# Structure:{Colors.reset}")
        lines.Add("")
        
        match JsonSerialization.tryParseResource json with
        | Ok (resourceType, resource) ->
            let visualizedLines = visualizeResource resourceType resource format
            lines.AddRange(visualizedLines)
        | Error errors ->
            lines.Add($"{Colors.bold}❌ Parsing Errors:{Colors.reset}")
            let branch = TreeChars.branch
            errors |> List.iter (fun error -> 
                lines.Add(branch + " " + Colors.boolValue + error + Colors.reset))
                
        lines.Add("")
        lines |> Seq.toList
        
    /// Visualize JSON string directly (for query results)
    let visualizeJson (json: string) (format: VisualizationFormat) =
        try
            match JsonSerialization.tryParseResource json with
            | Ok (resourceType, resource) ->
                visualizeResource resourceType resource format
            | Error errors ->
                let errorHeader = [$"{Colors.bold}❌ Parsing Errors:{Colors.reset}"]
                let branch = TreeChars.branch
                let errorLines = errors |> List.map (fun error -> 
                    branch + " " + Colors.boolValue + error + Colors.reset)
                errorHeader @ errorLines
        with
        | ex -> [$"{Colors.bold}❌ Error: {ex.Message}{Colors.reset}"]
        
    /// Visualize from file
    let visualizeFile (filePath: string) (format: VisualizationFormat) =
        if not (IO.File.Exists filePath) then
            [$"{Colors.bold}❌ File not found: {formatString filePath}{Colors.reset}"]
        else
            let json = IO.File.ReadAllText filePath
            let header = [$"{Colors.bold}📁 File: {formatString filePath}{Colors.reset}"; ""]
            let content = visualizeFromJson json format
            header @ content