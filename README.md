# FSharpFHIR

This repository provides a foundational set of tools for working with FHIR (Fast Healthcare Interoperability Resources) using the F# programming language.

## Components

The repository is composed of three main parts:

1.  **Core F# Library (`src/`)**: The heart of the project. It defines a domain model by translating parts of the official FHIR specification into F# types. This includes definitions for various FHIR resources like `Patient`, `Observation`, and `Condition`.

2.  **Validation CLI (`cli/`)**: A command-line tool that performs basic validation on a FHIR resource stored in a JSON file. It checks that the file contains valid JSON and that the JSON has a `resourceType` field.

3.  **Resource Creation CLI (`creator/`)**: Another command-line tool that can generate a simple `Patient` resource as a JSON object.

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

To validate a JSON file containing a FHIR resource run:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file path/to/resource.json
```

If the resource is valid the tool prints `Resource is valid`; otherwise it lists the validation errors.

### Example

Validate the provided sample patient resource:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file examples/patient.json
```

This should output:

```text
Resource is valid
```

### Create a resource

Generate a minimal `Patient` resource as JSON:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project creator -- create patient --name "Jane Doe" --gender female --birthDate 1980-01-01
```

The JSON is printed to stdout. Supply `--out path/to/file.json` to write it to disk.
