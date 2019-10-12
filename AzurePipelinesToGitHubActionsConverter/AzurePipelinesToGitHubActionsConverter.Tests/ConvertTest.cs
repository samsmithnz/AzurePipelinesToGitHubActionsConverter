using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class ConversionTest
    {
        //[TestMethod]
        //public void TestComment()
        //{
        //    //Arrange
        //    bool showGlobalHeaderComment = false;
        //    string input = "   #";
        //    Conversion conversion = new Conversion();

        //    //Act
        //    string output = conversion.ConvertPipelineToAction(input, showGlobalHeaderComment);

        //    //Assert
        //    Assert.AreEqual(input, output);
        //}

        //[TestMethod]
        //public void TestPoolUbuntuLatestString()
        //{
        //    //Arrange
        //    bool showGlobalHeaderComment = false;
        //    string input = "pool:" + Environment.NewLine +
        //                   "  vmImage: 'ubuntu-latest'";
        //    Conversion conversion = new Conversion();

        //    //Act
        //    string output = conversion.ConvertPipelineToAction(input, showGlobalHeaderComment);

        //    //Assert
        //    Assert.AreEqual(output, "runs-on: ubuntu-latest");
        //}

        //[TestMethod]
        //public void TestPoolWindowsLatestString()
        //{
        //    //Arrange
        //    bool showGlobalHeaderComment = false;
        //    string input = "pool: " + Environment.NewLine +
        //                   "  vmImage: 'windows-latest' ";
        //    Conversion conversion = new Conversion();

        //    //Act
        //    string output = conversion.ConvertPipelineToAction(input, showGlobalHeaderComment);

        //    //Assert
        //    Assert.AreEqual(output, "runs-on: windows-latest");
        //}

        //[TestMethod]
        //public void TestTriggerString()
        //{
        //    //Arrange
        //    bool showGlobalHeaderComment = false;
        //    string input = "" + Environment.NewLine +
        //        "trigger:" + Environment.NewLine +
        //        "- master" + Environment.NewLine;
        //    Conversion conversion = new Conversion();

        //    //Act
        //    string output = conversion.ConvertPipelineToAction(input, showGlobalHeaderComment);

        //    //Assert
        //    Assert.AreEqual(output, "on: [push]");
        //}

        [TestMethod]
        public void TestGitHubActionObjectToYaml()
        {
            //Arrange
            Conversion conversion = new Conversion();

            //Act
            string yaml = conversion.CreateGitHubAction();

            //Assert
            Assert.IsTrue(yaml != null);
        }

        [TestMethod]
        public void TestGitHubActionYamlToObject()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = "name: CI" + Environment.NewLine +
                        "on: '[push]'" + Environment.NewLine +
                        "jobs:" + Environment.NewLine +
                        "- build:" + Environment.NewLine +
                        "    runsOn: ubuntu-latest" + Environment.NewLine +
                        "    steps:" + Environment.NewLine +
                        "    - uses: actions/checkout @v1" + Environment.NewLine +
                        "    - name: Build with dotnet" + Environment.NewLine +
                        "      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release" + Environment.NewLine +
                        "    - name: Publish with dotnet" + Environment.NewLine +
                        "      run: dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release" + Environment.NewLine +
                        "    - name: publish build artifacts back to GitHub" + Environment.NewLine +
                        "      uses: actions/upload-artifact @master" + Environment.NewLine +
                        "      with:" + Environment.NewLine +
                        "        name: serviceapp" + Environment.NewLine +
                        "        path: WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish";

            //Act
            IDeserializer deserializer = new DeserializerBuilder()
                                    .Build();
            GitHubActionsRoot yamlObject = deserializer.Deserialize<GitHubActionsRoot>(yaml);

            //Assert
            Assert.IsTrue(yamlObject != null);
        }

        [TestMethod]
        public void TestAzurePipelineObjectToYaml()
        {
            //Arrange
            Conversion conversion = new Conversion();

            //Act
            string yaml = conversion.CreateAzurePipeline();

            //Assert
            Assert.IsTrue(yaml != null);
        }

        [TestMethod]
        public void TestAzurePipelineYamlToObject()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml =
                "trigger:" + Environment.NewLine +
                "- master" + Environment.NewLine +
                "pool:" + Environment.NewLine +
                "  vmImage: ubuntu-latest" + Environment.NewLine +
                "variables:" + Environment.NewLine +
                "  buildConfiguration: Release" + Environment.NewLine +
                "  anotherVariable: 12" + Environment.NewLine +
                "  yetAnotherVariable: var" + Environment.NewLine +
                "steps:" + Environment.NewLine +
                "- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj" + Environment.NewLine +
                "  displayName: dotnet build $(buildConfiguration)";

            //Act
            IDeserializer deserializer = new DeserializerBuilder()
                                               .Build();
            AzurePipelinesRoot yamlObject = deserializer.Deserialize<AzurePipelinesRoot>(yaml);
            //StringReader sr = new StringReader(yaml);
            //var yamlObject = deserializer.Deserialize(sr);

            //Assert
            Assert.IsTrue(yamlObject != null);
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
//- script: dotnet build --configuration $(buildConfiguration)
//  displayName: dotnet build $(buildConfiguration)




//name: CI
// 
//on: [push]
// 
//jobs:
//  build:
// 
//    runs-on: ubuntu-latest
// 
//    steps:
//    # checkout the repo
//    - uses: actions/checkout @v1
// 
//    # install dependencies, build, and test
//    - name: Setup.NET Core
//      uses: actions/setup-dotnet @v1
//      with:
//        dotnet-version: 3.0.100
//    - name: Build with dotnet
//      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
//    - name: Publish with dotnet
//      run: dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
//    - name: publish build artifacts back to GitHub
//      uses: actions/upload-artifact @master
//      with:
//        name: serviceapp
//        path: WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish




