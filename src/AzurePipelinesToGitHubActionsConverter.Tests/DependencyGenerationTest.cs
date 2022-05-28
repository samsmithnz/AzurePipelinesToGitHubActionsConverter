//using AzurePipelinesToGitHubActionsConverter.Core;
//using AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion;
//using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Text.Json;

//namespace AzurePipelinesToGitHubActionsConverter.Tests
//{
//    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
//    [TestClass]
//    public class DependencyGenerationTest
//    {
//        [TestMethod]
//        public void GenerateAllActionsTest()
//        {
//            //Arrange
//            Conversion conversion = new Conversion();
//            string yaml = @"
//jobs:
//- job: Build
//  pool:
//    vmImage: ubuntu-latest
//  steps:
//  - script: echo 'hello world'";

//            //Act
//            //convert the yaml into json, it's easier to parse
//            JsonDocument jsonDoc = null;
//            if (yaml != null)
//            {
//                //Clean up the YAML to remove conditional insert statements
//                string processedYaml = ConversionUtility.CleanYamlBeforeDeserializationV2(yaml);
//                jsonDoc = JsonSerialization.DeserializeStringToJsonDocument(processedYaml);
//            }
//            if (jsonDoc != null)
//            {
//                yaml = YamlSerialization.SerializeYaml(jsonDoc.RootElement.ToString());
//            }

//            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

//            //Assert
//            string expected = @"
//";
//            //When this test runs on a Linux runner, the YAML converter returns a slightly different result
//            expected = UtilityTests.TrimNewLines(expected);
//            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
//            expected = UtilityTests.TrimNewLines(expected);
//            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

//        }

//    }
//}
