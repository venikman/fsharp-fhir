namespace FSharpFHIR

/// MedicationAdministration resource
// See: https://hl7.org/fhir/medicationadministration.html

type MedicationAdministration = {
    resource : DomainResource
    identifier : Identifier list
    status : MedicationAdministrationStatus
    medication : CodeableConcept option
    subject : Reference option
    effective : FhirDateTime option
}
and MedicationAdministrationStatus =
    | InProgress
    | NotDone
    | OnHold
    | Completed
    | EnteredInError
    | Stopped
    | Unknown
