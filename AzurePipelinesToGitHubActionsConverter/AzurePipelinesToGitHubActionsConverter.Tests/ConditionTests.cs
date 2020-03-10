using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string condition = "succeeded()";

            //Act
            string result = ConditionsProcessing.GenerateConditions(condition);

            //Assert
            string expected = "success()";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ContainsTest()
        {
            //Arrange
            string condition = "contains('ABCDE', 'BCD')";

            //Act
            string result = ConditionsProcessing.GenerateConditions(condition);

            //Assert
            string expected = "contains('ABCDE', 'BCD')";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NotAndContainsTest()
        {
            //Arrange
            string condition = "not(contains('ABCDE', 'BCD'))";

            //Act
            string result = ConditionsProcessing.GenerateConditions(condition);

            //Assert
            string expected = "not(contains('ABCDE', 'BCD'))";
            Assert.AreEqual(expected, result);
        }   
        
        [TestMethod]
        public void EqualsTest()
        {
            //Arrange
            string condition = "eq('ABCDE', 'BCD')";

            //Act
            string result = ConditionsProcessing.GenerateConditions(condition);

            //Assert
            string expected = "eq('ABCDE', 'BCD')";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AndEqualsNotEqualsTest()
        {
            //Arrange
            string condition = "and(eq('ABCDE', 'BCD'), ne(0, 1))";

            //Act
            string result = ConditionsProcessing.GenerateConditions(condition);

            //Assert
            string expected = "and(eq('ABCDE', 'BCD'),ne(0, 1))";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NestedStringTest()
        {
            //Arrange
            string text = "(this is a (sample) string with (some special words. (another one)))";

            //Act
            List<string> results = ConditionsProcessing.FindBracketedContentsInString(text);

            //Assert
            Assert.IsTrue(results != null);
            Assert.IsTrue(results.Count == 4);
        }   
        
        [TestMethod]
        public void NestedString2Test()
        {
            //Arrange
            string text = "not(contains('ABCDE', 'BCD'))";

            //Act
            List<string> results = ConditionsProcessing.FindBracketedContentsInString(text);

            //Assert
            Assert.IsTrue(results != null);
            Assert.IsTrue(results.Count == 2);
            Assert.IsTrue(results[0] == "'ABCDE', 'BCD'");
            Assert.IsTrue(results[1]== "contains('ABCDE', 'BCD')");
        }



    }
}
