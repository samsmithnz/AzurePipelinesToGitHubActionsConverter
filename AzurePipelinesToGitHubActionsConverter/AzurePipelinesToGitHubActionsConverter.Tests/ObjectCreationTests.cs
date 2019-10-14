using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class ObjectCreationTests
    {
      
        [TestMethod]
        public void TestSampleGitHubActionObjectToYaml()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Build the GitHub actions object
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

            GitHubActionsRoot githubActionsYAML = new GitHubActionsRoot
            {
                name = "CI",
                on = new string[] { "push" },
                jobs = new Dictionary<string, Core.GitHubActions.Job>
                {
                    {
                        "build",
                        new Core.GitHubActions.Job
                        {
                            runs_on = "ubuntu-latest",
                            steps = new Core.GitHubActions.Step[]
                            {
                                new Core.GitHubActions.Step
                                {
                                    uses = "actions/checkout @v1"
                                },
                                new Core.GitHubActions.Step
                                {
                                    name = "Build with dotnet",
                                    run = "dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release"
                                },
                                new Core.GitHubActions.Step
                                {
                                    name = "Publish with dotnet",
                                    run = "dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release"
                                },
                                new Core.GitHubActions.Step
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

            //Act
            string yaml = conversion.WriteYAMLFile<GitHubActionsRoot>(githubActionsYAML);


            //Assert
            Assert.IsTrue(yaml != null);
        }

      
        [TestMethod]
        public void TestSampleAzurePipelineObjectToYaml()
        {
            //Arrange
            Conversion conversion = new Conversion();
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
                steps = new Core.AzurePipelines.Step[]
                {
                    new Core.AzurePipelines.Step {
                        script = "dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj",
                        displayName = "dotnet build $(buildConfiguration)"
                    }
                }
            };

            //Act
            string yaml = conversion.WriteYAMLFile<AzurePipelinesRoot>(azurePipelinesYAML);

            //Assert
            Assert.IsTrue(yaml != null);
        }

    }
}
