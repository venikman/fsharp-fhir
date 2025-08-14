namespace FSharpFHIR

/// AllergyIntolerance resource
// See: https://hl7.org/fhir/allergyintolerance.html

type AllergyIntolerance = {
    resource : DomainResource
    identifier : Identifier list
    clinicalStatus : CodeableConcept option
    verificationStatus : CodeableConcept option
    code : CodeableConcept option
    patient : Reference
}
