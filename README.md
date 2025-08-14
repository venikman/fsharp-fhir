# FSharpFHIR

A basic FHIR domain model and CLI validator written in F#.

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

The CLIs use the [Charm](https://charm.land/) library.

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

