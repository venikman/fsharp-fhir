namespace FSharpFHIR

/// Encounter resource
// See: https://hl7.org/fhir/encounter.html

type Encounter = {
    resource : DomainResource
    identifier : Identifier list
    status : EncounterStatus
    class : Coding option
    subject : Reference option
    period : Period option
}
and EncounterStatus =
    | Planned
    | InProgress
    | OnHold
    | Discharged
    | Completed
    | Cancelled
    | EnteredInError
    | Unknown
