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
            Conversion conversion = new Conversion();
            string yaml = "name: test ci pipelines";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = "name: test ci pipelines";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(yaml, gitHubOutput.pipelinesYaml);
        }

        [TestMethod]
        public void PipelineInvalidStringTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = "     ";

            //Act
            ConversionResponse gitHubOutput = null;
            try
            {
                gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);
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
            Conversion conversion = new Conversion();
            string yaml = null;

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = "";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PipelineGarbageStringTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = "gdagfds";

            //Act
            ConversionResponse gitHubOutput = null;
            try
            {
                gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);
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
            Conversion conversion = new Conversion();
            string yaml = "pool: windows-latest";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  build:
    runs-on: windows-latest
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PipelineWithInvalidStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
pool: 'windows-latest'
steps:
- task2: CmdLine@2 #This is purposely an invalid step
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - run: 'DeserializeYaml<AzurePipelines.Step[]>(stepsYaml) swallowed an exception: (Line: 2, Col: 3, Idx: 5) - (Line: 2, Col: 3, Idx: 5): Exception during deserialization'
";
#if Linux
            expected = @"
jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - run: 'DeserializeYaml<AzurePipelines.Step[]>(stepsYaml) swallowed an exception: (Line: 2, Col: 3, Idx: 4) - (Line: 2, Col: 3, Idx: 4): Exception during deserialization'
";    
#endif
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}