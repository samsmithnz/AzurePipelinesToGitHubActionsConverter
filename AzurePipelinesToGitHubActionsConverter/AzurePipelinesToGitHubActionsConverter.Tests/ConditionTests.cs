using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class ConditionTests
    {
        [TestMethod]
        public void SuccessConditionTest()
        {
            //Arrange
            string conditions = "succeeded()";

            //Act
            string result = ConditionsProcessing.GenerateConditions(conditions);

            //Assert
            string expected = "success()";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ContainsTest()
        {
            //Arrange
            string conditions = "contains('ABCDE', 'BCD')";

            //Act
            string result = ConditionsProcessing.GenerateConditions(conditions);

            //Assert
            string expected = "contains('ABCDE', 'BCD')";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void RegexTest()
        {
            //Arrange
            string text = "this is a (sample) string with (some special words. (another one))";
            string regexString = @"(?<=\().+?(?=\))";


            //Act
            //MatchCollection results = Regex.Matches(text, regexString);
            MatchCollection results =regex.Matches(text);

            //Assert
            Assert.IsTrue(results != null);
        }

        //public static string TrimNewLines(string input)
        //{
        //    //Trim off any leading or trailing new lines 
        //    input = input.TrimStart('\r', '\n');
        //    input = input.TrimEnd('\r', '\n');

        //    return input;
        //}

    }
}
