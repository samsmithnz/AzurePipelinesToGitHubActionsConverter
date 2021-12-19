using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StepsUnfinishedTests
    {

        //        [TestMethod]
        //        public void InvalidStepIndividualStepTest()
        //        {
        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            string yaml = @"
        //    - task: Gulp@1
        //";

        //            //Act
        //            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

        //            //Assert
        //            string expected = @"
        //- # ""Note: Error! The GULP@1 step does not have a conversion path yet, but it's on our radar. Please consider contributing! https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/219""
        //  run: ""echo \""Note: Error! The GULP@1 step does not have a conversion path yet, but it's on our radar. Please consider contributing! https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/219 #task: Gulp@1\""""
        //";
        //            expected = UtilityTests.TrimNewLines(expected);
        //            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //        }    

    }
}