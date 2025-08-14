namespace FSharpFHIR

/// Observation resource
// See: https://hl7.org/fhir/observation.html

type Observation = {
    resource : DomainResource
    identifier : Identifier list
    status : ObservationStatus
    code : CodeableConcept option
    subject : Reference option
    effective : FhirDateTime option
    value : ElementValue option
}
and ObservationStatus =
    | Registered
    | Preliminary
    | Final
    | Amended
