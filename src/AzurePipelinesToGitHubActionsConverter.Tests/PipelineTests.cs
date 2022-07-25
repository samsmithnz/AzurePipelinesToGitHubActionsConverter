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
    - run: ""This step is unknown and caused an exception: Property 'task2' not found on type 'AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines.Step'.""
";
            //When this test runs on a Linux runner, the YAML converter returns a slightly different result
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void LineNumberPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
  - stage: Test
    jobs:
      - job: Code_Coverage
        displayName: 'Publish Code Coverage'
        pool:
          vmImage: 'ubuntu 16.04'
        steps:
          - task: PublishCodeCoverageResults@1
            displayName: 'Publish Azure Code Coverage'
            inputs:
              codeCoverageTool: 'JaCoCo'
          - task: Gulp@1
          - task: PublishCodeCoverageResults@1
            displayName: 'Publish Azure Code Coverage'
            inputs:
              codeCoverageTool: 'JaCoCo'
          - task: Gulp@1
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            //#Conversion messages (2):
            //#(line 18) Error: the step 'Gulp@1' does not have a conversion path yet
            //#(line 10) Error: the step 'PublishCodeCoverageResults@1' does not have a conversion path yet
            string expected = @"
#Error (line 11): the step 'PublishCodeCoverageResults@1' does not have a conversion path yet
#Error (line 19): the step 'Gulp@1' does not have a conversion path yet
#Error (line 23): the step 'PublishCodeCoverageResults@1' does not have a conversion path yet
#Error (line 31): the step 'Gulp@1' does not have a conversion path yet
jobs:
  Test_Stage_Code_Coverage:
    name: Publish Code Coverage
    runs-on: ubuntu 16.04
    steps:
    - uses: actions/checkout@v2
    - # ""Error: the step 'PublishCodeCoverageResults@1' does not have a conversion path yet""
      name: Publish Azure Code Coverage
      run: |
        echo ""Error: the step 'PublishCodeCoverageResults@1' does not have a conversion path yet""
        #task: PublishCodeCoverageResults@1
        #displayName: Publish Azure Code Coverage
        #inputs:
        #  codecoveragetool: JaCoCo
    - # ""Error: the step 'Gulp@1' does not have a conversion path yet""
      run: |
        echo ""Error: the step 'Gulp@1' does not have a conversion path yet""
        #task: Gulp@1
    - # ""Error: the step 'PublishCodeCoverageResults@1' does not have a conversion path yet""
      name: Publish Azure Code Coverage
      run: |
        echo ""Error: the step 'PublishCodeCoverageResults@1' does not have a conversion path yet""
        #task: PublishCodeCoverageResults@1
        #displayName: Publish Azure Code Coverage
        #inputs:
        #  codecoveragetool: JaCoCo
    - # ""Error: the step 'Gulp@1' does not have a conversion path yet""
      run: |
        echo ""Error: the step 'Gulp@1' does not have a conversion path yet""
        #task: Gulp@1
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

    }
}