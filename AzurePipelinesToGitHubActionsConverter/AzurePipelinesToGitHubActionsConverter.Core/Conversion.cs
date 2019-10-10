using System;
using System.IO;
using System.Text;
using System.Xml;
using YamlDotNet.RepresentationModel;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;

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
                            output.Append(Global.GetSpaces(nodeStartPosition));
                            output.Append("on: [push]");
                            break;

                        case "pool":
                            var pool = deserializer.Deserialize<Pool>(entry.ToString());
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

        public bool CreateGitHubAction()
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

            Root root = new Root
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
                            steps = new Step[]
                            {
                                new Step
                                {
                                    uses = "actions/checkout @v1"
                                },
                                new Step
                                {
                                    name = "Build with dotnet",
                                    run = "dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release"
                                },
                                new Step
                                {
                                    name = "Publish with dotnet",
                                    run = "dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release"
                                },
                                new Step
                                {
                                    name = "publish build artifacts back to GitHub",
                                    uses = "actions/upload-artifact @master"
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

            
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(root);
            Console.WriteLine(yaml);


            //       Step step4 = new Step();
            //step4.name = "publish build artifacts back to GitHub";
            //step4.uses = "actions/upload-artifact @master";
            //With with = new With();
            //with.name = "serviceapp";
            //with.path = "WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish";
            //step4.with = with;
            //build.steps.Add(step3);



            //}
            //root.jobs.Add(job);


            //Convert the object into a YAML document

            return true;
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
//  vmImage: 'ubuntu-latest'

//variables:
//  buildConfiguration: 'Release'

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