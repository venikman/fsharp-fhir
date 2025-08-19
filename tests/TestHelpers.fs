module TestHelpers

open System
open FSharpFHIR.Types
open FSharpFHIR.JsonSerialization

// Sample FHIR Patient resource for testing
let samplePatientJson = """
{
  "resourceType": "Patient",
  "id": "example",
  "active": true,
  "name": [
    {
      "use": "official",
      "family": "Chalmers",
      "given": ["Peter", "James"]
    }
  ],
  "telecom": [
    {
      "system": "phone",
      "value": "(03) 5555 6473",
      "use": "work",
      "rank": 1
    }
  ],
  "gender": "male",
  "birthDate": "1974-12-25",
  "address": [
    {
      "use": "home",
      "type": "both",
      "text": "534 Erewhon St PeasantVille, Rainbow, Vic  3999",
      "line": ["534 Erewhon St"],
      "city": "PleasantVille",
      "district": "Rainbow",
      "state": "Vic",
      "postalCode": "3999",
      "period": {
        "start": "1974-12-25"
      }
    }
  ]
}"""

// Sample Observation resource for testing
let sampleObservationJson = """
{
  "resourceType": "Observation",
  "id": "example",
  "status": "final",
  "category": [
    {
      "coding": [
        {
          "system": "http://terminology.hl7.org/CodeSystem/observation-category",
          "code": "vital-signs",
          "display": "Vital Signs"
        }
      ]
    }
  ],
  "code": {
    "coding": [
      {
        "system": "http://loinc.org",
        "code": "29463-7",
        "display": "Body Weight"
      }
    ]
  },
  "subject": {
    "reference": "Patient/example"
  },
  "effectiveDateTime": "2016-03-28",
  "valueQuantity": {
    "value": 185,
    "unit": "lbs",
    "system": "http://unitsofmeasure.org",
    "code": "[lb_av]"
  }
}"""

// Invalid JSON for testing error handling
let invalidJson = """
{
  "resourceType": "Patient",
  "id": "invalid",
  "active": "not-a-boolean",
  "birthDate": "invalid-date"
}"""

// Helper function to create test Patient
let createTestPatient () =
    {
        Id = Some "test-patient"
        Active = Some true
        Name = Some [
            {
                Use = Some "official"
                Family = Some "TestFamily"
                Given = Some ["TestGiven"]
            }
        ]
        Telecom = None
        Gender = Some "male"
        BirthDate = Some "1990-01-01"
        Address = None
    }

// Helper function to create test Observation
let createTestObservation () =
    {
        Id = Some "test-observation"
        Status = "final"
        Category = None
        Code = {
            Coding = Some [
                {
                    System = Some "http://loinc.org"
                    Code = Some "29463-7"
                    Display = Some "Body Weight"
                }
            ]
        }
        Subject = Some { Reference = Some "Patient/test-patient" }
        EffectiveDateTime = Some "2023-01-01"
        ValueQuantity = Some {
            Value = Some 70.0
            Unit = Some "kg"
            System = Some "http://unitsofmeasure.org"
            Code = Some "kg"
        }
    }

// Helper function to assert JSON equality (ignoring whitespace)
let assertJsonEqual expected actual =
    let normalize (json: string) =
        json.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "")
    let normalizedExpected = normalize expected
    let normalizedActual = normalize actual
    normalizedExpected = normalizedActual

// Helper function to measure execution time
let measureTime (action: unit -> 'T) =
    let stopwatch = System.Diagnostics.Stopwatch.StartNew()
    let result = action()
    stopwatch.Stop()
    (result, stopwatch.ElapsedMilliseconds)