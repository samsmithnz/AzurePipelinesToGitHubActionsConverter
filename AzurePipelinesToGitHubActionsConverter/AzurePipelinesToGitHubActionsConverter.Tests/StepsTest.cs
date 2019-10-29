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
        public void InvalidStepIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: invalid false test
";

            //Act
            string output = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: '***This step is not currently supported***: '
";

            Assert.AreEqual(output, TestUtility.TrimNewLines(expectedOutput));
        }

        [TestMethod]
        public void CmdLineIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: CmdLine@2
  inputs:
    script: echo your commands here 
";

            //Act
            string output = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- run: echo your commands here
  shell: cmd
";

            Assert.AreEqual(output, TestUtility.TrimNewLines(expectedOutput));
        }  
        
        [TestMethod]
        public void PowerShellIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- powershell: Write-Host 'some text'
  displayName: test PowerShell
";

            //Act
            string output = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: test PowerShell
  run: Write-Host 'some text'
  shell: powershell
";

            Assert.AreEqual(output, TestUtility.TrimNewLines(expectedOutput));
        }  
        
        [TestMethod]
        public void PwshIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- pwsh: Write-Host 'some text'
  displayName: test pwsh
";

            //Act
            string output = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: test pwsh
  run: Write-Host 'some text'
  shell: pwsh
";

            Assert.AreEqual(output, TestUtility.TrimNewLines(expectedOutput));
        }

        [TestMethod]
        public void BashIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- bash: Write-Host 'some text'
  displayName: test bash
";

            //Act
            string output = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: test bash
  run: Write-Host 'some text'
  shell: bash
";

            Assert.AreEqual(output, TestUtility.TrimNewLines(expectedOutput));
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
            string output = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: Use .NET Core sdk
  uses: actions/setup-dotnet@v1
  with:
    dotnet-version: 2.2.203
";

            Assert.AreEqual(output, TestUtility.TrimNewLines(expectedOutput));
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

            Assert.AreEqual(output, TestUtility.TrimNewLines(expectedOutput));
        }


        [TestMethod]
        public void PowershellWithSinglelineIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: PowerShell@2
  displayName: 'PowerShell test task'
  inputs:
    targetType: inline
    script: Write-Host 'Hello World'";

            //Act
            string output = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            Assert.IsTrue(output != null);
        }

        [TestMethod]
        public void PowershellWithMultilineIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: PowerShell@2
  displayName: 'PowerShell test task'
  inputs:
    targetType: inline
    script: |
      Write-Host 'Hello World'
      Write-Host 'Hello World2'";

            //Act
            string output = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            Assert.IsTrue(output != null);
        }

    }
}