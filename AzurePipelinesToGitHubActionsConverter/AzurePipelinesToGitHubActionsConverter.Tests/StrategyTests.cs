using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class StrategyTests
    {

        [TestMethod]
        public void StrategyTest()
        {
            //strategy:
            //  matrix:
            //    linux:
            //      imageName: "ubuntu-16.04"
            //    mac:
            //      imageName: "macos-10.13"
            //    windows:
            //      imageName: "vs2017-win2016"

            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master
strategy:
  maxParallel: 3
  parallel: 1
  matrix:
    linux:
      imageName: ubuntu-16.04
    mac:
      imageName: macos-10.13
    windows:
      imageName: vs2017-win2016
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: $(imageName)
  variables:
    buildConfiguration: Debug
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build part 1
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            //Note that we are using the longer form, as sequence flow (showing an array like: [ubuntu-16.04, macos-10.13, vs2017-win2016]), doesn't exist in this YAML Serializer yet.
            string expected = @"
on:
  push:
    branches:
    - master
jobs:
  Build:
    name: Build job
    runs-on: ${{ matrix.imageName }}
    strategy:
      matrix:
        imageName:
        - ubuntu-16.04
        - macos-10.13
        - vs2017-win2016
      max-parallel: 3
    env:
      buildConfiguration: Debug
    steps:
    - uses: actions/checkout@v1
    - name: dotnet build part 1
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void StrategyRunOnceDeploymentTest()
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yet
jobs:
  DeployInfrastructure:
    #: 'Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yet'
    name: Deploy job
    runs-on: windows-latest
    steps:
    - name: Test
      run: Write-Host ""Hello world""
      shell: powershell";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}