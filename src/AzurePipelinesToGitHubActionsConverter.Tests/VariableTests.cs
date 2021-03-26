using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
- main
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - main
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
- main
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - main
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
        public void SimpleVariablesNoneTest()
        {
            //Arrange
            string input = @"
variables:";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void SimpleVariablesAndSimpleTriggerTest()
        {
            //Arrange
            string input = @"
trigger:
- main
- develop
variables:
  myVariable: myValue
  myVariable2: myValue2";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - main
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
        public void ComplexVariablesWithTwoVariableGroupsTest()
        {
            //Arrange
            string input = @"
variables:
- group: myVariablegroup1
- group: myVariablegroup2
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
env:
  group: myVariablegroup1";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void VariableInsertationTest()
        {
            //Arrange
            string input = @"
variables:
  ${{ if ne(variables['Build.SourceBranchName'], 'main') }}:
    prId: ""$(System.PullRequest.PullRequestId)""
  ${{ if eq(variables['Build.SourceBranchName'], 'main') }}:
    prId: '000'
  prName: ""PR$(prId)""
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
    ${{ if ne(variables['Build.SourceBranchName'], 'main') }}:
      prId: '00A'
    ${{ if eq(variables['Build.SourceBranchName'], 'main') }}:
      prId: '00B'
    prUC: '002'
    prLC: '003'";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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


        [TestMethod]
        public void ParametersAndVariablesComplexTest()
        {
            //Arrange
            string input = @"
parameters: # defaults for any parameters that aren't specified
  - name: plainVar
    type: string
    default: ok
  - name: environment
    type: string
    default: Dev
  - name: strategy
    type: string
    default: Dev
  - name: pool
    type: string
variables: 
  - name: plainVar2
    value: ok2
    readonly: true
  - name: environment2
    value: dev2
    readonly: true
  - name: strategy2
    value: dev2
    readonly: false
  - name: pool2
    value: """"
    readonly: false
";

            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
env:
  plainVar: ok
  environment: Dev
  strategy: Dev
  pool: ''
  plainVar2: ok2
  environment2: dev2
  strategy2: dev2
  pool2: ''
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }


        [TestMethod]
        public void VariablesWithTemplateTest()
        {
            //Arrange
            string input = @"
variables:
- template: vars.yml  # Template reference

steps:
- script: echo My favorite vegetable is ${{ variables.favoriteVeggie }}.
";

            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
env:
  template: vars.yml
jobs:
  build:
    steps:
    - uses: actions/checkout@v2
    - run: echo My favorite vegetable is ${{ env. variables.favoriteVeggie  }}.
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

    }
}