namespace FSharpFHIR

/// MedicationRequest resource
// See: https://hl7.org/fhir/medicationrequest.html

type MedicationRequest = {
    resource : DomainResource
    identifier : Identifier list
    status : MedicationRequestStatus
    intent : MedicationRequestIntent
    medication : CodeableConcept option
    subject : Reference option
}
and MedicationRequestStatus =
    | Active
    | OnHold
    | Cancelled
    | Completed
    | EnteredInError
    | Stopped
    | Draft
    | Unknown
and MedicationRequestIntent =
    | Order
    | Plan
    | Proposal
    | OriginalOrder
    | ReflexOrder
    | FillerOrder
    | InstanceOrder
    | Option
