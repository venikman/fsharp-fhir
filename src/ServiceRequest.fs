namespace FSharpFHIR

/// ServiceRequest resource
// See: https://hl7.org/fhir/servicerequest.html

type ServiceRequest = {
    resource : DomainResource
    identifier : Identifier list
    status : ServiceRequestStatus
    intent : ServiceRequestIntent
    code : CodeableConcept option
    subject : Reference option
}
and ServiceRequestStatus =
    | Draft
    | Active
    | OnHold
    | Revoked
    | Completed
    | EnteredInError
    | Unknown
and ServiceRequestIntent =
    | Proposal
    | Plan
    | Directive
    | Order
    | OriginalOrder
    | ReflexOrder
    | FillerOrder
    | InstanceOrder
    | Option
