namespace FSharpFHIR

/// DiagnosticReport resource
// See: https://hl7.org/fhir/diagnosticreport.html

type DiagnosticReport = {
    resource : DomainResource
    identifier : Identifier list
    status : DiagnosticReportStatus
    code : CodeableConcept option
    subject : Reference option
    effective : FhirDateTime option
    result : Reference list
}
and DiagnosticReportStatus =
    | Registered
    | Partial
    | Preliminary
    | Final
    | Amended
    | Corrected
    | Appended
    | Cancelled
    | EnteredInError
    | Unknown
