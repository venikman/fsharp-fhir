namespace FSharpFHIR

/// Procedure resource
// See: https://hl7.org/fhir/procedure.html

type Procedure = {
    resource : DomainResource
    identifier : Identifier list
    status : ProcedureStatus
    code : CodeableConcept option
    subject : Reference option
    performed : FhirDateTime option
}
and ProcedureStatus =
    | Preparation
    | InProgress
    | Completed
    | EnteredInError
    | Stopped
    | Unknown
