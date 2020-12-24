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

            string expectedLinux = @"
jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - run: 'DeserializeYaml<AzurePipelines.Step[]>(stepsYaml) swallowed an exception: (Line: 2, Col: 3, Idx: 4) - (Line: 2, Col: 3, Idx: 4): Exception during deserialization'
";

            //When this test runs on a Linux runner, the YAML converter returns a slightly different result
            expected = UtilityTests.TrimNewLines(expected);
            if (expected == gitHubOutput.actionsYaml)
            {
                Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            }
            else
            {
                expectedLinux = UtilityTests.TrimNewLines(expectedLinux);
                Assert.AreEqual(expectedLinux, gitHubOutput.actionsYaml);
            }
        }

//        [TestMethod]
//        public void LineNumberPipelineTest()
//        {
//            //Arrange
//            Conversion conversion = new Conversion();
//            string yaml = @"
//stages:
//  - stage: Test
//    jobs:
//      - job: Code_Coverage
//        displayName: 'Publish Code Coverage'
//        pool:
//          vmImage: 'ubuntu 16.04'
//        steps:
//          - task: PublishCodeCoverageResults@1
//            displayName: 'Publish Azure Code Coverage'
//            inputs:
//              codeCoverageTool: 'JaCoCo'
//              summaryFileLocation: '$(buildFolderName)/$(testResultFolderName)/JaCoCo_coverage.xml'
//              pathToSources: '$(Build.SourcesDirectory)/$(buildFolderName)/$(dscBuildVariable.RepositoryName)'
//";

//            //Act
//            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

//            //Assert
//            string expected = @"
//#(Line 8) Note: Error! This step does not have a conversion path yet: PublishCodeCoverageResults@1
//jobs:
//  Test_Stage_Code_Coverage:
//    name: Publish Code Coverage
//    runs-on: ubuntu 16.04
//    steps:
//    - uses: actions/checkout@v2
//    - # 'Note: Error! This step does not have a conversion path yet: PublishCodeCoverageResults@1'
//      name: Publish Azure Code Coverage
//      run: 'echo ""Note: Error! This step does not have a conversion path yet: PublishCodeCoverageResults@1 #task: PublishCodeCoverageResults@1#displayName: Publish Azure Code Coverage#inputs:#  codecoveragetool: JaCoCo#  summaryfilelocation: ${{ env.buildFolderName }}/${{ env.testResultFolderName }}/JaCoCo_coverage.xml#  pathtosources: ${{ github.workspace }}/${{ env.buildFolderName }}/${{ env.dscBuildVariable.RepositoryName }}""'";

//            expected = UtilityTests.TrimNewLines(expected);
//            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

//        }

    }
}