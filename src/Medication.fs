namespace FSharpFHIR

/// Medication resource
// See: https://hl7.org/fhir/medication.html

type Medication = {
    resource : DomainResource
    code : CodeableConcept option
}
