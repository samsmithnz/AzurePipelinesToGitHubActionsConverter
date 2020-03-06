# Convert Azure Pipelines YAML to GitHub Actions YAML 
A project to create a conversion tool to make migrations between Azure Pipelines YAML and GitHub Actions YAML possible. As GitHub Actions becomes more popular, it's clear that a migration tool will be useful to move workloads to GitHub. 

![Current build](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/workflows/CI/badge.svg)
[![NuGet version (BlackBeltCoder.Silk)](https://img.shields.io/nuget/v/BlackBeltCoder.Silk.svg?style=flat-square)](https://www.nuget.org/packages/AzurePipelinesToGitHubActionsConverter.Core/)

As GitHub Actions becomes more popular, it's clear that a migration tool will be useful to move workloads to GitHub. 

# How to use
There is a website that consumes this module at: https://pipelinestoactions.azurewebsites.net/.  
You can also use the (currently prerelease) [NuGet package](https://www.nuget.org/packages/AzurePipelinesToGitHubActionsConverter.Core/)

# How this works
**Currently this only supports one-way migrations from Azure Pipelines to GitHub Actions. There are functions to deserialize Azure Pipelines, and serialize and deserialize GitHub Actions. While this is translating many steps, it has so far been targeted to just supporting the basic .NET pipelines so far. Check the [issues](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues) for incomplete items, or the TODO's in the source code.**
 
Yaml can be challenging. The [yaml wikipedia](https://en.wikipedia.org/wiki/YAML) page does a very good job of laying out the rules, but when we are talking about converting yaml to C#, there are a few things to know:

1. Use a good editor - Visual Studio Code has a decent YAML extension (https://marketplace.visualstudio.com/items?itemName=docsmsft.docs-yaml), or if using Visual Studio, enable spaces with CTRL+R,CTRL+W. The GitHub and Azure DevOps in-browser editors are decent too. 
2. String arrays (string[]) are useful for lists (e.g -job). Note both of the following pieces of code for triggers are effectively the same (although the YamlDotNet serializer does not currently support the single line 'sequence flow' format)
```YAML
trigger: [master,develop]

trigger:
- master
- develop
```
3. The dictonary object (dictonary<string,string>) is useful for dynamic key value pairs, for example, variables
```YAML
variables:
  MY_VAR: 'my value'
  ANOTHER_VAR: 'another value'
```
The C# definition for a dictonary looks like:
```C#
Dictionary<string, string> variables = new Dictionary<string, string>
{
    { "MY_VAR", "my value" },
    { "ANOTHER_VAR", "another value" }
};
```
4. Just about everything else can be a string or object. Here is an example of a simple job:
```YAML
- job: Build
  displayName: 'Build job'
  pool:
    vmImage: ubuntu-latest
```
```C#
public string job { get; set; }
public string displayName { get; set; }
public Pool pool { get; set; }
```
5. Yaml is wack. The white spaces can destroy you, as the errors returned are often not helpful at all. Take lots of breaks.

## Current limitations
There are a number of Azure Pipeline features that don't currently match up well with a GitHub feature, and hence, these migrate with a change in functionality (e.g. parameters become variables and stages become jobs), or not at all (e.g. )
- Stages: become jobs. For example, a job "JobA" in a stage "Stage1", becomes a job named "Stage1_JobA"
```Azure Pipelines YAML

```
- Parameters: become variables
```Azure Pipelines YAML

```
- Deployment jobs: The strategy is removed and it becomes a regular job
```Azure Pipelines YAML

```
- oneonce deployment strategy
```Azure Pipelines YAML

```

## Architecture
The core functionality is contained in a .NET Standard 2.1 class, "AzurePipelinesToGitHubActionsConverter.Core".
- In the [conversion object](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/blob/master/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter.Core/Conversion.cs), is a public call, "ConvertAzurePipelineToGitHubAction", to convert Azure DevOps yaml to GitHub Actions yaml: 
- In the same conversion object is a call to convert individual tasks/steps: "ConvertAzurePinelineTaskToGitHubActionTask"
- The [GitHubActionsSerialization object](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/blob/master/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter.Core/GitHubActionsSerialization.cs) has calls to serialize and deserialize GitHub actions 

Testing:
- There is a .NET CORE 3.1 MSTest project for tests, "AzurePipelinesToGitHubActionsConverter.Tests" 
Current projects consuming it:
- There is a website in [another GitHub project](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverterWeb) where you can test this interactively, at: https://pipelinestoactions.azurewebsites.net/ 

## Example: 
The Azure Pipelines YAML to build a dotnet application on ubuntu:
```YAML
trigger:
- master
variables:
  buildConfiguration: Release
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: ubuntu-latest
  variables:
    myJobVariable: data
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build $(myJobVariable)
```
In GitHub Actions:
```YAML
on: 
  push:
    branches:
    - master
env:
  buildConfiguration: Release
jobs:
  Build:
    name: Build job
    runs-on: ubuntu-latest
    env:
      myJobVariable: data
    steps:
    - uses: actions/checkout@v1
    - name: dotnet build ${{ env.myJobVariable }}
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}";
```
## References
Made with help from https://github.com/aaubry/YamlDotNet and https://en.wikipedia.org/wiki/YAML.
- Azure Pipelines YAML docs: https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema
- GitHub Actions YAML docs: https://help.github.com/en/articles/workflow-syntax-for-github-actions
- Software installed on GitHub Action runners: https://help.github.com/en/actions/automating-your-workflow-with-github-actions/software-installed-on-github-hosted-runners

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
