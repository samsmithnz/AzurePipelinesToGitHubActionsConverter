using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class PoolTests
    {
        [TestMethod]
        public void PoolUbuntuLatestStringTest()
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
        public void PoolWindowsLatestStringTest()
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
        public void PoolNameDependsStringTest()
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

//        [TestMethod]
//        public void PoolNameDependsListStringTest()
//        {
//            //Arrange
//            string input = @"
//pool:
//  name: Hosted VS2017
//  demands: 
//  - npm";
//            Conversion conversion = new Conversion();

//            //Act
//            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

//            //Assert
//            string expected = @"
//#Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet
//jobs:
//  build:
//    runs-on: Hosted VS2017";
//            expected = UtilityTests.TrimNewLines(expected);
//            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
//        }

    }
}