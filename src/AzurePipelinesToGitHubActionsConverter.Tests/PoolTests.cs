using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
        public void PoolUbuntuVMImageLatestStringTest()
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
        public void PoolUbuntuNameLatestStringTest()
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
        public void PoolWindowsVMImageLatestStringTest()
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
        public void PoolWindowsStringLatestStringTest()
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
        public void PoolNameDemandsStringTest()
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
        public void PoolNameDemandsListStringTest()
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
        public void PoolSimpleNameTest()
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

    }
}