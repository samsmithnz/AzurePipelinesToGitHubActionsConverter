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
        private List<string> _variableList;

        public string ConvertAzurePipelineToGitHubAction(string input)
        {
            AzurePipelinesRoot azurePipeline = ReadYamlFile<AzurePipelinesRoot>(input);
            GitHubActionsRoot gitHubActions = new GitHubActionsRoot();
            _variableList = new List<string>();


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
                    //TODO: While it converts variables, we still don't know how to implement them. ${{ myVar }} and $myVar both don't work...
                    gitHubActions.env = ProcessVariables(azurePipeline.variables);
                }

                //Stages
                if (azurePipeline.stages != null)
                {
                    //TODO: Stages are not yet supported in GitHub actions (I think?)
                }

                //Jobs (when no stages are defined)
                if (azurePipeline.jobs != null)
                {
                    gitHubActions.jobs = ProcessJobs(azurePipeline.jobs);
                }

                //Pool + Steps (When there are no jobs defined)
                if (azurePipeline.pool != null ||
                        (azurePipeline.steps != null && azurePipeline.steps.Length > 0))
                {
                    //Steps only have one job, so we just create it here
                    gitHubActions.jobs = new Dictionary<string, GitHubActions.Job>
                    {
                        {
                            "build",
                            new GitHubActions.Job
                            {
                                runs_on = ProcessPool(azurePipeline.pool),
                                steps = ProcessSteps(azurePipeline.steps)
                            }
                        }
                    };
                }

                string yaml = WriteYAMLFile<GitHubActionsRoot>(gitHubActions);

                //Fix some variables for serialization, the '-' character is not valid in property names, and some of the YAML standard uses reserved words (e.g. if)
                yaml = PrepareYamlPropertiesForGitHubSerialization(yaml);

                //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
                yaml = PrepareYamlVariablesForGitHubSerialization(yaml);

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

        private Dictionary<string, string> ProcessVariables(Dictionary<string, string> variables)
        {
            //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
            foreach (string item in variables.Keys)
            {
                _variableList.Add(item);
            }

            return variables;
        }

        private Dictionary<string, GitHubActions.Job> ProcessJobs(AzurePipelines.Job[] jobs)
        {
            //A dictonary is perfect here, as the job_id (a string), must be unique in the action
            Dictionary<string, GitHubActions.Job> newJobs = null;
            if (jobs != null)
            {
                newJobs = new Dictionary<string, GitHubActions.Job>();
                for (int i = 0; i < jobs.Length; i++)
                {
                    newJobs.Add(jobs[i].job, ProcessIndividualJob(jobs[i]));
                }
            }
            return newJobs;
        }

        private GitHubActions.Job ProcessIndividualJob(AzurePipelines.Job job)
        {
            return new GitHubActions.Job
            {
                name = job.displayName,
                needs = job.dependsOn,
                _if = job.condition,
                env = ProcessVariables(job.variables),
                runs_on = ProcessPool(job.pool),
                timeout_minutes = job.timeoutInMinutes,
                steps = ProcessSteps(job.steps)
            };
        }

        private GitHubActions.Step[] ProcessSteps(AzurePipelines.Step[] steps, bool addCheckoutStep = true)
        {
            GitHubActions.Step[] newSteps = null;
            if (steps != null)
            {
                int adjustment = 0;
                if (addCheckoutStep == true)
                {
                    adjustment = 1; // we are inserting a step and need to start moving steps 1 place into the array
                }
                newSteps = new GitHubActions.Step[steps.Length + adjustment]; //Add 1 for the check out step

                if (addCheckoutStep == true)
                {
                    newSteps[0] = new GitHubActions.Step
                    {
                        uses = "actions/checkout@v1"
                    };
                }

                //Translate the other steps
                for (int i = adjustment; i < steps.Length + adjustment; i++)
                {
                    newSteps[i] = new GitHubActions.Step
                    {
                        name = steps[i - adjustment].displayName,
                        run = steps[i - adjustment].script
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
                jobs = new Dictionary<string, GitHubActions.Job>
                {
                    {
                        "build",
                        new GitHubActions.Job
                        {
                            runs_on = "ubuntu-latest",
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
                    { "buildConfiguration", "Release" }
                },
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

        //private IDictionary<YamlNode, YamlNode> ReadYAMLFile(string input)
        //{
        //    // Setup the input
        //    StringReader inputSR = new StringReader(input);

        //    // Load the stream
        //    YamlStream yaml = new YamlStream();
        //    yaml.Load(inputSR);

        //    // Examine the stream
        //    IDictionary<YamlNode, YamlNode> results = null;
        //    if (yaml.Documents.Count > 0)
        //    {
        //        YamlMappingNode mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
        //        results = mapping.Children;
        //    }

        //    // Return all nodes from the YAML document
        //    return results;
        //}


        public GitHubActionsRoot ReadGitHubActionsYaml(string yaml)
        {
            //Fix some variables that we can't use for property names because the - character is not allowed or it's a reserved word (e.g. if)
            yaml = yaml.Replace("runs-on", "runs_on");
            yaml = yaml.Replace("if", "_if");
            yaml = yaml.Replace("timeout-minutes", "timeout_minutes");

            return ReadYamlFile<GitHubActionsRoot>(yaml);
        }

        private string PrepareYamlPropertiesForGitHubSerialization(string yaml)
        {
            //Fix some variables that we can't use for property names because the - character is not allowed or it's a reserved word (e.g. if)
            yaml = yaml.Replace("runs_on", "runs-on");
            yaml = yaml.Replace("_if", "if");
            yaml = yaml.Replace("timeout_minutes", "timeout-minutes");
            return yaml;
        }

        private string PrepareYamlVariablesForGitHubSerialization(string yaml)
        {
            foreach (string item in _variableList)
            {
                //Replace variables with the format "$(MyVar)" with the format "$MyVar"
                yaml = yaml.Replace("$(" + item + ")", "$" + item);
            }

            return yaml;
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

