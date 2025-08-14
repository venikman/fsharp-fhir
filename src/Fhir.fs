namespace FSharpFHIR

open System

/// Primitive FHIR types
// These aliases reflect the primitive data types defined by the FHIR specification
// See: https://hl7.org/fhir/datatypes.html

type FhirBoolean = bool

type FhirInteger = int

type FhirString = string

type FhirDecimal = decimal

type FhirUri = Uri

type FhirDateTime = DateTime

/// Base Element type that allows extensions
// Almost all FHIR structures derive from Element

type Element = {
    id : string option
    extension : Extension list
}
and Extension = {
    url : FhirUri
    value : ElementValue option
}
and ElementValue =
    | ValueBoolean of FhirBoolean
    | ValueInteger of FhirInteger
    | ValueString of FhirString
    | ValueDecimal of FhirDecimal
    | ValueDateTime of FhirDateTime
    | ValueUri of FhirUri

/// Meta information supported on all resources
// See: https://hl7.org/fhir/resource.html#Meta

type Meta = {
    versionId : string option
    lastUpdated : FhirDateTime option
    profile : FhirUri list
}

/// Narrative block
// See: https://hl7.org/fhir/narrative.html

type Narrative = {
    status : NarrativeStatus
    div : string
}
and NarrativeStatus =
    | Generated
    | Extensions
    | Additional
    | Empty

/// Generic Resource definition (minimal subset)

type Resource = {
    id : string option
    meta : Meta option
    language : string option
}

/// DomainResource extends Resource with narrative and extensions

type DomainResource = {
    resource : Resource
    text : Narrative option
    contained : DomainResource list
    extension : Extension list
    modifierExtension : Extension list
}

/// Identifier data type used by many resources
// See: https://hl7.org/fhir/datatypes.html#Identifier

type Identifier = {
    ``use`` : IdentifierUse option
    system : FhirUri option
    value : FhirString option
}
and IdentifierUse =
    | Usual
    | Official
    | Temp
    | Secondary
    | Old

/// HumanName data type
// See: https://hl7.org/fhir/datatypes.html#HumanName

type HumanName = {
    ``use`` : NameUse option
    text : FhirString option
    family : FhirString option
    given : FhirString list
}
and NameUse =
    | Usual
    | Official
    | Temp
    | Nickname
    | Anonymous
    | Old
    | Maiden

/// Administrative gender
// See: https://hl7.org/fhir/valueset-administrative-gender.html

type AdministrativeGender =
    | Male
    | Female
    | Other
    | Unknown

/// Coding data type
// See: https://hl7.org/fhir/datatypes.html#Coding

type Coding = {
    system : FhirUri option
    code : FhirString option
    display : FhirString option
}

/// CodeableConcept data type
// See: https://hl7.org/fhir/datatypes.html#CodeableConcept

type CodeableConcept = {
    coding : Coding list
    text : FhirString option
}

/// Reference data type
// See: https://hl7.org/fhir/references.html

type Reference = {
    reference : FhirString option
    display : FhirString option
}

/// Period data type
// See: https://hl7.org/fhir/datatypes.html#Period

type Period = {
    start : FhirDateTime option
    ``end`` : FhirDateTime option
}

