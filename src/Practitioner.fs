namespace FSharpFHIR

/// Practitioner resource
// See: https://hl7.org/fhir/practitioner.html

type Practitioner = {
    resource : DomainResource
    identifier : Identifier list
    active : FhirBoolean option
    name : HumanName list
}
