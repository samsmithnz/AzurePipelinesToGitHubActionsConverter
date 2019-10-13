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
                //Name
                if (azurePipeline.name != null)
                {
                    gitHubActions.name = azurePipeline.name;
                }

                //Trigger
                if (azurePipeline.trigger != null)
                {
                    gitHubActions.on = ProcessTrigger(azurePipeline.trigger);
                }

                //Variables
                if (azurePipeline.variables != null)
                {
                    gitHubActions.env = azurePipeline.variables;
                }

                //Stages
                if (azurePipeline.stages != null)
                {
                    //TODO: Stages are not yet supported in GitHub actions (I think?)
                }

                //Jobs
                if (azurePipeline.jobs != null)
                {
                    gitHubActions.jobs = ProcessJobs(azurePipeline.jobs);
                }

                //Pool + Steps (When there are no jobs defined and a minimal set of steps and a pool)
                if (azurePipeline.pool != null ||
                        (azurePipeline.steps != null && azurePipeline.steps.Length > 0))
                {
                    //Steps only have one job, so we just create it here
                    gitHubActions.jobs = new GitHubActions.Job[]
                    {
                        new GitHubActions.Job
                        {
                            build = new Build
                            {
                                runsOn = azurePipeline.pool.vmImage,
                                steps = ProcessSteps(azurePipeline.steps)
                            }
                        }
                    };
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

        private string[] ProcessTrigger(string[] trigger)
        {
            //return trigger;
            //TODO: Add more processing on different triggers
            return new string[] { "push" };
        }  
        
        private string ProcessPool(Pool pool)
        {
            string newPool = null;
            if (pool != null)
            {
                newPool = pool.vmImage;
            }
            return newPool;
        }

        private GitHubActions.Job[] ProcessJobs(AzurePipelines.Job[] jobs)
        {
            GitHubActions.Job[] newJobs = null;
            if (jobs != null)
            {
                newJobs = new GitHubActions.Job[jobs.Length];
                for (int i = 0; i < jobs.Length; i++)
                {
                    newJobs[i] = new GitHubActions.Job
                    {
                        build = new Build
                        {
                            runsOn = ProcessPool(jobs[i].pool),
                            steps = ProcessSteps(jobs[i].steps)
                        }
                    };
                }
            }
            return newJobs;
        }

        private GitHubActions.Step[] ProcessSteps(AzurePipelines.Step[] steps)
        {
            GitHubActions.Step[] newSteps = null;
            if (steps != null)
            {
                newSteps = new GitHubActions.Step[steps.Length];
                for (int i = 0; i < steps.Length; i++)
                {
                    newSteps[i] = new GitHubActions.Step
                    {
                        name = steps[i].displayName,
                        run = steps[i].script
                    };
                }
            }

            return newSteps;
        }

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
                jobs = new GitHubActions.Job[]
                {
                    new GitHubActions.Job
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
                variables = new Dictionary<string, string>
                {
                    { "buildConfiguration","Release" }
                },
                //variables = new Variables
                //{
                //    buildConfiguration = "Release"
                //},
                steps = new AzurePipelines.Step[]
                {
                    new AzurePipelines.Step {
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

