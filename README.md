# Convert Azure Pipelines YAML to GitHub Actions YAML 
A project to create a conversion tool to make migrations between Azure Pipelines YAML and GitHub Actions YAML possible. As GitHub Actions becomes more popular, it's clear that a migration tool will be useful to move workloads to GitHub. 

![Current build](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/workflows/CI/badge.svg)
[![Coverage Status](https://coveralls.io/repos/github/samsmithnz/AzurePipelinesToGitHubActionsConverter/badge.svg?branch=main)](https://coveralls.io/github/samsmithnz/AzurePipelinesToGitHubActionsConverter?branch=main)
[![Latest NuGet package](https://img.shields.io/nuget/v/AzurePipelinesToGitHubActionsConverter.Core)](https://www.nuget.org/packages/AzurePipelinesToGitHubActionsConverter.Core/)
![Current Release](https://img.shields.io/github/release/samsmithnz/AzurePipelinesToGitHubActionsConverter/all.svg)

# How to use
There is a website that consumes this module at: https://pipelinestoactions.azurewebsites.net/.  
You can also use the [NuGet package](https://www.nuget.org/packages/AzurePipelinesToGitHubActionsConverter.Core/)

## System Variables
This is our current table of how we are translating [Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml) system variables, to [GitHub Environment variables](https://docs.github.com/en/free-pro-team@latest/actions/reference/environment-variables#default-environment-variables).
| Azure Pipelines | GitHub Actions |
| -- | -- |
| Build.ArtifactStagingDirectory | github.workspace | 
| Build.BuildId | github.run_id |
| Build.BuildNumber | github.run_number |
| Build.SourceBranch | github.ref |
| Build.Repository.Name | github.repository |
| Build.SourcesDirectory | github.workspace |
| Build.StagingDirectory | github.workspace |
| System.DefaultWorkingDirectory | github.workspace |
| Agent.OS | runner.os |
<!-- | Build.SourceBranchName |  |-->
<!-- | Build.Reason |  |-->

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

## Current builds running
The table below shows current, functioning YML files that run on a regular schedule to regression test the YAML produced
| Language | Azure Pipelines | GitHub Actions |
| -- | -- | -- |
| .NET Core WebAPI | [![.NET Core WebAPI](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_apis/build/status/DotNetCoreWebAPI?branchName=master)](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_build/latest?definitionId=77&branchName=master) | [![.NET Core WebAPI](https://github.com/samsmithnz/GitHubActionsDemos/workflows/.NET%20Core%20WebAPI/badge.svg)](https://github.com/samsmithnz/GitHubActionsDemos/actions?query=workflow%3A%22.NET+Core+WebAPI%22) |
| .NET Framework Desktop | [![.NET Framework Desktop](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_apis/build/status/DotNetFrameworkDesktop?branchName=master)](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_build/latest?definitionId=81&branchName=master) | [![.NET Framework Desktop](https://github.com/samsmithnz/GitHubActionsDemos/workflows/.NET%20Framework%20Desktop/badge.svg)](https://github.com/samsmithnz/GitHubActionsDemos/actions?query=workflow%3A%22.NET+Framework+Desktop%22) |
| Ant | [![Ant](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_apis/build/status/Ant?branchName=master)](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_build/latest?definitionId=76&branchName=master) | [![Ant](https://github.com/samsmithnz/GitHubActionsDemos/workflows/Ant/badge.svg)](https://github.com/samsmithnz/GitHubActionsDemos/actions?query=workflow%3AAnt) |
| Maven | [![Maven](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_apis/build/status/Maven?branchName=master)](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_build/latest?definitionId=79&branchName=master) | [![Maven](https://github.com/samsmithnz/GitHubActionsDemos/workflows/Maven/badge.svg)](https://github.com/samsmithnz/GitHubActionsDemos/actions?query=workflow%3AMaven) |
| NodeJS | [![NodeJS](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_apis/build/status/NodeJS?branchName=master)](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_build/latest?definitionId=80&branchName=master) | [![Node.js](https://github.com/samsmithnz/GitHubActionsDemos/workflows/Node.js/badge.svg)](https://github.com/samsmithnz/GitHubActionsDemos/actions?query=workflow%3ANode.js) |
| Python | [![Python](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_apis/build/status/Python?branchName=master)](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_build/latest?definitionId=78&branchName=master) | [![Python](https://github.com/samsmithnz/GitHubActionsDemos/workflows/Python/badge.svg)](https://github.com/samsmithnz/GitHubActionsDemos/actions?query=workflow%3APython) |
| Ruby | [![Ruby](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_apis/build/status/Ruby?branchName=master)](https://dev.azure.com/samsmithnz/AzurePipelinesDemos/_build/latest?definitionId=82&branchName=master) | [![Ruby](https://github.com/samsmithnz/GitHubActionsDemos/workflows/Ruby/badge.svg)](https://github.com/samsmithnz/GitHubActionsDemos/actions?query=workflow%3ARuby) |


# How this works
**Currently this only supports one-way migrations from Azure Pipelines to GitHub Actions. There are functions to deserialize Azure Pipelines, and serialize and deserialize GitHub Actions. While this is translating many steps, there are nearly infinite combinations, therefore most of the focus has been supporting the basic .NET pipelines. Even if steps can't convert, the pipeline *should*. Check the [issues](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues) for incomplete items, or the TODO's in the source code.**
 
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
5. Yaml is wack. The white spaces can destroy you, as the errors returned are often not helpful at all. Take lots of breaks. In the end YAML is worth it - I promise!



## Current limitations
There are a number of Azure Pipeline features that don't currently match up well with a GitHub feature, and hence, these migrate with a change in functionality (e.g. parameters become variables and stages become jobs), or not at all (e.g. deployment strategies/environments). As/if these features are added to Actions, we will build in the conversions

#### **Stages**
Stages are converted to jobs. For example, a job "JobA" in a stage "Stage1", becomes a job named "Stage1_JobA"
###### Azure Pipelines YAML
```YAML
stages:
- stage: Build
  displayName: 'Build Stage'
  jobs:
  - job: Build
    displayName: 'Build job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: |
         Write-Host "Hello world!"

  - job: Build2
    displayName: 'Build job 2'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: |
         Write-Host "Hello world 2!"
```
###### GitHub Actions YAML
```YAML
jobs:
  Build_Stage_Build:
    name: Build job
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v1
    - run: 
        Write-Host "Hello world!"
      shell: powershell
  Build_Stage_Build2:
    name: Build job 2
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v1
    - run: 
        Write-Host "Hello world 2!"
      shell: powershell
```

#### **Parameters**
Parameters become variables
###### Azure Pipelines YAML
```YAML
parameters: 
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'

jobs:
  - job: Build
    displayName: 'Build job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      displayName: 'Test'
      inputs:
        targetType: inline
        script: |
          Write-Host "Hello world ${{parameters.buildConfiguration}} ${{parameters.buildPlatform}}"
```
###### GitHub Actions YAML
```YAML
env:
  buildConfiguration: Release
  buildPlatform: Any CPU
jobs:
  Build:
    name: Build job
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v1
    - name: Test
      run: Write-Host "Hello world ${{ env.buildConfiguration }} ${{ env.buildPlatform }}"
      shell: powershell
```

#### **RunOnce deployment strategy and deployment jobs**
The strategy and deployment job is consolidated to a job, with the strategy removed, as there is no GitHub equivalent

###### Azure Pipelines YAML
```YAML
jobs:
  - deployment: DeployInfrastructure
    displayName: Deploy job
    environment: Dev
    pool:
      vmImage: windows-latest
    strategy:
      runOnce:
        deploy:
          steps:
          - task: PowerShell@2
            displayName: 'Test'
            inputs:
              targetType: inline
              script: |
                Write-Host ""Hello world""";
```
###### GitHub Actions YAML
```YAML
jobs:
  DeployInfrastructure:
    name: Deploy job
    runs-on: windows-latest
    steps:
    - name: Test
      run: Write-Host ""Hello world""
      shell: powershell
```

#### Templates
There are no templates in GitHub actions. At this time we are converting the template into an (almost) empty job.
```YAML
jobs:
- template: azure-pipelines-build-template.yml
  parameters:
    buildConfiguration: 'Release'
    buildPlatform: 'Any CPU'
    vmImage: windows-latest
```
```YAML
jobs:
  job_1_template:
    #: 'Note: Azure DevOps template does not have an equivalent in GitHub Actions yet'
    steps:
    - uses: actions/checkout@v1
```

#### **Conditions**
Conditions are processing with about 98% accuracy. There are some system variables that still need conversions, but we've tried to handle the most popular combinations. 

## Architecture
The core functionality is contained in a .NET Standard 2.0 class, "AzurePipelinesToGitHubActionsConverter.Core".
- In the [conversion object](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/blob/master/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter.Core/Conversion.cs), is a public call, "ConvertAzurePipelineToGitHubAction", to convert Azure DevOps yaml to GitHub Actions yaml: 
- In the same conversion object is a call to convert individual tasks/steps: "ConvertAzurePinelineTaskToGitHubActionTask"
- The [GitHubActionsSerialization object](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/blob/master/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter.Core/GitHubActionsSerialization.cs) has calls to serialize and deserialize GitHub actions 

Testing:
- There is a .NET CORE 3.1 MSTest project for tests, "AzurePipelinesToGitHubActionsConverter.Tests" 

Current projects consuming this:
- There is a website in [another GitHub project](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverterWeb) where you can test this interactively, at: https://pipelinestoactions.azurewebsites.net/ 
- Alex, (who has made many contributions here), has created a classic Azure Pipelines to Azure Pipelines/GitHub Actions YAML converter: [yamlizer](https://github.com/f2calv/yamlizr)


## References
Made with help from https://github.com/aaubry/YamlDotNet and https://en.wikipedia.org/wiki/YAML.
- Azure Pipelines YAML docs: https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema
- GitHub Actions YAML docs: https://help.github.com/en/articles/workflow-syntax-for-github-actions
- Software installed on GitHub Action runners: https://docs.github.com/en/free-pro-team@latest/actions/reference/specifications-for-github-hosted-runners#supported-software

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change. Please use a consistent naming convention for your feature branches to identify the work done in the branch. Some suggestions for naming:
- features/feature-name
- bugfix/description
- hotfix/description
