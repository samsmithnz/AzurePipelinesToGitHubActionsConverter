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

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class Conversion
    {

        public string ConvertPipelineToAction(string input, bool showGlobalHeaderComment = true)
        {
            StringBuilder output = new StringBuilder();
            var deserializer = new DeserializerBuilder()
                .Build();

            //Process the Azure Pipelines YAML file
            IDictionary<YamlNode, YamlNode> yamlNodes = ReadYAMLFile(input);

            //Add conversion text to the top of the page
            if (showGlobalHeaderComment == true)
            {
                output.Append(Global.GetHeaderComment());
            }

            if (yamlNodes != null)
            {
                foreach (KeyValuePair<YamlNode, YamlNode> entry in yamlNodes)
                {
                    Console.WriteLine(entry); //Returns the entire node
                    Console.WriteLine(((YamlScalarNode)entry.Key).Value); //Returns the parent node name

                    string nodeName = ((YamlScalarNode)entry.Key).Value;
                    int nodeStartPosition = ((YamlScalarNode)entry.Key).Start.Column - 1;
                    switch (nodeName)
                    {
                        case "trigger":
                            StringReader sr = new StringReader(entry.ToString());
                            var yamlObject = deserializer.Deserialize(sr);

                            var serializer = new SerializerBuilder()
                                .JsonCompatible()
                                .Build();

                            var json = serializer.Serialize(yamlObject);
                            //var json4 = serializer.Serialize(entry.ToString());

                            //var trigger = JsonConvert.DeserializeObject(json);
                            //var trigger2 = JsonConvert.DeserializeObject<TriggerRoot>(json);

                            //var trigger3 = deserializer.Deserialize<TriggerRoot>(json);
                            //output.Append(Global.GetSpaces(nodeStartPosition));
                            output.Append("on: [push]");
                            break;

                        case "pool":
                            StringReader sr2 = new StringReader(entry.ToString());
                            var yamlObject2 = deserializer.Deserialize(sr2);

                            var serializer2 = new SerializerBuilder()
                                .JsonCompatible()
                                .Build();

                            var json2 = serializer2.Serialize(yamlObject2);
                            var json3 = serializer2.Serialize(entry.ToString());

                            var pool3 = JsonConvert.DeserializeObject(json2);
                            //var pool = deserializer.Deserialize<PoolRoot>(entry.ToString());
                            //var pool = deserializer.Deserialize<PoolRoot>("[pool, { vmImage, ubuntu }]");
                            //deserializer.Deserialize<Dictionary<string, string>>(entry.ToString());
                            output.Append(Global.GetSpaces(nodeStartPosition));
                            output.Append("runs-on: ubuntu-latest");
                            break;

                        default:
                            if (nodeName.Trim().StartsWith("#") == true)
                            {
                                //It's a comment, just append it back to the output
                                output.Append(nodeName);
                            }
                            else
                            {
                                output.Append(Global.GetLineComment());
                                output.Append(nodeName);
                            }
                            break;
                    }
                }
            }


            //    for (int i = 0; i < lines.Length; i++)
            //    {
            //        string item = lines[i];
            //        if (item.Trim().StartsWith("#") == true)
            //        {
            //            //It's a comment, just append it back to the output
            //            output.Append(item);
            //        }
            //        else if (item.Trim().StartsWith("trigger:") == true)
            //        {
            //            //trigger:
            //            //- master
            //            string triggerText = item;
            //            int n = i;
            //            string newItem = "";
            //            while (lines.Length - 1 >= i + n)
            //            {
            //                n++;
            //                newItem = lines[0 + n];
            //                triggerText += Environment.NewLine + newItem;
            //            }
            //            i += n;
            //            output.Append("on: [push]");
            //        }
            //        else
            //        {
            //            //output.Append(Global.GetLineComment());
            //            output.Append(item);
            //        }
            //    }

            return output.ToString();
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
            //else
            //{
            //    yaml.ToString();
            //}

            // Return all nodes from the YAML document
            return results;
        }

        private string WriteAzurePipelinesYAMLFile(AzurePipelinesRoot azurePipelines)
        {
            //Convert the object into a YAML document
            ISerializer serializer = new SerializerBuilder()
                    .Build();
            string yaml = serializer.Serialize(azurePipelines);
            Console.WriteLine(yaml);

            return yaml;
        }

        private string WriteGitHubActionsYAMLFile(GitHubActionsRoot gitHubActions)
        {
            //Convert the object into a YAML document
            ISerializer serializer = new SerializerBuilder().Build();
            string yaml = serializer.Serialize(gitHubActions);
            Console.WriteLine(yaml);

            return yaml;
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
                on = "[push]",
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

            string yaml = WriteGitHubActionsYAMLFile(githubActionsYAML);

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

            //Dictionary<string, Script> script1 = new Dictionary<string, Script>();
            //Script scriptDetails = new Script
            //{
            //    displayname = "dotnet build $(buildConfiguration)"
            //};
            //script1.Add("dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj", scriptDetails);

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
                    buildConfiguration = "Release",
                    anotherVariable = 12,
                    yetAnotherVariable = "var"
                },
                steps = new Script[]
                {
                    new Script {
                        script = "dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj",
                        displayName = "dotnet build $(buildConfiguration)"
                    } 
                  //new Dictionary<string, string>() { {"script","dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj" } },
                  //new Dictionary<string, string>() { { "displayname", "dotnet build $(buildConfiguration)" } }              
                }
                //steps = new Dictionary<string, Script>[]
                //{
                //    new Dictionary<string, Script>() {{"",new Script { script= "dotnet build", } }}
                //  //  new Dictionary<string, string>() { {"script","dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj" } },
                //  //new Dictionary<string, string>() { { "displayname", "dotnet build $(buildConfiguration)" } }              
                //}

                //steps = new AzurePipelines.Step[1]()
                //{
                //    new AzurePipelines.Step
                //    {
                //        script = script1
                //        //script = new Dictionary<string, Script>{
                //        //    { "dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj", new Script {
                //        //         displayname="dotnet build $(buildConfiguration)"
                //        //        }
                //        //    }
                //        // }
                //    }
                //}
            };

            ////Set steps array size
            //azurePipelinesYAML.steps = new AzurePipelines.Step[1];
            //Dictionary<string, Script> script1 = new Dictionary<string, Script>{
            //    { "dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj", new Script {
            //         displayname="dotnet build $(buildConfiguration)"
            //        }
            //    }
            // };
            //azurePipelinesYAML.steps[0] = new AzurePipelines.Step
            //{
            //    script = script1
            //};

            string yaml = WriteAzurePipelinesYAMLFile(azurePipelinesYAML);

            return yaml;
        }
    }

    public static class KeyValuePair
    {
        public static KeyValuePair<K, V> Create<K, V>(K key, V value)
        {
            return new KeyValuePair<K, V>(key, value);
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





//public string ConvertPipelineToAction(string input, bool showGlobalHeaderComment = true)
//{
//    string[] lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

//    StringBuilder output = new StringBuilder();
//    //Add conversion text to the top of the page
//    if (showGlobalHeaderComment == true)
//    {
//        output.Append(Global.GetHeaderComment());
//    }
//    for (int i = 0; i < lines.Length; i++)
//    {
//        string item = lines[i];
//        if (item.Trim().StartsWith("#") == true)
//        {
//            //It's a comment, just append it back to the output
//            output.Append(item);
//        }
//        else if (item.Trim().StartsWith("trigger:") == true)
//        {
//            //trigger:
//            //- master
//            string triggerText = item;
//            int n = i;
//            string newItem = "";
//            while (lines.Length - 1 >= i + n)
//            {
//                n++;
//                newItem = lines[0 + n];
//                triggerText += Environment.NewLine + newItem;
//            }
//            i += n;
//            output.Append("on: [push]");
//        }
//        else
//        {
//            //output.Append(Global.GetLineComment());
//            output.Append(item);
//        }
//    }

//    return output.ToString();
//}