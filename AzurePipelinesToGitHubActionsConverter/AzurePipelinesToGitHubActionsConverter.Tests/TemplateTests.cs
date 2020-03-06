using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class TemplateTests
    {
        [TestMethod]
        public void BuildTemplatesParentTest()
        {
            //Arrange
            string input = @"
stages:
- stage: Build
  displayName: 'Build stage'
  jobs:
  - template: azure-pipelines-build-template.yml
    parameters:
      buildConfiguration: 'Release'
      buildPlatform: 'Any CPU'
      vmImage: windows-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#NOTE: Azure DevOps template does not have an equivalent in GitHub Actions yet
jobs:
  Build_Stage_Template:
    #: 'NOTE: Azure DevOps template does not have an equivalent in GitHub Actions yet'
    steps:
    - uses: actions/checkout@v1";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void BuildTemplatesChildTest()
        {
            //Arrange
            string input = @"
parameters: 
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'

jobs:
  - job: Build
    displayName: 'Build job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      displayName: 'Test'
      inputs:
        targetType: inline
        script: |
          Write-Host ""Hello world $(buildConfiguration) $(buildPlatform)""";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
env:
  buildConfiguration: Release
  buildPlatform: Any CPU
jobs:
  Build:
    name: Build job
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v1
    - name: Test
      run: Write-Host ""Hello world ${{ env.buildConfiguration }} ${{ env.buildPlatform }}""
      shell: powershell";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        } 
        
        [TestMethod]
        public void BuildTemplatesJobWithDeploymentTest()
        {
            //Arrange
            string input = @"
jobs:
  - deployment: DeployInfrastructure
    displayName: Deploy job
    environment: Dev
    pool:
      vmImage: windows-latest     
    strategy:
      runOnce:
        deploy:
          steps:
          - task: PowerShell@2
            displayName: 'Test'
            inputs:
              targetType: inline
              script: |
                Write-Host ""Hello world""";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#NOTE: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yet
jobs:
  DeployInfrastructure:
    #: 'NOTE: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yet'
    name: Deploy job
    runs-on: windows-latest
    strategy: {}
    steps:
    - name: Test
      run: Write-Host ""Hello world""
      shell: powershell";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


    }
}