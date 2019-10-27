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
    public class StepsTest
    {
        [TestMethod]
        public void TestSteps()
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
- task: UseDotNet@2
  displayName: 'Use .NET Core sdk'
  inputs:
    packageType: sdk
    version: 2.2.203
- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
  displayName: dotnet build $(buildConfiguration) part 1
- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
  displayName: dotnet build $(buildConfiguration) part 2";

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.IsTrue(output != null);
        }


        [TestMethod]
        public void UseDotNetIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: UseDotNet@2
  displayName: 'Use .NET Core sdk'
  inputs:
    packageType: sdk
    version: 2.2.203";

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expectedOutput = @"
    - name: Use .NET Core sdk
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 2.2.203";
            Assert.AreEqual(output, expectedOutput);
        }

        [TestMethod]
        public void BuildDotNetIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
  displayName: dotnet build $(buildConfiguration) part 1";

            //Act
            string output = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: dotnet build $buildConfiguration part 1
  run: dotnet build --configuration $buildConfiguration WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
";
            Assert.AreEqual(output, expectedOutput);
        }


        [TestMethod]
        public void TestStepsWithSinglelinePowerShell()
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
- task: PowerShell@2
  displayName: 'PowerShell test task'
  inputs:
    targetType: inline
    script: Write-Host 'Hello World'";

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.IsTrue(output != null);
        }

        [TestMethod]
        public void TestStepsWithMultilinePowerShell()
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
- task: PowerShell@2
  displayName: 'PowerShell test task'
  inputs:
    targetType: inline
    script: |
      Write-Host 'Hello World'
      Write-Host 'Hello World2'";

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.IsTrue(output != null);
        }

    }
}