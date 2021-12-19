using AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void GenerateSpacesTest()
        {
            //Arrange
            int number0 = 0;
            int number1 = 1;
            int number4 = 4;
            int number9 = 9;

            //Act
            string results0 = ConversionUtility.GenerateSpaces(number0);
            string results1 = ConversionUtility.GenerateSpaces(number1);
            string results4 = ConversionUtility.GenerateSpaces(number4);
            string results9 = ConversionUtility.GenerateSpaces(number9);

            //Assert
            Assert.AreEqual("", results0);
            Assert.AreEqual(" ", results1);
            Assert.AreEqual("    ", results4);
            Assert.AreEqual("         ", results9);
        }

        public static string TrimNewLines(string input)
        {
            //Trim off any leading or trailing new lines 
            input = input.TrimStart('\r', '\n');
            input = input.TrimEnd('\r', '\n');

            return input;
        }

        public static string DebugNewLineCharacters(string input)
        {
            input = input.Replace("\r", "xxx");
            input = input.Replace("\n", "000");
            return input;
        }

    }
}