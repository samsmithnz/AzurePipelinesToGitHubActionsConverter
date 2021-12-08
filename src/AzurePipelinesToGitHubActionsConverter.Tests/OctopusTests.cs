using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class OctopusTests
    {

        [TestMethod]
        public void OctopusPackTaskTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Ant@1
  inputs:
    workingDirectory: ''
    buildFile: 'build.xml'
    javaHomeOption: 'JDKVersion'
    jdkVersionOption: '1.8'
    jdkArchitectureOption: 'x64'
    publishJUnitResults: true
    testResultsFiles: '**/TEST-*.xml'  
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"

";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void OctopusPushTaskTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Ant@1
  inputs:
    workingDirectory: ''
    buildFile: 'build.xml'
    javaHomeOption: 'JDKVersion'
    jdkVersionOption: '1.8'
    jdkArchitectureOption: 'x64'
    publishJUnitResults: true
    testResultsFiles: '**/TEST-*.xml'  
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"

";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void OctopusPipelineTest()
        {           
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = "";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"

";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    

    }
}