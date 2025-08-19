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

## Architecture

This project showcases modern F# development practices:

- **CLI Implementation**: Built with [Fli](https://github.com/CaptnCodr/Fli) using F# computation expressions for clean, declarative command definitions
- **Query Engine**: Implements a jq-like query language with functional parsing and execution
- **Interactive REPL**: Provides real-time query exploration with immediate feedback
- **Type Safety**: Leverages F#'s strong type system for FHIR resource validation
- **Functional Design**: Uses immutable data structures and functional programming patterns throughout

### Key Components

- **FSharpFHIR.QueryEngine**: Core query parsing and execution engine
- **FSharpFHIR.InteractiveRepl**: Interactive REPL implementation
- **FSharpFHIR.Validator**: FHIR resource validation logic
- **FSharpFHIR.Visualizer**: Human-readable FHIR resource formatting
- **CLI Tool**: Modern command-line interface using Fli computation expressions

## Installation

### Global Tool Installation (Recommended)

Install the CLI as a global dotnet tool from local source:

```bash
# Build and pack the project
dotnet pack cli/FSharpFHIR.Cli.fsproj

# Install the tool globally from the local package
dotnet tool install --global --add-source ./cli/bin/Debug FSharpFHIR.Cli
```

After installation, use the `fsharp-fhir` command directly:

```bash
fsharp-fhir --help
```

### Local Development

For development, you can run the CLI directly from source:

```bash
dotnet run --project cli -- --help
```

## CLI Usage

The CLI tool is built using the modern [Fli](https://github.com/CaptnCodr/Fli) library and provides several commands for working with FHIR resources:

### Validate Command

Validate FHIR resources against their schemas:

```bash
# Using global tool (recommended)
fsharp-fhir validate --file path/to/resource.json
fsharp-fhir validate --file path/to/resource.json --verbose

# Using local development
dotnet run --project cli -- validate --file path/to/resource.json
dotnet run --project cli -- validate --file path/to/resource.json --verbose
```

#### Example: Validating a Patient Resource

```bash
# Using global tool
fsharp-fhir validate --file examples/patient.json

# Using local development
dotnet run --project cli -- validate --file examples/patient.json
```

#### Example: Validating an Observation Resource

```bash
# Using global tool
fsharp-fhir validate --file examples/observation.json

# Using local development
dotnet run --project cli -- validate --file examples/observation.json
```

### Visualize Command

Visualize FHIR resources in a formatted, human-readable way:

```bash
# Using global tool
fsharp-fhir visualize --file examples/patient.json
fsharp-fhir visualize --file examples/patient.json --compact

# Using local development
dotnet run --project cli -- visualize --file examples/patient.json
dotnet run --project cli -- visualize --file examples/patient.json --compact
```

### Query Command

Query FHIR resources using jq-like syntax with powerful filtering and transformation capabilities:

```bash
# Using global tool
fsharp-fhir query --file examples/patient.json --query ".name"
fsharp-fhir query --file examples/patient.json --query ".name[0].given[0]"
fsharp-fhir query --file examples/patient.json --query ".name | select(.use == 'official')"
fsharp-fhir query --file examples/patient.json --query ".name | map(.given[0]) | sort"
fsharp-fhir query --file examples/patient.json --interactive

# Using local development
dotnet run --project cli -- query --file examples/patient.json --query ".name"
dotnet run --project cli -- query --file examples/patient.json --query ".name[0].given[0]"
dotnet run --project cli -- query --file examples/patient.json --query ".name | select(.use == 'official')"
dotnet run --project cli -- query --file examples/patient.json --query ".name | map(.given[0]) | sort"
dotnet run --project cli -- query --file examples/patient.json --interactive
```

#### Query Language Features

The query language supports jq-like syntax with the following operations:

- **Property Access**: `.property`, `.nested.property`
- **Array Operations**: `.[0]`, `.array[1]`, `.array[]`
- **Filtering**: `select(condition)`, `has("key")`, `contains("value")`
- **Transformations**: `map(expression)`, `sort`, `sort_by(expression)`
- **Aggregations**: `length`, `count`, `sum`, `min`, `max`, `average`
- **String Operations**: `startswith("prefix")`, `endswith("suffix")`, `split(",")`
- **Type Checking**: `type`, `isnull`, `isempty`, `issome`, `isnone`
- **Conditional Logic**: `if condition then expr else expr end`

#### Interactive REPL Mode

The interactive mode provides a powerful REPL (Read-Eval-Print Loop) for exploring FHIR data:

```bash
# Using global tool
fsharp-fhir query --file examples/patient.json --interactive

# Using local development
dotnet run --project cli -- query --file examples/patient.json --interactive

# In the REPL, you can:
# - Type queries and see results immediately
# - Use :help for available commands
# - Use :quit to exit
# - Explore data structure interactively
```

### Create Command

Create new FHIR resources interactively:

```bash
# Using global tool
fsharp-fhir create

# Using local development
dotnet run --project cli -- create

# Generate a minimal Patient resource as JSON (creator tool)
dotnet run --project creator -- create patient --name "Jane Doe" --gender female --birthDate 1980-01-01
```

The JSON is printed to stdout. Supply `--out path/to/file.json` to write it to disk.

### Help

Get help for any command:

```bash
# Using global tool
fsharp-fhir --help
fsharp-fhir validate --help
fsharp-fhir visualize --help
fsharp-fhir query --help
fsharp-fhir create --help

# Using local development
dotnet run --project cli -- --help
dotnet run --project cli -- validate --help
dotnet run --project cli -- visualize --help
dotnet run --project cli -- query --help
dotnet run --project cli -- create --help

# Creator CLI help
dotnet run --project creator -- --help
dotnet run --project creator -- create patient --help
```

## Supported Resource Types

- **Patient** - Validates name structure, gender, birthDate, and other fields
- **Observation** - Validates required status field and optional fields

## Validation Examples

The CLI validator tests FHIR resources by parsing JSON into F# type definitions and validating structure, required fields, and data types.

To validate a JSON file containing a FHIR resource run:

```bash
# Using global tool
fsharp-fhir validate --file path/to/resource.json [--verbose]

# Using local development
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file path/to/resource.json [--verbose]
```

If the resource is valid the tool prints `✓ Resource is valid`; otherwise it lists the validation errors.

Validate the provided sample patient resource:

```bash
# Using global tool
fsharp-fhir validate --file examples/patient.json

# Using local development
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file examples/patient.json
```

This should output:

```text
✓ Resource is valid
```

For detailed information about the validated resource, use `--verbose`:

```bash
# Using global tool
fsharp-fhir validate --file examples/patient.json --verbose

# Using local development
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file examples/patient.json --verbose
```

Output:
```text
✓ Resource is valid
  Resource type: Patient
  Details: Patient resource with name: No name specified
```

**Additional Validation Examples:**

Valid Patient with name:
```bash
echo '{"resourceType":"Patient","name":[{"text":"John Doe"}],"gender":"male"}' > test-patient.json
# Using global tool
fsharp-fhir validate --file test-patient.json --verbose
# Using local development
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file test-patient.json --verbose
```

Valid Observation:
```bash
echo '{"resourceType":"Observation","status":"final"}' > test-observation.json
# Using global tool
fsharp-fhir validate --file test-observation.json --verbose
# Using local development
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file test-observation.json --verbose
```

Invalid Observation (missing required status):
```bash
echo '{"resourceType":"Observation","id":"test"}' > invalid-observation.json
# Using global tool
fsharp-fhir validate --file invalid-observation.json
# Using local development
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet run --project cli -- validate --file invalid-observation.json
```

Output:
```text
✗ Validation errors:
  - Missing required 'status' property
```

## Examples

### Basic Query Examples

```bash
# Using global tool
fsharp-fhir query --file examples/patient.json --query ".name[0].given[0]"
fsharp-fhir query --file examples/patient.json --query "select(.active == true)"
fsharp-fhir query --file examples/patient.json --query ".name | length"
fsharp-fhir query --file examples/patient.json --query ".birthDate | split('-')[0]"

# Using local development
dotnet run --project cli -- query --file examples/patient.json --query ".name[0].given[0]"
dotnet run --project cli -- query --file examples/patient.json --query "select(.active == true)"
dotnet run --project cli -- query --file examples/patient.json --query ".name | length"
dotnet run --project cli -- query --file examples/patient.json --query ".birthDate | split('-')[0]"
```

### Advanced Query Examples

```bash
# Using global tool
fsharp-fhir query --file examples/patient.json --query ".name | map(select(.use == 'official')) | map(.given[0])"
fsharp-fhir query --file examples/patient.json --query "if .active then .name[0].given[0] else 'Inactive' end"
fsharp-fhir query --file examples/patient.json --query ".telecom | map(select(.system == 'email')) | map(.value)"

# Using local development
dotnet run --project cli -- query --file examples/patient.json --query ".name | map(select(.use == 'official')) | map(.given[0])"
dotnet run --project cli -- query --file examples/patient.json --query "if .active then .name[0].given[0] else 'Inactive' end"
dotnet run --project cli -- query --file examples/patient.json --query ".telecom | map(select(.system == 'email')) | map(.value)"
```

### Interactive Session Example

```bash
# Using global tool
$ fsharp-fhir query --file examples/patient.json --interactive
FHIR Query REPL - Type :help for commands
> .name
[
  {
    "use": "official",
    "family": "Doe",
    "given": ["John"]
  }
]
> .name[0].given[0]
"John"
> :quit

# Using local development
$ dotnet run --project cli -- query --file examples/patient.json --interactive
# (same interactive session as above)
```

## Development

### Prerequisites

- .NET 8 SDK
- F# development environment (VS Code with Ionide, Visual Studio, or JetBrains Rider)

### Project Structure

```
fsharp-fhir/
├── src/                    # Core FHIR library
│   ├── QueryEngine.fs      # jq-like query language implementation
│   ├── InteractiveRepl.fs  # Interactive REPL functionality
│   ├── Validator.fs        # FHIR resource validation
│   ├── Visualizer.fs       # Human-readable formatting
│   └── Fhir.fs            # FHIR type definitions
├── cli/                    # CLI tool using Fli
│   └── Program.fs          # Command-line interface
├── creator/                # Resource creation tool
│   └── Program.fs          # Interactive resource creator
└── examples/               # Sample FHIR resources
    ├── patient.json
    └── observation.json
```

### Building and Testing

```bash
# Build all projects
dotnet build

# Run tests (if available)
dotnet test

# Package and install as global tool
dotnet pack cli/FSharpFHIR.Cli.fsproj
dotnet tool install --global --add-source ./cli/bin/Debug fsharp-fhir

# Run the CLI tool (global)
fsharp-fhir --help
fsharp-fhir query --file examples/patient.json --query ".name"

# Run the CLI tool (local development)
dotnet run --project cli
dotnet run --project cli -- query --file examples/patient.json --query ".name"
```

### Contributing

Contributions are welcome! This project demonstrates:

- Modern F# development practices
- Functional programming patterns
- Type-safe FHIR resource handling
- Interactive CLI development with Fli
- Query language implementation

Feel free to submit issues, feature requests, or pull requests to improve the project.

