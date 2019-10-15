# Convert Azure Pipelines YAML to GitHub Actions YAML 
A project to create a conversion tool to make migrations between Azure Pipelines YAML and GitHub Actions YAML possible. As GitHub Actions becomes more popular, it's clear that a migration tool will be useful to move workloads to GitHub. 

Current build: ![](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/workflows/CI/badge.svg)

**Currently this only supports one-way migrations from Azure Pipelines to GitHub Actions. Also note that this is not translating steps at this time, just translating the infrastructure around it. Check the [issues](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues) for incomplete items, or the TODO's in the source code.**
 
Yaml can be challenging. The wikipedia page lays out the rules nicely, but when we are talking about converting yaml to C#, there are a few things to know

1. Yaml is wack. The white spaces will destroy you, as the errors returned are often not helpful at all. Take lots of breaks.
2. Use a good editor - Visual Studio Code has a decent YAML extension (https://marketplace.visualstudio.com/items?itemName=docsmsft.docs-yaml), or if using Visual Studio, enable spaces with CTRL+R,CTRL+W
3. String arrays (string[]) are useful for lists (e.g) -job. Note both of the following pieces of code for a triggers are effectively the same 
```YAML
trigger: [master,develop]

trigger:
- master
- develop
```
4. The dictonary object (dictonary<string,string>) is useful for dynamic key value pairs, for example, variables
```YAML
variables:
  MY_VAR: 'my value'
  ANOTHER_VAR: 'another value'
```
The C# definition for this looks like:
```C#
public Dictionary<string, string> variables { get; set; }
```
5. Just about everything else can be a string or object. Here is an example of a simple job:
```YAML
- job: Build
  displayName: 'Build job'
  pool:
    vmImage: $(vmImage)
```
```C#
public string job { get; set; }
public string displayName { get; set; }
public Pool pool { get; set; }
```

## Architecture
Currently a .NET Standard 2.0 class that is used by a MSTEST project for tests, and a .NET Core 3.0 console app

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
on: [push]
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
    - name: dotnet build $myJobVariable
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $buildConfiguration
```
## References
Made with help from https://github.com/aaubry/YamlDotNet and https://en.wikipedia.org/wiki/YAML.
- Azure Pipelines YAML docs: https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema
- GitHub Actions YAML docs: https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idstepsuses

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
