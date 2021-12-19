using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class TemplateTests
    {
        [TestMethod]
        public void TemplatesCallingYamlTest()
        {
            //Arrange
            string input = @"
jobs:
- template: azure-pipelines-build-template.yml
  parameters:
    buildConfiguration: 'Release'
    buildPlatform: 'Any CPU'
    vmImage: windows-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: Azure DevOps template does not have an equivalent in GitHub Actions yet
jobs:
  job_1_template:
    # 'Note: Azure DevOps template does not have an equivalent in GitHub Actions yet'
    steps:
    - uses: actions/checkout@v2";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void TemplatesChildYAMLWithParametersTest()
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
          Write-Host ""Hello world ${{parameters.buildConfiguration}} ${{parameters.buildPlatform}}""";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
    - uses: actions/checkout@v2
    - name: Test
      run: Write-Host ""Hello world ${{ env.buildConfiguration }} ${{ env.buildPlatform }}""
      shell: powershell";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

    }
}