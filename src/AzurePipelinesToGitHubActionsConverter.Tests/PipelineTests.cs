using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class PipelineTests
    {
        [TestMethod]
        public void PipelineNameTest()
        {
            //Arrange
            string input = "name: test ci pipelines";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "name: test ci pipelines";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(input, gitHubOutput.pipelinesYaml);
        }

        [TestMethod]
        public void PipelineInvalidStringTest()
        {
            //Arrange
            string input = "     ";
            Conversion conversion = new Conversion();


            //Act
            ConversionResponse gitHubOutput = null;
            try
            {
                gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("This appears to be invalid YAML", ex.Message);
                Assert.AreEqual(null, gitHubOutput);
            }
        }

        [TestMethod]
        public void PipelineNullStringTest()
        {
            //Arrange
            string input = null;
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PipelineGarbageStringTest()
        {
            //Arrange
            string input = "gdagfds";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = null;
            try
            {
                gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);
            }
            catch (Exception ex)
            {
                //Assert
                Assert.AreEqual("This appears to be invalid YAML", ex.Message);
                Assert.AreEqual(null, gitHubOutput);
            }

        }

        [TestMethod]
        public void PipelineSimpleStringTest()
        {
            //Arrange
            string input = "pool: windows-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
jobs:
  build:
    runs-on: windows-latest
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}