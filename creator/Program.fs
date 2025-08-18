module FSharpFHIR.Creator

open System
open System.CommandLine

/// Simple CLI for generating minimal Patient resources
[<EntryPoint>]
let main argv =
    // Create the root command
    let rootCommand = RootCommand("Generate minimal FHIR resources as JSON.")
    
    // Create the create command
    let createCommand = Command("create", "Create a FHIR resource")
    
    // Create the patient subcommand
    let patientCommand = Command("patient", "Create a Patient resource")
    
    // Create options for patient
    let nameOption = Option<string>("--name", "Name of the patient")
    let genderOption = Option<string>("--gender", "Gender of the patient")
    let birthDateOption = Option<string>("--birthDate", "Birth date of the patient (YYYY-MM-DD)")
    let outOption = Option<string>("--out", "Output file path (prints to stdout if not specified)")
    
    // Add options to the patient command
    patientCommand.AddOption(nameOption)
    patientCommand.AddOption(genderOption)
    patientCommand.AddOption(birthDateOption)
    patientCommand.AddOption(outOption)
    
    // Set the handler for the patient command
    patientCommand.SetHandler(Action<string, string, string, string>(fun name gender birthDate outFile ->
        let parts =
            [ Some "\"resourceType\":\"Patient\"";
              if not (String.IsNullOrEmpty name) then Some $"\"name\":[{{\"text\":\"{name}\"}}]" else None;
              if not (String.IsNullOrEmpty gender) then Some $"\"gender\":\"{gender}\"" else None;
              if not (String.IsNullOrEmpty birthDate) then Some $"\"birthDate\":\"{birthDate}\"" else None ]
            |> List.choose id
        let json = "{" + String.Join(",", parts) + "}"

        if not (String.IsNullOrEmpty outFile) then
            System.IO.File.WriteAllText(outFile, json)
            printfn "Wrote Patient resource to %s" outFile
            Environment.Exit(0)
        else
            printfn "%s" json
            Environment.Exit(0)
    ), nameOption, genderOption, birthDateOption, outOption)
    
    // Add the patient command to the create command
    createCommand.AddCommand(patientCommand)
    
    // Add the create command to the root command
    rootCommand.AddCommand(createCommand)
    
    // Invoke the command
    rootCommand.Invoke(argv)
