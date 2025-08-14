namespace FSharpFHIR

/// Condition resource
// See: https://hl7.org/fhir/condition.html

type Condition = {
    resource : DomainResource
    identifier : Identifier list
    clinicalStatus : CodeableConcept option
    verificationStatus : CodeableConcept option
    code : CodeableConcept option
    subject : Reference option
}
