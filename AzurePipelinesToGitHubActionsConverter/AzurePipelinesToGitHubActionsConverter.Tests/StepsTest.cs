using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class StepsTest
    {
        //TODO: Resolve difference between Ubuntu and Windows
        //[TestMethod]
        //public void InvalidStepIndividualStepTest()
        //{
        //    //Arrange
        //    Conversion conversion = new Conversion();
        //    string yaml = "- task: invalid fake task";

        //    //Act
        //    ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

        //    //Assert
        //    string expected = "- name: '***This step could not be migrated***'\r\n  run: \r\n    #task: invalid fake task\r\n  shell: powershell";

        //    Assert.AreEqual(gitHubOutput.actionsYaml, TestUtility.TrimNewLines(expected));
        //}

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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- run: echo your commands here
  shell: cmd
";
            expected = TestUtility.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test PowerShell
  run: Write-Host 'some text'
  shell: powershell
";
            expected = TestUtility.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test pwsh
  run: Write-Host 'some text'
  shell: pwsh
";
            expected = TestUtility.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test bash
  run: Write-Host 'some text'
  shell: bash
";
            expected = TestUtility.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Use .NET Core sdk
  uses: actions/setup-dotnet@v1
  with:
    dotnet-version: 2.2.203
";
            expected = TestUtility.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: dotnet build $buildConfiguration part 1
  run: dotnet build --configuration $buildConfiguration WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
";
            expected = TestUtility.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            Assert.IsTrue(gitHubOutput.actionsYaml != null);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            Assert.IsTrue(gitHubOutput.actionsYaml != null);
        }

    }
}