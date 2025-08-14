namespace FSharpFHIR

/// Organization resource
// See: https://hl7.org/fhir/organization.html

type Organization = {
    resource : DomainResource
    identifier : Identifier list
    active : FhirBoolean option
    name : FhirString option
}
