namespace FSharpFHIR

/// Immunization resource
// See: https://hl7.org/fhir/immunization.html

type Immunization = {
    resource : DomainResource
    identifier : Identifier list
    status : ImmunizationStatus
    vaccineCode : CodeableConcept
    patient : Reference
}
and ImmunizationStatus =
    | Completed
    | EnteredInError
    | NotDone
