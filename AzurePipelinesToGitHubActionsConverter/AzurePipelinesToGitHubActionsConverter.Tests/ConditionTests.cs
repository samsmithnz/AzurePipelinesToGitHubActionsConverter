using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            string result = ConditionsProcessing.TranslateConditions(condition);

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
            string result = ConditionsProcessing.TranslateConditions(condition);

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
            string result = ConditionsProcessing.TranslateConditions(condition);

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
            string result = ConditionsProcessing.TranslateConditions(condition);

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
            string result = ConditionsProcessing.TranslateConditions(condition);

            //Assert
            string expected = "and(eq('ABCDE', 'BCD'),ne(0, 1))";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void BranchVariableTest()
        {
            //Arrange
            string condition = "and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/master'))";

            //Act
            string result = ConditionsProcessing.TranslateConditions(condition);

            //Assert
            string expected = "and(success(),eq(github.ref, 'refs/heads/master'))";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void BranchNameVariableTest()
        {
            //Arrange
            string condition = "and(succeeded(), eq(variables['Build.SourceBranchName'], 'master'))";

            //Act
            string result = ConditionsProcessing.TranslateConditions(condition);

            //Assert
            string expected = "and(success(),endsWith(github.ref, 'master'))";
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
