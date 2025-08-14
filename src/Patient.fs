namespace FSharpFHIR

/// Simple Patient resource representation
// This is a minimal subset of the Patient definition.
// Full definition includes many more fields. See: https://hl7.org/fhir/patient.html

type Patient = {
    resource : DomainResource
    identifier : Identifier list
    active : FhirBoolean option
    name : HumanName list
    gender : AdministrativeGender option
    birthDate : FhirDateTime option
}
