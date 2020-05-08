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
            string input = "pool:" + Environment.NewLine +
                           "  vmImage: ubuntu-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "jobs:" + Environment.NewLine +
                                    "  build:" + Environment.NewLine +
                                    "    runs-on: ubuntu-latest";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PoolWindowsLatestStringTest()
        {
            //Arrange
            string input = "pool: " + Environment.NewLine +
                           "  vmImage: windows-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "jobs:" + Environment.NewLine +
                                    "  build:" + Environment.NewLine +
                                    "    runs-on: windows-latest";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}