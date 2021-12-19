using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    //pool can be either:
    //pool:
    //  name: string  # name of the pool to run this job in
    //  demands: string | [ string ]  # see the following "Demands" topic
    //  vmImage: string # name of the VM image you want to use; valid only in the Microsoft-hosted pool

    //OR:
    //pool: string # name of the private pool to run this job in

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class PoolTests
    {
        [TestMethod]
        public void PoolVMImageUbuntuLatestStringTest()
        {
            //Arrange
            string input = @"
pool:
  vmImage: ubuntu-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
jobs:
  build:
    runs-on: ubuntu-latest";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PoolStringUbuntuNameLatestTest()
        {
            //Arrange
            string input = @"
pool:
  name: ubuntu-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
jobs:
  build:
    runs-on: ubuntu-latest";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PoolVMImageWindowsLatestStringTest()
        {
            //Arrange
            string input = @"
pool: 
  vmImage: windows-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
jobs:
  build:
    runs-on: windows-latest";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PoolStringWindowsLatestStringTest()
        {
            //Arrange
            string input = @"
pool:  windows-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
jobs:
  build:
    runs-on: windows-latest";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PoolNameAndDemandsTest()
        {
            //Arrange
            string input = @"
pool:
  name: Hosted VS2017
  demands: npm";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet
jobs:
  build:
    runs-on: Hosted VS2017";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PoolNameAndDemandsListTest()
        {
            //Arrange
            string input = @"
pool:
  name: Hosted VS2017
  demands: 
  - npm
  - Agent.OS -equals Windows_NT";

            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet
jobs:
  build:
    runs-on: Hosted VS2017";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }


        [TestMethod]
        public void PoolVMImageAndDemandsTest()
        {
            //Arrange
            string input = @"
pool:
  vmImage: Hosted VS2017
  demands: npm";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet
jobs:
  build:
    runs-on: Hosted VS2017";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PoolVMImageAndDemandsListTest()
        {
            //Arrange
            string input = @"
pool:
  vmImage: Hosted VS2017
  demands: 
  - npm
  - Agent.OS -equals Windows_NT";

            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet
jobs:
  build:
    runs-on: Hosted VS2017";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PoolStringSimpleNameTest()
        {
            //Arrange
            string input = @"
pool: 'Pipeline-Demo-Windows'";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
jobs:
  build:
    runs-on: Pipeline-Demo-Windows";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PoolMultipleInstancesWithDemandsTest()
        {
            //Arrange
            string input = @"
pool: 'Pipeline-Demo-Windows'

stages:
- stage: Build
  jobs: 
    - job: BuildSpark
      pool:
        name: 'Pipeline-Demo-Windows'
        demands:
        - Agent.OS -equals Windows_NT
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
jobs:
  Build_Stage_BuildSpark:
    runs-on: Pipeline-Demo-Windows";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PoolInStageTest()
        {
            //Arrange
            string input = @"
stages:
  - stage: Build
    displayName: Build
    pool:
      vmImage: 'ubuntu-latest'
      demands: npm
    jobs:
      - job: BuildApi
        displayName: Build API
        steps:
          - script: npm ci --cache $(NPM_CACHE_FOLDER)
            displayName: 'Install npm dependencies'
      - job: BuildApi2
        displayName: Build API
        pool:
          vmImage: 'windows-latest'
        steps:
          - script: npm ci --cache $(NPM_CACHE_FOLDER)
            displayName: 'Install npm dependencies'
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
jobs:
  Build_Stage_BuildApi:
    name: Build API
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Install npm dependencies
      run: npm ci --cache ${{ env.NPM_CACHE_FOLDER }}
  Build_Stage_BuildApi2:
    name: Build API
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - name: Install npm dependencies
      run: npm ci --cache ${{ env.NPM_CACHE_FOLDER }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

    }
}