using System;
using System.IO;
using System.Text;
using System.Xml;
using YamlDotNet.RepresentationModel;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using Newtonsoft.Json;
using YamlDotNet.Core;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Core.Events;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class Conversion
    {

        public string ConvertAzurePipelineToGitHubAction(string input)
        {
            AzurePipelinesRoot azurePipeline = ReadYamlFile<AzurePipelinesRoot>(input);
            int stepsLength = 0;
            if (azurePipeline != null && azurePipeline.steps != null)
            {
                stepsLength = azurePipeline.steps.Length;
            }

            GitHubActionsRoot gitHubActions = new GitHubActionsRoot();

            if (azurePipeline != null)
            {
                //gitHubAction.name = Global.GetHeaderComment();

                if (azurePipeline.trigger != null)
                {
                    //TODO: update this to handle multiple triggers
                    gitHubActions.on = new string[] { "push" };
                }
                if (azurePipeline.pool != null ||
                        (azurePipeline.steps != null && azurePipeline.steps.Length > 0))
                {
                    gitHubActions.jobs = new Job[]
                    {
                        new Job
                        {
                            build = new Build
                            {
                                runsOn = azurePipeline.pool.vmImage,
                                steps = new Step[stepsLength] //Initialize the array, and then populate it below
                            }
                        }
                    };

                    //TODO: Refactor this to be part of the steps creation, instead of a clean up step
                    if (gitHubActions.jobs[0].build.steps.Length == 0)
                    {
                        gitHubActions.jobs[0].build.steps = null;
                    }
                    else
                    {
                        for (int i = 0; i < stepsLength; i++)
                        {
                            gitHubActions.jobs[0].build.steps[i] = new Step
                            {
                                name = azurePipeline.steps[i].displayName,
                                run = azurePipeline.steps[i].script
                            };
                        }
                    }
                }
                string yaml = WriteYAMLFile<GitHubActionsRoot>(gitHubActions);
                return yaml;
            }
            else
            {
                return "";
            }
        }

        //name: CI

        //on: [push]

        //jobs:
        //  build:

        //    runs-on: ubuntu-latest

        //    steps:
        //    # checkout the repo
        //    - uses: actions/checkout @v1

        //    # install dependencies, build, and test
        //    - name: Build with dotnet
        //      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
        //    - name: Publish with dotnet
        //      run: dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
        //    - name: publish build artifacts back to GitHub
        //      uses: actions/upload-artifact @master
        //      with:
        //        name: serviceapp
        //        path: WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish

        public string CreateGitHubAction()
        {
            //name: CI

            //on: [push]

            //jobs:
            //  build:

            //    runs-on: ubuntu-latest

            //    steps:
            //    # checkout the repo
            //    - uses: actions/checkout @v1

            //    # install dependencies, build, and test
            //    - name: Build with dotnet
            //      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
            //    - name: Publish with dotnet
            //      run: dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
            //    - name: publish build artifacts back to GitHub
            //      uses: actions/upload-artifact @master
            //      with:
            //        name: serviceapp
            //        path: WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish

            //Build the GitHub actions object
            GitHubActionsRoot githubActionsYAML = new GitHubActionsRoot
            {
                name = "CI",
                on = new string[] { "push" },
                jobs = new Job[]
                {
                    new Job
                    {
                        build = new Build
                        {
                            runsOn = "ubuntu-latest",
                            steps = new GitHubActions.Step[]
                            {
                                new GitHubActions.Step
                                {
                                    uses = "actions/checkout @v1"
                                },
                                new GitHubActions.Step
                                {
                                    name = "Build with dotnet",
                                    run = "dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release"
                                },
                                new GitHubActions.Step
                                {
                                    name = "Publish with dotnet",
                                    run = "dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release"
                                },
                                new GitHubActions.Step
                                {
                                    name = "publish build artifacts back to GitHub",
                                    uses = "actions/upload-artifact @master",
                                    with = new With
                                    {
                                        name = "serviceapp",
                                        path = "WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            string yaml = WriteYAMLFile<GitHubActionsRoot>(githubActionsYAML);

            return yaml;
        }
        public string CreateAzurePipeline()
        {
            //trigger:
            //- master

            //pool:
            //  vmImage: ubuntu-latest

            //variables:
            //  buildConfiguration: Release

            //steps:
            //- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
            //  displayName: dotnet build $(buildConfiguration)

            //Build the Azure Pipelines object
            AzurePipelinesRoot azurePipelinesYAML = new AzurePipelinesRoot
            {
                trigger = new string[]
                {
                    "master"
                },
                pool = new Pool
                {
                    vmImage = "ubuntu-latest"
                },
                variables = new Variables
                {
                    buildConfiguration = "Release"
                },
                steps = new Script[]
                {
                    new Script {
                        script = "dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj",
                        displayName = "dotnet build $(buildConfiguration)"
                    }
                }
            };

            string yaml = WriteYAMLFile<AzurePipelinesRoot>(azurePipelinesYAML);

            return yaml;
        }

        private IDictionary<YamlNode, YamlNode> ReadYAMLFile(string input)
        {
            // Setup the input
            StringReader inputSR = new StringReader(input);

            // Load the stream
            YamlStream yaml = new YamlStream();
            yaml.Load(inputSR);

            // Examine the stream
            IDictionary<YamlNode, YamlNode> results = null;
            if (yaml.Documents.Count > 0)
            {
                YamlMappingNode mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
                results = mapping.Children;
            }

            // Return all nodes from the YAML document
            return results;
        }

        private T ReadYamlFile<T>(string yaml)
        {
            IDeserializer deserializer = new DeserializerBuilder()
                        .Build();
            T yamlObject = deserializer.Deserialize<T>(yaml);

            return yamlObject;
        }

        private string WriteYAMLFile<T>(T obj)
        {
            //Convert the object into a YAML document
            ISerializer serializer = new SerializerBuilder()
                    .Build();
            string yaml = serializer.Serialize(obj);
            //TODO: Resolve Sequence.Flow issue for on-push array to be on a single line (https://github.com/aaubry/YamlDotNet/issues/9)
            yaml = yaml.Replace("on:" + Environment.NewLine + "- push", "on: [push]");

            return yaml;
        }

    }

}



//# ASP.NET Core
//# Build and test ASP.NET Core projects targeting .NET Core.
//# Add steps that run tests, create a NuGet package, deploy, and more:
//# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core

//trigger:
//- master

//pool:
//  vmImage: ubuntu-latest

//variables:
//  buildConfiguration: Release

//steps:
//- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
//  displayName: 'dotnet build $(buildConfiguration)'


//name: CI

//on: [push]

//jobs:
//  build:

//    runs-on: ubuntu-latest

//    steps:
//    # checkout the repo
//    - uses: actions/checkout @v1

//    # install dependencies, build, and test
//    - name: Build with dotnet
//      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
//    - name: Publish with dotnet
//      run: dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
//    - name: publish build artifacts back to GitHub
//      uses: actions/upload-artifact @master
//      with:
//        name: serviceapp
//        path: WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish

