using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class ConversionTest
    {
        [TestMethod]
        public void TestName()
        {
            //Arrange
            string input = "name: test ci pipelines";
            Conversion conversion = new Conversion();

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "name: test ci pipelines" + Environment.NewLine);
        }

        [TestMethod]
        public void TestTriggerString()
        {
            //Arrange
            string input = "trigger:" + Environment.NewLine +
                           "- master";
            Conversion conversion = new Conversion();

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "on: [push]" + Environment.NewLine);
        }

        [TestMethod]
        public void TestPoolUbuntuLatestString()
        {
            //Arrange
            string input = "pool:" + Environment.NewLine +
                           "  vmImage: ubuntu-latest";
            Conversion conversion = new Conversion();

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "jobs:" + Environment.NewLine +
                                    "  build:" + Environment.NewLine +
                                    "    runs-on: ubuntu-latest" + Environment.NewLine);
        }

        [TestMethod]
        public void TestPoolWindowsLatestString()
        {
            //Arrange
            string input = "pool: " + Environment.NewLine +
                           "  vmImage: windows-latest";
            Conversion conversion = new Conversion();

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "jobs:" + Environment.NewLine +
                                    "  build:" + Environment.NewLine +
                                    "    runs-on: windows-latest" + Environment.NewLine);
        }

        [TestMethod]
        public void TestVariables()
        {
            //Arrange
            string input = "variables:" + Environment.NewLine +
                           "  vmImage: windows-latest" + Environment.NewLine +
                           "  buildConfiguration: Release" + Environment.NewLine +
                           "  buildPlatform: Any CPU" + Environment.NewLine +
                           "  buildNumber: 1.1.0.0" + Environment.NewLine;
            Conversion conversion = new Conversion();

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "env:" + Environment.NewLine +
                                    "  vmImage: windows-latest" + Environment.NewLine +
                                    "  buildConfiguration: Release" + Environment.NewLine +
                                    "  buildPlatform: Any CPU" + Environment.NewLine +
                                    "  buildNumber: 1.1.0.0" + Environment.NewLine);
        }

        [TestMethod]
        public void TestSampleGitHubActionObjectToYaml()
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
                        "on: [push]" + Environment.NewLine +
                        "jobs:" + Environment.NewLine +
                        "  build:" + Environment.NewLine +
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
        public void TestSampleAzurePipelineObjectToYaml()
        {
            //Arrange
            Conversion conversion = new Conversion();

            //Act
            string yaml = conversion.CreateAzurePipeline();

            //Assert
            Assert.IsTrue(yaml != null);
        }

        [TestMethod]
        public void TestStagesWithAzurePipelineYamlToObject()
        {
            //stages:
            //- stage: Build
            //  displayName: 'Build/Test Stage'
            //  jobs:
            //  - job: Build
            //    displayName: 'Build job'
            //    pool:
            //      vmImage: $(vmImage)
            //    steps:

            //Arrange
            Conversion conversion = new Conversion();
            string yaml =
@"name: test dotnet build with stages 
trigger: 
- master 
variables: 
  buildConfiguration: Release 
  randomVariable: 14 
stages: 
- stage: Build 
  displayName: Build/Test Stage 
  jobs: 
  - job: Build 
    displayName: Build job 
    pool: 
      vmImage: $(vmImage) 
    steps: 
    - script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj 
      displayName: dotnet build $(buildConfiguration)";

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.IsTrue(output != null);
        }

        [TestMethod]
        public void TestJobsWithAzurePipelineYamlToObject()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master
variables:
  buildConfiguration: Release
  vmImage: windows-latest
jobs:
- job: Build
  displayName: Build job part A
  pool: 
    vmImage: $(vmImage) 
  steps: 
  - script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
    displayName: dotnet build $(buildConfiguration) part A1
- job: Build2
  displayName: Build job part B 
  pool: 
    vmImage: $(vmImage) 
  steps: 
  - script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
    displayName: dotnet build $(buildConfiguration) part B1
  - script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
    displayName: dotnet build $(buildConfiguration) part B2";

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.IsTrue(output != null);
        }

        [TestMethod]
        public void TestStepsWithAzurePipelineYamlToObject()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master
pool:
  vmImage: ubuntu-latest
variables:
  buildConfiguration: Release
steps:
- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
  displayName: dotnet build $(buildConfiguration) part 1
- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
  displayName: dotnet build $(buildConfiguration) part 2";

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.IsTrue(output != null);
        }

        //[TestMethod]
        //public void TestLargeAzurePipelineYamlToObject()
        //{
        //    //Arrange
        //    Conversion conversion = new Conversion();
        //    string yaml =
        //        "trigger:" + Environment.NewLine +
        //        "- master" + Environment.NewLine +
        //        "pr:" + Environment.NewLine +
        //        "  branches:" + Environment.NewLine +
        //        "    include:" + Environment.NewLine +
        //        "    - '*'  # must quote since \" * \" is a YAML reserved character; we want a string" + Environment.NewLine +
        //        "" + Environment.NewLine +
        //        "variables:" + Environment.NewLine +
        //        "  vmImage: 'windows-latest'" + Environment.NewLine +
        //        "  buildConfiguration: 'Release'" + Environment.NewLine +
        //        "  buildPlatform: 'Any CPU'" + Environment.NewLine +
        //        "  buildNumber: '1.1.0.0'" + Environment.NewLine +
        //        "";// + Environment.NewLine +
        //        //"stages:" + Environment.NewLine +
        //        //"- stage: Build" + Environment.NewLine +
        //        //"  displayName: 'Build/Test Stage'" + Environment.NewLine +
        //        //"  jobs:" + Environment.NewLine +
        //        //"  - job: Build" + Environment.NewLine +
        //        //"    displayName: 'Build job'" + Environment.NewLine +
        //        //"    pool:" + Environment.NewLine +
        //        //"      vmImage: $(vmImage)" + Environment.NewLine +
        //        //"    steps:" + Environment.NewLine +
        //        //"    - task: PowerShell@2" + Environment.NewLine +
        //        //"      displayName: 'Generate build version number'" + Environment.NewLine +
        //        //"      inputs:" + Environment.NewLine +
        //        //"        targetType: 'inline'" + Environment.NewLine +
        //        //"        script: |" + Environment.NewLine +
        //        //"         Write -Host \"Generating Build Number\"" + Environment.NewLine;// +
        //        //"" + Environment.NewLine +
        //        //"    - task: CopyFiles@2" + Environment.NewLine +
        //        //"      displayName: 'Copy environment ARM template files to: $(build.artifactstagingdirectory)'" + Environment.NewLine +
        //        //"      inputs:" + Environment.NewLine +
        //        //@"        SourceFolder: '$(system.defaultworkingdirectory)\FeatureFlags\FeatureFlags.ARMTemplates'" + Environment.NewLine +
        //        //@"        Contents: '**\*' # **\* = Copy all files and all files in sub directories" + Environment.NewLine +
        //        //@"        TargetFolder: '$(build.artifactstagingdirectory)\ARMTemplates'" + Environment.NewLine +
        //        //"" + Environment.NewLine +
        //        ////"    - task: DotNetCoreCLI@2" + Environment.NewLine +
        //        ////"      displayName: 'Test dotnet code projects'" + Environment.NewLine +
        //        ////"      inputs:" + Environment.NewLine +
        //        ////"        command: test" + Environment.NewLine +
        //        ////"        projects: |" + Environment.NewLine +
        //        ////"         FeatureFlags /FeatureFlags.Tests/FeatureFlags.Tests.csproj" + Environment.NewLine +
        //        ////"        arguments: '--configuration $(buildConfiguration) --logger trx --collect "Code coverage" --settings:$(Build.SourcesDirectory)\FeatureFlags\FeatureFlags.Tests\CodeCoverage.runsettings'" + Environment.NewLine +
        //        ////"" + Environment.NewLine +
        //        //"    - task: DotNetCoreCLI@2" + Environment.NewLine +
        //        //"      displayName: 'Publish dotnet core projects'" + Environment.NewLine +
        //        //"      inputs:" + Environment.NewLine +
        //        //"        command: publish" + Environment.NewLine +
        //        //"        publishWebProjects: false" + Environment.NewLine +
        //        //"        projects: |" + Environment.NewLine +
        //        //"         FeatureFlags /FeatureFlags.Service/FeatureFlags.Service.csproj" + Environment.NewLine +
        //        //"         FeatureFlags /FeatureFlags.Web/FeatureFlags.Web.csproj" + Environment.NewLine +
        //        //"        arguments: '--configuration $(buildConfiguration) --output $(build.artifactstagingdirectory) -p:Version=$(buildNumber)'" + Environment.NewLine +
        //        //"        zipAfterPublish: true" + Environment.NewLine +
        //        //"" + Environment.NewLine +
        //        //"    # Publish the artifacts" + Environment.NewLine +
        //        //"    - task: PublishBuildArtifacts@1" + Environment.NewLine +
        //        //"      displayName: 'Publish Artifact'" + Environment.NewLine +
        //        //"      inputs:" + Environment.NewLine +
        //        //"        PathtoPublish: '$(build.artifactstagingdirectory)'";
        //    //Act
        //    IDeserializer deserializer = new DeserializerBuilder()
        //                                       .Build();
        //    AzurePipelinesRoot yamlObject = deserializer.Deserialize<AzurePipelinesRoot>(yaml);

        //    //Assert
        //    Assert.IsTrue(yamlObject != null);
        //}
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




