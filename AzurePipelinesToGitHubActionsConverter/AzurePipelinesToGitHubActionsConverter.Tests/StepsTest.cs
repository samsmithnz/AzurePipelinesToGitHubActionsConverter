using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
- task: invalid fake task
";

            //Act
            GitHubConversion gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = "- name: '***This step could not be migrated***: '\r\n  run: \r\n    #task: invalid fake task\r\n\r\n  shell: powershell";

            Assert.AreEqual(gitHubOutput.yaml, TestUtility.TrimNewLines(expectedOutput));
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
            GitHubConversion gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- run: echo your commands here
  shell: cmd
";

            Assert.AreEqual(gitHubOutput.yaml, TestUtility.TrimNewLines(expectedOutput));
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
            GitHubConversion gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: test PowerShell
  run: Write-Host 'some text'
  shell: powershell
";

            Assert.AreEqual(gitHubOutput.yaml, TestUtility.TrimNewLines(expectedOutput));
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
            GitHubConversion gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: test pwsh
  run: Write-Host 'some text'
  shell: pwsh
";

            Assert.AreEqual(gitHubOutput.yaml, TestUtility.TrimNewLines(expectedOutput));
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
            GitHubConversion gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: test bash
  run: Write-Host 'some text'
  shell: bash
";

            Assert.AreEqual(gitHubOutput.yaml, TestUtility.TrimNewLines(expectedOutput));
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
            GitHubConversion gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: Use .NET Core sdk
  uses: actions/setup-dotnet@v1
  with:
    dotnet-version: 2.2.203
";

            Assert.AreEqual(gitHubOutput.yaml, TestUtility.TrimNewLines(expectedOutput));
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
            GitHubConversion gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expectedOutput = @"
- name: dotnet build $buildConfiguration part 1
  run: dotnet build --configuration $buildConfiguration WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
";

            Assert.AreEqual(gitHubOutput.yaml, TestUtility.TrimNewLines(expectedOutput));
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
            GitHubConversion gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            Assert.IsTrue(gitHubOutput.yaml != null);
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
            GitHubConversion gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            Assert.IsTrue(gitHubOutput.yaml != null);
        }

    }
}