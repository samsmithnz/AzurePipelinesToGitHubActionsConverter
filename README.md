# Convert Azure Pipelines YAML to GitHub Actions YAML 
A project to create a conversion tool to make migrations between Azure Pipelines YAML and GitHub Actions YAML possible.

![](https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/workflows/CI/badge.svg)

As GitHub Actions becomes more popular, it's clear that a migration tool will be useful to move workloads to GitHub. 

Currently this only supports one-way migrations from Azure Pipelines to GitHub Actions. 

Made with help from https://github.com/aaubry/YamlDotNet and https://en.wikipedia.org/wiki/YAML.

Yaml can be challenging. The wikipedia page lays out the rules nicely, but when we are talking about converting yaml to C#, there are a few things to know

1. Yaml is wack. The white spaces will destroy you, as the errors returned are often not helpful at all. Take lots of breaks.
2. Use a good editor - Visual Studio Code has a decent YAML extension
3. String arrays (string[]) are useful for lists (e.g) -job. 
4. The dictonary object (dictonary<string,string>) is useful for dynamic key value pairs, for example, variables
5. Just about everything else can be a string or object
