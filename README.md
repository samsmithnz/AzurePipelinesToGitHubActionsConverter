# Convert Azure Pipelines YAML to GitHub Actions YAML 
A project to create a conversion tool to make migrations between Azure Pipelines YAML and GitHub Actions YAML possible.

![](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/workflows/CI/badge.svg)

As GitHub Actions becomes more popular, it's clear that a migration tool will be useful to move workloads to GitHub. 

Currently this only supports one-way migrations from Azure Pipelines to GitHub Actions. Also note that this is not translating steps at this time, just translating the infrastructure around it.
 
Made with help from https://github.com/aaubry/YamlDotNet and https://en.wikipedia.org/wiki/YAML.
- Azure Pipelines YAML docs: https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema
- GitHub Actions YAML docs: https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idstepsuses

Yaml can be challenging. The wikipedia page lays out the rules nicely, but when we are talking about converting yaml to C#, there are a few things to know

1. Yaml is wack. The white spaces will destroy you, as the errors returned are often not helpful at all. Take lots of breaks.
2. Use a good editor - Visual Studio Code has a decent YAML extension
3. String arrays (string[]) are useful for lists (e.g) -job. 
4. The dictonary object (dictonary<string,string>) is useful for dynamic key value pairs, for example, variables
5. Just about everything else can be a string or object

## Architecture
Currently a .NET Standard 2.0 class that is used by a MSTEST project for tests, and a .NET Core 3.0 console app

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.