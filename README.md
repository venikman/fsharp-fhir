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

## Testing

This project includes a comprehensive test suite built with the [Expecto](https://github.com/haf/expecto) testing framework, providing high code coverage and reliability across all key modules.

### Running Tests

#### Basic Test Execution

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with minimal output
dotnet test --verbosity quiet

# Run tests and collect code coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### Expecto-Specific Commands

```bash
# Run tests directly with Expecto (from tests directory)
cd tests
dotnet run

# Run tests with specific filters
dotnet run -- --filter "QueryEngine"
dotnet run -- --filter "Validator"

# Run tests with parallel execution
dotnet run -- --parallel

# Run tests with summary output
dotnet run -- --summary
```

### Test Project Structure

The test suite is organized in the `tests/` directory with the following structure:

```
tests/
├── FSharpFHIR.Tests.fsproj    # Test project configuration
├── TestHelpers.fs             # Shared test utilities and helpers
├── QueryEngineTests.fs        # Query language parsing and execution tests
├── ValidatorTests.fs          # FHIR resource validation tests
├── VisualizerTests.fs         # Resource formatting and display tests
├── JsonSerializationTests.fs  # JSON parsing and serialization tests
├── FHIRResourceTests.fs       # FHIR resource type definition tests
├── IntegrationTests.fs        # End-to-end CLI and workflow tests
├── Program.fs                 # Test runner entry point
└── expecto.config             # Expecto configuration
```

### Test Categories

Tests are organized into several categories for targeted execution:

#### Unit Tests
- **QueryEngine Tests**: Query parsing, AST generation, and execution logic
- **Validator Tests**: FHIR resource validation rules and error handling
- **Visualizer Tests**: Resource formatting and human-readable output
- **JsonSerialization Tests**: JSON parsing, serialization, and error cases
- **FHIR Resource Tests**: Type definitions, constraints, and data validation

#### Integration Tests
- **CLI Command Tests**: End-to-end testing of validate, visualize, query, and create commands
- **File I/O Tests**: Reading and writing FHIR resources from/to files
- **Error Handling Tests**: Comprehensive error scenarios and user feedback
- **Performance Tests**: Large resource handling and query performance

#### Running Specific Test Categories

```bash
# Run unit tests only
dotnet test --filter "Category=Unit"

# Run integration tests only
dotnet test --filter "Category=Integration"

# Run performance tests only
dotnet test --filter "Category=Performance"

# Run tests for specific modules
dotnet test --filter "FullyQualifiedName~QueryEngine"
dotnet test --filter "FullyQualifiedName~Validator"
dotnet test --filter "FullyQualifiedName~Visualizer"
```

### Running Specific Test Modules

```bash
# Run QueryEngine tests only
cd tests
dotnet run -- --filter "QueryEngine"

# Run Validator tests only
dotnet run -- --filter "Validator"

# Run Integration tests only
dotnet run -- --filter "Integration"

# Run tests matching a pattern
dotnet run -- --filter "*Patient*"
dotnet run -- --filter "*JSON*"
```

### Test Coverage

The test suite provides comprehensive coverage across:

- **Query Language**: 95%+ coverage of query parsing, AST operations, and execution paths
- **Validation Logic**: 90%+ coverage of FHIR validation rules and error conditions
- **Resource Types**: 100% coverage of supported FHIR resource type definitions
- **CLI Commands**: 85%+ coverage of command-line interface functionality
- **Error Handling**: Comprehensive testing of error scenarios and user feedback

#### Generating Coverage Reports

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Generate HTML coverage report (requires reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/html" -reporttypes:Html
```

### Adding New Tests

When adding new functionality, follow these guidelines for test development:

#### 1. Unit Tests

```fsharp
// Add to appropriate test file (e.g., QueryEngineTests.fs)
open Expecto
open FSharpFHIR.QueryEngine

[<Tests>]
let newFeatureTests =
    testList "New Feature Tests" [
        test "should handle basic case" {
            let result = newFunction "input"
            Expect.equal result "expected" "Should return expected value"
        }
        
        test "should handle edge case" {
            let result = newFunction ""
            Expect.isError result "Should return error for empty input"
        }
    ]
```

#### 2. Integration Tests

```fsharp
// Add to IntegrationTests.fs
test "new CLI command should work end-to-end" {
    let tempFile = createTempFile validInput
    try
        let (exitCode, output, _) = runCliCommand ["new-command"; tempFile]
        Expect.equal exitCode 0 "Should exit with success code"
        Expect.contains output "expected output" "Should contain expected result"
    finally
        cleanupTempFile tempFile
}
```

#### 3. Test Helpers

Utilize the shared test helpers in `TestHelpers.fs`:

```fsharp
// Available helper functions
createTempFile content        // Create temporary test file
cleanupTempFile path         // Clean up temporary file
runCliCommand args           // Execute CLI command with arguments
assertJsonEqual expected actual  // Compare JSON with normalization
measureTime operation        // Measure execution time
```

### Continuous Integration

For CI/CD pipelines, use these commands:

```bash
# CI test execution with XML output
dotnet test --logger "trx;LogFileName=test-results.trx" --logger "console;verbosity=normal"

# CI with coverage and results
dotnet test --collect:"XPlat Code Coverage" --logger "trx" --results-directory ./test-results

# Fail fast on first test failure
dotnet test --logger "console;verbosity=normal" -- --fail-fast
```

#### GitHub Actions Example

```yaml
- name: Run Tests
  run: |
    dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx"
    
- name: Upload Test Results
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: test-results
    path: test-results/
```

### Test Configuration

The test suite can be configured via `tests/expecto.config`:

```
--parallel
--summary
--colours 256
```

For custom test execution, modify the configuration or pass arguments directly:

```bash
# Override configuration
cd tests
dotnet run -- --parallel --summary --filter "QueryEngine" --colours 256
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

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run specific test categories
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Performance"

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

## Testing

This project includes a comprehensive test suite built with the [Expecto](https://github.com/haf/expecto) testing framework, providing high code coverage and reliability.

### Test Structure

The test suite is organized into several categories:

```
tests/
├── QueryEngineTests.fs     # Query parsing and execution tests
├── ValidatorTests.fs       # FHIR resource validation tests
├── VisualizerTests.fs      # Formatting and display tests
├── JsonSerializationTests.fs # JSON parsing and serialization tests
├── FhirResourceTests.fs    # FHIR type definitions and construction tests
├── IntegrationTests.fs     # End-to-end CLI and workflow tests
├── TestHelpers.fs          # Shared test utilities and helpers
├── expecto.config          # Test runner configuration
└── FSharpFHIR.Tests.fsproj # Test project file
```

### Running Tests

#### Basic Test Execution

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests in parallel (default)
dotnet test --parallel

# Run tests sequentially (for debugging)
dotnet test --no-parallel
```

#### Test Filtering

```bash
# Run specific test categories
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Performance"

# Run tests for specific modules
dotnet test --filter "FullyQualifiedName~QueryEngine"
dotnet test --filter "FullyQualifiedName~Validator"
dotnet test --filter "FullyQualifiedName~JsonSerialization"

# Run tests matching a pattern
dotnet test --filter "DisplayName~Patient"
dotnet test --filter "DisplayName~Observation"
```

#### Advanced Test Options

```bash
# Generate test results in JUnit format
dotnet test --logger "junit;LogFilePath=test-results.xml"

# Generate test results in TRX format
dotnet test --logger "trx;LogFileName=test-results.trx"

# Run tests with code coverage (requires coverlet)
dotnet test --collect:"XPlat Code Coverage"

# Set memory and time limits
dotnet test --settings test.runsettings
```

### Test Categories

#### Unit Tests
Test individual functions and modules in isolation:
- **QueryEngine**: Query parsing, AST construction, execution logic
- **Validator**: FHIR resource validation rules and error handling
- **Visualizer**: Formatting functions and display options
- **JsonSerialization**: JSON parsing, type conversion, error handling
- **FHIR Resources**: Type construction, validation, and edge cases

#### Integration Tests
Test complete workflows and CLI functionality:
- **CLI Commands**: Validate, visualize, and query commands
- **End-to-End Workflows**: Full resource processing pipelines
- **Error Handling**: Invalid inputs and edge cases
- **File Operations**: Reading, writing, and temporary file management

#### Performance Tests
Ensure acceptable performance characteristics:
- **Query Execution**: Large resource processing
- **Validation Speed**: Bulk validation operations
- **Memory Usage**: Resource consumption limits
- **Concurrent Operations**: Parallel processing capabilities

### Test Configuration

The test suite uses `expecto.config` for configuration:

```bash
# View current test configuration
cat tests/expecto.config

# Run tests with custom configuration
dotnet test --settings tests/expecto.config
```

### Writing Tests

When contributing new features, follow these testing guidelines:

1. **Unit Tests**: Test each function with valid inputs, invalid inputs, and edge cases
2. **Integration Tests**: Test complete workflows and CLI interactions
3. **Performance Tests**: Include performance tests for computationally intensive operations
4. **Error Handling**: Test all error conditions and edge cases
5. **Documentation**: Include clear test descriptions and expected behaviors

#### Example Test Structure

```fsharp
open Expecto
open FSharpFHIR

[<Tests>]
let queryEngineTests =
    testList "QueryEngine" [
        testCase "should parse simple property access" <| fun _ ->
            let query = ".name"
            let result = QueryEngine.parse query
            Expect.isOk result "Query should parse successfully"
            
        testCase "should handle invalid syntax" <| fun _ ->
            let query = ".invalid..syntax"
            let result = QueryEngine.parse query
            Expect.isError result "Invalid query should fail to parse"
    ]
```

### Continuous Integration

The test suite is designed to run in CI/CD environments:

```bash
# CI-friendly test execution
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet test --no-restore --verbosity normal --logger "trx;LogFileName=test-results.trx"

# Generate code coverage reports
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### Test Coverage

The test suite aims for high code coverage across all modules:
- **QueryEngine**: >90% coverage including edge cases
- **Validator**: >95% coverage of validation rules
- **Visualizer**: >85% coverage of formatting functions
- **JsonSerialization**: >90% coverage including error paths
- **FHIR Resources**: >80% coverage of type definitions
- **Integration**: >75% coverage of CLI workflows

To generate coverage reports:

```bash
# Install coverage tools
dotnet tool install --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool

# Generate coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

### Contributing

Contributions are welcome! This project demonstrates:

- Modern F# development practices
- Functional programming patterns
- Type-safe FHIR resource handling
- Interactive CLI development with Fli
- Query language implementation

Feel free to submit issues, feature requests, or pull requests to improve the project.

