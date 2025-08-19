module FhirResourceTests

open Expecto
open FSharpFHIR
open FSharpFHIR.Types
open TestHelpers
open System

[<Tests>]
let fhirResourceTests =
    testList "FHIR Resource Tests" [
        
        testList "Primitive Types Tests" [
            test "FhirBoolean should work as bool" {
                let value: FhirBoolean = true
                Expect.isTrue value "FhirBoolean should be true"
                let falseValue: FhirBoolean = false
                Expect.isFalse falseValue "FhirBoolean should be false"
            }
            
            test "FhirInteger should work as int" {
                let value: FhirInteger = 42
                Expect.equal value 42 "FhirInteger should equal 42"
                let negativeValue: FhirInteger = -10
                Expect.equal negativeValue -10 "FhirInteger should handle negative values"
            }
            
            test "FhirString should work as string" {
                let value: FhirString = "test"
                Expect.equal value "test" "FhirString should equal 'test'"
                let emptyValue: FhirString = ""
                Expect.equal emptyValue "" "FhirString should handle empty strings"
            }
            
            test "FhirDecimal should work as decimal" {
                let value: FhirDecimal = 3.14m
                Expect.equal value 3.14m "FhirDecimal should equal 3.14"
                let zeroValue: FhirDecimal = 0m
                Expect.equal zeroValue 0m "FhirDecimal should handle zero"
            }
            
            test "FhirUri should work as Uri" {
                let value: FhirUri = Uri("https://example.com")
                Expect.equal value.Host "example.com" "FhirUri should have correct host"
                Expect.equal value.Scheme "https" "FhirUri should have correct scheme"
            }
            
            test "FhirDateTime should work as DateTime" {
                let value: FhirDateTime = DateTime(2023, 12, 25, 10, 30, 0)
                Expect.equal value.Year 2023 "FhirDateTime should have correct year"
                Expect.equal value.Month 12 "FhirDateTime should have correct month"
                Expect.equal value.Day 25 "FhirDateTime should have correct day"
            }
        ]
        
        testList "ElementValue Tests" [
            test "ElementValue should handle boolean values" {
                let value = ValueBoolean true
                match value with
                | ValueBoolean b -> Expect.isTrue b "Should extract boolean value"
                | _ -> failtest "Should be ValueBoolean"
            }
            
            test "ElementValue should handle integer values" {
                let value = ValueInteger 42
                match value with
                | ValueInteger i -> Expect.equal i 42 "Should extract integer value"
                | _ -> failtest "Should be ValueInteger"
            }
            
            test "ElementValue should handle string values" {
                let value = ValueString "test"
                match value with
                | ValueString s -> Expect.equal s "test" "Should extract string value"
                | _ -> failtest "Should be ValueString"
            }
            
            test "ElementValue should handle decimal values" {
                let value = ValueDecimal 3.14m
                match value with
                | ValueDecimal d -> Expect.equal d 3.14m "Should extract decimal value"
                | _ -> failtest "Should be ValueDecimal"
            }
            
            test "ElementValue should handle DateTime values" {
                let dateTime = DateTime(2023, 12, 25)
                let value = ValueDateTime dateTime
                match value with
                | ValueDateTime dt -> Expect.equal dt dateTime "Should extract DateTime value"
                | _ -> failtest "Should be ValueDateTime"
            }
            
            test "ElementValue should handle Uri values" {
                let uri = Uri("https://example.com")
                let value = ValueUri uri
                match value with
                | ValueUri u -> Expect.equal u uri "Should extract Uri value"
                | _ -> failtest "Should be ValueUri"
            }
        ]
        
        testList "Enum Tests" [
            test "AdministrativeGender should have all expected values" {
                let genders = [Male; Female; Other; Unknown]
                Expect.hasLength genders 4 "Should have 4 gender values"
                
                // Test string representation
                Expect.equal (Male.ToString()) "Male" "Male should stringify correctly"
                Expect.equal (Female.ToString()) "Female" "Female should stringify correctly"
                Expect.equal (Other.ToString()) "Other" "Other should stringify correctly"
                Expect.equal (Unknown.ToString()) "Unknown" "Unknown should stringify correctly"
            }
            
            test "ObservationStatus should have all expected values" {
                let statuses = [Registered; Preliminary; Final; Amended]
                Expect.hasLength statuses 4 "Should have 4 observation status values"
                
                // Test string representation
                Expect.equal (Registered.ToString()) "Registered" "Registered should stringify correctly"
                Expect.equal (Preliminary.ToString()) "Preliminary" "Preliminary should stringify correctly"
                Expect.equal (Final.ToString()) "Final" "Final should stringify correctly"
                Expect.equal (Amended.ToString()) "Amended" "Amended should stringify correctly"
            }
            
            test "IdentifierUse should have all expected values" {
                let uses = [Usual; Official; Temp; Secondary; Old]
                Expect.hasLength uses 5 "Should have 5 identifier use values"
            }
            
            test "NameUse should have all expected values" {
                let uses = [NameUse.Usual; NameUse.Official; NameUse.Temp; Nickname; Anonymous; NameUse.Old; Maiden]
                Expect.hasLength uses 7 "Should have 7 name use values"
            }
            
            test "NarrativeStatus should have all expected values" {
                let statuses = [Generated; Extensions; Additional; Empty]
                Expect.hasLength statuses 4 "Should have 4 narrative status values"
            }
        ]
        
        testList "Data Type Construction Tests" [
            test "Element should construct with optional fields" {
                let element = {
                    id = Some "test-id"
                    extension = []
                }
                Expect.equal element.id (Some "test-id") "Should have correct ID"
                Expect.isEmpty element.extension "Should have empty extensions"
            }
            
            test "Extension should construct properly" {
                let extension = {
                    url = Uri("https://example.com/extension")
                    value = Some (ValueString "test")
                }
                Expect.equal extension.url.Host "example.com" "Should have correct URL"
                match extension.value with
                | Some (ValueString s) -> Expect.equal s "test" "Should have correct value"
                | _ -> failtest "Should have string value"
            }
            
            test "Meta should construct with optional fields" {
                let meta = {
                    versionId = Some "1"
                    lastUpdated = Some (DateTime(2023, 12, 25))
                    profile = [Uri("https://example.com/profile")]
                }
                Expect.equal meta.versionId (Some "1") "Should have correct version ID"
                Expect.isSome meta.lastUpdated "Should have last updated"
                Expect.hasLength meta.profile 1 "Should have one profile"
            }
            
            test "Narrative should construct properly" {
                let narrative = {
                    status = Generated
                    div = "<div>Test narrative</div>"
                }
                Expect.equal narrative.status Generated "Should have Generated status"
                Expect.equal narrative.div "<div>Test narrative</div>" "Should have correct div content"
            }
            
            test "Resource should construct with optional fields" {
                let resource = {
                    id = Some "test-resource"
                    meta = None
                    language = Some "en"
                }
                Expect.equal resource.id (Some "test-resource") "Should have correct ID"
                Expect.isNone resource.meta "Should have no meta"
                Expect.equal resource.language (Some "en") "Should have correct language"
            }
            
            test "DomainResource should construct properly" {
                let baseResource = {
                    id = Some "domain-test"
                    meta = None
                    language = None
                }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                Expect.equal domainResource.resource.id (Some "domain-test") "Should have correct base resource ID"
                Expect.isEmpty domainResource.contained "Should have no contained resources"
                Expect.isEmpty domainResource.extension "Should have no extensions"
            }
            
            test "Identifier should construct with optional fields" {
                let identifier = {
                    ``use`` = Some Official
                    system = Some (Uri("https://example.com/ids"))
                    value = Some "12345"
                }
                Expect.equal identifier.``use`` (Some Official) "Should have Official use"
                Expect.isSome identifier.system "Should have system"
                Expect.equal identifier.value (Some "12345") "Should have correct value"
            }
            
            test "HumanName should construct with lists" {
                let name = {
                    ``use`` = Some NameUse.Official
                    text = Some "John Doe"
                    family = Some "Doe"
                    given = ["John"; "Middle"]
                }
                Expect.equal name.``use`` (Some NameUse.Official) "Should have Official use"
                Expect.equal name.text (Some "John Doe") "Should have correct text"
                Expect.equal name.family (Some "Doe") "Should have correct family name"
                Expect.hasLength name.given 2 "Should have two given names"
                Expect.contains name.given "John" "Should contain first given name"
                Expect.contains name.given "Middle" "Should contain middle name"
            }
            
            test "Coding should construct properly" {
                let coding = {
                    system = Some (Uri("https://loinc.org"))
                    code = Some "8302-2"
                    display = Some "Body height"
                }
                Expect.isSome coding.system "Should have system"
                Expect.equal coding.code (Some "8302-2") "Should have correct code"
                Expect.equal coding.display (Some "Body height") "Should have correct display"
            }
            
            test "CodeableConcept should construct with coding list" {
                let coding = {
                    system = Some (Uri("https://loinc.org"))
                    code = Some "8302-2"
                    display = Some "Body height"
                }
                let concept = {
                    coding = [coding]
                    text = Some "Height measurement"
                }
                Expect.hasLength concept.coding 1 "Should have one coding"
                Expect.equal concept.text (Some "Height measurement") "Should have correct text"
            }
            
            test "Reference should construct properly" {
                let reference = {
                    reference = Some "Patient/123"
                    display = Some "John Doe"
                }
                Expect.equal reference.reference (Some "Patient/123") "Should have correct reference"
                Expect.equal reference.display (Some "John Doe") "Should have correct display"
            }
            
            test "Period should construct with start and end" {
                let startDate = DateTime(2023, 1, 1)
                let endDate = DateTime(2023, 12, 31)
                let period = {
                    start = Some startDate
                    ``end`` = Some endDate
                }
                Expect.equal period.start (Some startDate) "Should have correct start date"
                Expect.equal period.``end`` (Some endDate) "Should have correct end date"
            }
        ]
        
        testList "Patient Resource Tests" [
            test "Patient should construct with all fields" {
                let baseResource = {
                    id = Some "patient-123"
                    meta = None
                    language = Some "en"
                }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                let name = {
                    ``use`` = Some NameUse.Official
                    text = Some "John Doe"
                    family = Some "Doe"
                    given = ["John"]
                }
                let patient = {
                    resource = domainResource
                    identifier = []
                    active = Some true
                    name = [name]
                    gender = Some Male
                    birthDate = Some (DateTime(1990, 1, 1))
                }
                
                Expect.equal patient.resource.resource.id (Some "patient-123") "Should have correct ID"
                Expect.equal patient.active (Some true) "Should be active"
                Expect.hasLength patient.name 1 "Should have one name"
                Expect.equal patient.name.[0].family (Some "Doe") "Should have correct family name"
                Expect.equal patient.gender (Some Male) "Should have male gender"
                Expect.isSome patient.birthDate "Should have birth date"
            }
            
            test "Patient should handle minimal construction" {
                let baseResource = {
                    id = None
                    meta = None
                    language = None
                }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                let patient = {
                    resource = domainResource
                    identifier = []
                    active = None
                    name = []
                    gender = None
                    birthDate = None
                }
                
                Expect.isNone patient.resource.resource.id "Should have no ID"
                Expect.isNone patient.active "Should have no active status"
                Expect.isEmpty patient.name "Should have no names"
                Expect.isNone patient.gender "Should have no gender"
                Expect.isNone patient.birthDate "Should have no birth date"
            }
            
            test "Patient should handle multiple names" {
                let baseResource = { id = None; meta = None; language = None }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                let officialName = {
                    ``use`` = Some NameUse.Official
                    text = Some "John Doe"
                    family = Some "Doe"
                    given = ["John"]
                }
                let nickname = {
                    ``use`` = Some Nickname
                    text = Some "Johnny"
                    family = None
                    given = ["Johnny"]
                }
                let patient = {
                    resource = domainResource
                    identifier = []
                    active = Some true
                    name = [officialName; nickname]
                    gender = None
                    birthDate = None
                }
                
                Expect.hasLength patient.name 2 "Should have two names"
                Expect.equal patient.name.[0].``use`` (Some NameUse.Official) "First name should be official"
                Expect.equal patient.name.[1].``use`` (Some Nickname) "Second name should be nickname"
            }
        ]
        
        testList "Observation Resource Tests" [
            test "Observation should construct with required fields" {
                let baseResource = {
                    id = Some "obs-123"
                    meta = None
                    language = None
                }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                let observation = {
                    resource = domainResource
                    identifier = []
                    status = Final
                    code = None
                    subject = None
                    effective = Some (DateTime(2023, 12, 25))
                    value = None
                }
                
                Expect.equal observation.resource.resource.id (Some "obs-123") "Should have correct ID"
                Expect.equal observation.status Final "Should have Final status"
                Expect.isSome observation.effective "Should have effective date"
                Expect.isNone observation.code "Should have no code"
                Expect.isNone observation.subject "Should have no subject"
            }
            
            test "Observation should handle all status values" {
                let baseResource = { id = None; meta = None; language = None }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                
                let statuses = [Registered; Preliminary; Final; Amended]
                for status in statuses do
                    let observation = {
                        resource = domainResource
                        identifier = []
                        status = status
                        code = None
                        subject = None
                        effective = None
                        value = None
                    }
                    Expect.equal observation.status status $"Should have {status} status"
            }
            
            test "Observation should handle complex structure" {
                let baseResource = {
                    id = Some "complex-obs"
                    meta = None
                    language = None
                }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                let coding = {
                    system = Some (Uri("https://loinc.org"))
                    code = Some "8302-2"
                    display = Some "Body height"
                }
                let code = {
                    coding = [coding]
                    text = Some "Height"
                }
                let subject = {
                    reference = Some "Patient/123"
                    display = Some "John Doe"
                }
                let observation = {
                    resource = domainResource
                    identifier = []
                    status = Final
                    code = Some code
                    subject = Some subject
                    effective = Some (DateTime(2023, 12, 25))
                    value = Some (ValueDecimal 180.5m)
                }
                
                Expect.isSome observation.code "Should have code"
                Expect.isSome observation.subject "Should have subject"
                Expect.isSome observation.value "Should have value"
                match observation.value with
                | Some (ValueDecimal d) -> Expect.equal d 180.5m "Should have correct decimal value"
                | _ -> failtest "Should have decimal value"
            }
        ]
        
        testList "Resource Validation Tests" [
            test "Resources should maintain referential integrity" {
                // Test that references between resources are properly typed
                let patientRef = {
                    reference = Some "Patient/123"
                    display = Some "John Doe"
                }
                
                let baseResource = { id = None; meta = None; language = None }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                
                let observation = {
                    resource = domainResource
                    identifier = []
                    status = Final
                    code = None
                    subject = Some patientRef
                    effective = None
                    value = None
                }
                
                match observation.subject with
                | Some ref -> 
                    Expect.equal ref.reference (Some "Patient/123") "Should reference correct patient"
                    Expect.equal ref.display (Some "John Doe") "Should have correct display"
                | None -> failtest "Should have subject reference"
            }
            
            test "Extensions should be properly typed" {
                let extension = {
                    url = Uri("https://example.com/extension")
                    value = Some (ValueString "test-value")
                }
                
                let baseResource = { id = None; meta = None; language = None }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = [extension]
                    modifierExtension = []
                }
                
                Expect.hasLength domainResource.extension 1 "Should have one extension"
                let ext = domainResource.extension.[0]
                Expect.equal ext.url.Host "example.com" "Should have correct extension URL"
                match ext.value with
                | Some (ValueString s) -> Expect.equal s "test-value" "Should have correct extension value"
                | _ -> failtest "Should have string extension value"
            }
        ]
        
        testList "Edge Cases Tests" [
            test "Empty lists should be handled properly" {
                let baseResource = { id = None; meta = None; language = None }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                let patient = {
                    resource = domainResource
                    identifier = []
                    active = None
                    name = []
                    gender = None
                    birthDate = None
                }
                
                Expect.isEmpty patient.identifier "Should handle empty identifier list"
                Expect.isEmpty patient.name "Should handle empty name list"
                Expect.isEmpty patient.resource.extension "Should handle empty extension list"
            }
            
            test "Optional fields should handle None values" {
                let baseResource = { id = None; meta = None; language = None }
                let domainResource = {
                    resource = baseResource
                    text = None
                    contained = []
                    extension = []
                    modifierExtension = []
                }
                
                Expect.isNone baseResource.id "Should handle None ID"
                Expect.isNone baseResource.meta "Should handle None meta"
                Expect.isNone baseResource.language "Should handle None language"
                Expect.isNone domainResource.text "Should handle None text"
            }
            
            test "Complex nested structures should work" {
                let extension = {
                    url = Uri("https://example.com/nested")
                    value = Some (ValueString "nested-value")
                }
                let meta = {
                    versionId = Some "1"
                    lastUpdated = Some (DateTime.Now)
                    profile = [Uri("https://example.com/profile")]
                }
                let narrative = {
                    status = Generated
                    div = "<div>Generated narrative</div>"
                }
                let baseResource = {
                    id = Some "complex-resource"
                    meta = Some meta
                    language = Some "en-US"
                }
                let domainResource = {
                    resource = baseResource
                    text = Some narrative
                    contained = []
                    extension = [extension]
                    modifierExtension = []
                }
                
                Expect.isSome domainResource.resource.meta "Should have meta"
                Expect.isSome domainResource.text "Should have narrative"
                Expect.hasLength domainResource.extension 1 "Should have extension"
                
                match domainResource.resource.meta with
                | Some m -> 
                    Expect.equal m.versionId (Some "1") "Should have correct version"
                    Expect.hasLength m.profile 1 "Should have profile"
                | None -> failtest "Should have meta"
            }
        ]
    ]