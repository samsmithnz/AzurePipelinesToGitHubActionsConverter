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
  vmImage: ubuntu-latest
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: ubuntu-latest
  variables:
    buildConfiguration: Debug
    myJobVariable: 'data'
    myJobVariable2: 'data2'
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build part 1
- job: Build2
  displayName: Build job
  dependsOn: Build
  pool: 
    vmImage: ubuntu-latest
  variables:
    myJobVariable: 'data'
  steps:
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build part 2
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build part 3";

            //Process the input
            Conversion conversion = new Conversion();
            GitHubConversion gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Output the result
            Console.WriteLine("Azure Pipelines YAML: " + Environment.NewLine + input);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("GitHub Actions YAML: " + Environment.NewLine + gitHubOutput.yaml);
        }
    }
}
