namespace FSharpFHIR

/// Appointment resource
// See: https://hl7.org/fhir/appointment.html

type Appointment = {
    resource : DomainResource
    identifier : Identifier list
    status : AppointmentStatus
    start : FhirDateTime option
    end_ : FhirDateTime option
    participant : Reference list
}
and AppointmentStatus =
    | Proposed
    | Pending
    | Booked
    | Arrived
    | Fulfilled
    | Cancelled
    | NoShow
    | EnteredInError
    | CheckedIn
    | Waitlist
