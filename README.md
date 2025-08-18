# FSharpFHIR

A FHIR domain model and CLI validator written in F# that validates JSON resources against F# type definitions.

## Building

This repository targets .NET 8. Build the library or CLIs with:

```bash
# build the domain model library
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet build src/FSharpFHIR.fsproj

# build the validator CLI
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet build cli/FSharpFHIR.Cli.fsproj

# build the resource creator CLI
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet build creator/FSharpFHIR.Creator.fsproj
```

## CLI Usage

### Validate a resource

The CLI validator tests FHIR resources by parsing JSON into F# type definitions and validating structure, required fields, and data types.

To validate a JSON file containing a FHIR resource run:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file path/to/resource.json [--verbose]
```

If the resource is valid the tool prints `✓ Resource is valid`; otherwise it lists the validation errors.

**Supported Resource Types:**
- `Patient` - Validates name structure, gender, birthDate, and other fields
- `Observation` - Validates required status field and optional fields

### Examples

Validate the provided sample patient resource:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file examples/patient.json
```

This should output:

```text
✓ Resource is valid
```

For detailed information about the validated resource, use `--verbose`:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file examples/patient.json --verbose
```

Output:
```text
✓ Resource is valid
  Resource type: Patient
  Details: Patient resource with name: No name specified
```

**Validation Examples:**

Valid Patient with name:
```bash
echo '{"resourceType":"Patient","name":[{"text":"John Doe"}],"gender":"male"}' > test-patient.json
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file test-patient.json --verbose
```

Valid Observation:
```bash
echo '{"resourceType":"Observation","status":"final"}' > test-observation.json  
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file test-observation.json --verbose
```

Invalid Observation (missing required status):
```bash
echo '{"resourceType":"Observation","id":"test"}' > invalid-observation.json
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file invalid-observation.json
```

Output:
```text
✗ Validation errors:
  - Missing required 'status' property
```

### Create a resource

Generate a minimal `Patient` resource as JSON:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project creator -- create patient --name "Jane Doe" --gender female --birthDate 1980-01-01
```

The JSON is printed to stdout. Supply `--out path/to/file.json` to write it to disk.

### Help

For detailed usage information:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --help
```

