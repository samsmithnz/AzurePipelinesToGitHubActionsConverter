using AzurePipelinesToGitHubActionsConverter.Core;
using System;

namespace AzurePipelinesToGitHubActionsConverter.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = @"
trigger:
- master
variables:
  buildConfiguration: Release
  vmImage: windows-latest
jobs:
- job: Build
  displayName: Build job part A
  pool: 
    vmImage: $(vmImage) 
  steps: 
  - script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
    displayName: dotnet build $(buildConfiguration) part A1
- job: Build
  displayName: Build job part B 
  pool: 
    vmImage: $(vmImage) 
  steps: 
  - script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
    displayName: dotnet build $(buildConfiguration) part B1
  - script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
    displayName: dotnet build $(buildConfiguration) part B2";

            //Process the input
            Conversion conversion = new Conversion();
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Output the result
            Console.WriteLine("Azure Pipelines YAML: " + Environment.NewLine + input);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("GitHub Actions YAML: " + Environment.NewLine + output);
        }
    }
}
