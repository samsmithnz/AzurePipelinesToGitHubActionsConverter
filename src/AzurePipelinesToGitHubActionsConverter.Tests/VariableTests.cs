using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class VariableTests
    {

        [TestMethod]
        public void VariablesTest()
        {
            //Arrange
            string input = @"
trigger:
- master
variables:
  buildConfiguration: Release
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: ubuntu-latest
  variables:
    myJobVariable: 'data'
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build $(myJobVariable)";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
env:
  buildConfiguration: Release
jobs:
  Build:
    name: Build job
    runs-on: ubuntu-latest
    env:
      myJobVariable: data
    steps:
    - uses: actions/checkout@v2
    - name: dotnet build ${{ env.myJobVariable }}
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void VariablesWithSpacesTest()
        {
            //Arrange
            string input = @"
trigger:
- master
variables:
  buildConfiguration: Release
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: ubuntu-latest
  variables:
    myJobVariable: 'data'
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $buildConfiguration
    displayName: dotnet build $myJobVariable
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
env:
  buildConfiguration: Release
jobs:
  Build:
    name: Build job
    runs-on: ubuntu-latest
    env:
      myJobVariable: data
    steps:
    - uses: actions/checkout@v2
    - name: dotnet build ${{ env.myJobVariable }}
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void SimpleVariablesTest()
        {
            //Arrange
            string input = @"
variables:
  configuration: debug
  platform: x64";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
env:
  configuration: debug
  platform: x64";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void ComplexVariablesTest()
        {
            //Arrange
            string input = @"
variables:
- name: myVariable
  value: myValue
- name: myVariable2
  value: myValue2
- group: myVariablegroup
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
env:
  myVariable: myValue
  myVariable2: myValue2
  group: myVariablegroup";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void SimpleVariablesAndSimpleTriggerTest()
        {
            //Arrange
            string input = @"
trigger:
- master
- develop
variables:
  myVariable: myValue
  myVariable2: myValue2";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
    - develop
env:
  myVariable: myValue
  myVariable2: myValue2";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ComplexVariablesAndComplexTriggerTest()
        {
            //Arrange
            string input = @"
trigger:
  batch: true
  branches:
    include:
    - features/*
    exclude:
    - features/experimental/*
  paths:
    include:
    - README.md
  tags:
    include:
    - v1     
    - v1.*
variables:
- name: myVariable
  value: myValue
- name: myVariable2
  value: myValue2
- group: myVariablegroup
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - features/*
    paths:
    - README.md
    tags:
    - v1
    - v1.*
env:
  myVariable: myValue
  myVariable2: myValue2
  group: myVariablegroup";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        } 
        
        [TestMethod]
        public void VariableInsertationTest()
        {
            //Arrange
            string input = @"
variables:
  ${{ if ne(variables['Build.SourceBranchName'], 'master') }}:
    prId: ""$(System.PullRequest.PullRequestId)""
  ${{ if eq(variables['Build.SourceBranchName'], 'master') }}:
    prId: '000'
  prName: ""PR$(prId)""
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
env:
  prId: 000
  prName: PR${{ env.prId }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void VariablesWithConditionalStatementsTest()
        {
            //Arrange
            string input = @"
variables:
  # Agent VM image name
  vmImageName: 'ubuntu-latest'
  
  {{#if reviewApp}}
  # Name of the new namespace being created to deploy the PR changes.
  k8sNamespaceForPR: 'inconditionalstatement'
  {{/if}}";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
env:
  vmImageName: ubuntu-latest
  k8sNamespaceForPR: inconditionalstatement";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void VariablesWithConditionalStatementsVariationTest()
        {
            //Arrange
            string input = @"
  variables:
    ${{ if ne(variables['Build.SourceBranchName'], 'master') }}:
      prId: '00A'
    ${{ if eq(variables['Build.SourceBranchName'], 'master') }}:
      prId: '00B'
    prUC: '002'
    prLC: '003'";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
env:
  prId: 00B
  prUC: 002
  prLC: 003";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ParametersReservedWordTest()
        {
            //Arrange
            string input = @"
parameters: # defaults for any parameters that aren't specified
  plainVar: 'ok'
  environment: 'Dev'
  strategy: Dev
  pool: 'Dev'
variables: 
  plainVar2: 'ok2'
  environment2: 'Dev2'
  strategy2: Dev2
  pool2: 'Dev2'  
";

            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubActionV3(input);

            //Assert
            string expected = @"
env:
  plainVar: ok
  environment: Dev
  strategy: Dev
  pool: Dev
  plainVar2: ok2
  environment2: Dev2
  strategy2: Dev2
  pool2: Dev2
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}