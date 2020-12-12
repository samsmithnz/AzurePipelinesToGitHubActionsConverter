using AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
        public void SuccessOrFailureTest()
        {
            string condition = "succeededOrFailed()";

            //Act
            string result = ConditionsProcessing.TranslateConditions(condition);

            //Assert
            string expected = "ne(${{ job.status }}, 'cancelled')";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void FailureConditionTest()
        {
            //Arrange
            string condition = "failed()";

            //Act
            string result = ConditionsProcessing.TranslateConditions(condition);

            //Assert
            string expected = "failure()";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AlwaysConditionTest()
        {
            //Arrange
            string condition = "always()";

            //Act
            string result = ConditionsProcessing.TranslateConditions(condition);

            //Assert
            string expected = "always()";
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
        public void MultilineConditionTest()
        {
            //Arrange
            string text = @"
and(
succeeded(),
or(
    eq(variables['Build.SourceBranch'], 'refs/heads/master'),
    startsWith(variables['Build.SourceBranch'], 'refs/tags/')
),
contains(variables['System.TeamFoundationCollectionUri'], 'dsccommunity')
)";

            //Act
            string result = ConditionsProcessing.TranslateConditions(text);

            //Assert
            string expected = "and(success(),or(eq(github.ref, 'refs/heads/master'),startsWith(github.ref, 'refs/tags/')),contains(variables['System.TeamFoundationCollectionUri'], 'dsccommunity'))";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AndOrConditionTest()
        {
            //Arrange
            string text = @"
and(succeeded(),or(succeeded(),succeeded()))";

            //Act
            string result = ConditionsProcessing.TranslateConditions(text);

            //Assert
            string expected = "and(success(),or(success(),success()))";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AndUpperCaseOrConditionTest()
        {
            //Arrange
            string text = @"
and(succeeded(),OR(succeeded(),succeeded()))";

            //Act
            string result = ConditionsProcessing.TranslateConditions(text);

            //Assert
            string expected = "and(success(),OR(success(),success()))";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void StartsWithBranchTest()
        {
            //Arrange
            string text = @"
startsWith(variables['Build.SourceBranch'], 'refs/tags/')
";

            //Act
            string result = ConditionsProcessing.TranslateConditions(text);

            //Assert
            string expected = "startsWith(github.ref, 'refs/tags/')";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AndEqualAndNotEqualComplexTest()
        {
            //Arrange
            string text = @"
and(succeeded(), eq(variables['Build.Reason'], 'PullRequest'), ne(variables['System.PullRequest.PullRequestId'], 'Null'))
";

            //Act
            string result = ConditionsProcessing.TranslateConditions(text);

            //Assert
            string expected = "and(success(),eq(variables['Build.Reason'], 'PullRequest'),ne(variables['System.PullRequest.PullRequestId'], 'Null'))";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void NestedBracketsStringTest()
        {
            //Arrange
            string text = "(this is a (sample) string with (some special words. (another one)))";

            //Act
            List<string> results = ConditionsProcessing.FindBracketedContentsInString(text);

            //Assert
            Assert.AreNotEqual(null, results);
            Assert.AreEqual(4, results.Count);
        }

        [TestMethod]
        public void NestedBracketsString2Test()
        {
            //Arrange
            string text = "not(contains('ABCDE', 'BCD'))";

            //Act
            List<string> results = ConditionsProcessing.FindBracketedContentsInString(text);

            //Assert
            Assert.AreNotEqual(null, results);
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("'ABCDE', 'BCD'", results[0]);
            Assert.AreEqual("contains('ABCDE', 'BCD')", results[1]);
        }

        [TestMethod]
        public void MultilineBracketsTest()
        {
            //Arrange
            string text = @"
and(
succeeded(),
or(
    eq(variables['Build.SourceBranch'], 'refs/heads/master'),
    startsWith(variables['Build.SourceBranch'], 'refs/tags/')
),
contains(variables['System.TeamFoundationCollectionUri'], 'dsccommunity')
)";

            //Act
            List<string> results = ConditionsProcessing.FindBracketedContentsInString(text);

            //Assert
            Assert.AreNotEqual(null, results);
            Assert.AreEqual(6, results.Count);
        }

        [TestMethod]
        public void SimpleTwoSplitTest()
        {
            //Arrange
            string condition = "'ABCDE', 'BCD'";

            //Act
            List<string> results = ConditionsProcessing.SplitContents(condition);

            //Assert
            Assert.AreEqual(2, results.Count);

        }

        [TestMethod]
        public void TrickSingleSplitWithBracketsTest()
        {
            //Arrange
            string condition = "contains('ABCDE', 'BCD')";

            //Act
            List<string> results = ConditionsProcessing.SplitContents(condition);

            //Assert
            Assert.AreEqual(1, results.Count);
        }

        [TestMethod]
        public void ComplexDoubleNestedBracketsSplitTest()
        {
            //Arrange
            string condition = "('ABCDE', 'BCD'), ne(0, 1)";

            //Act
            List<string> results = ConditionsProcessing.SplitContents(condition);

            //Assert
            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void SimpleThreeSplitTest()
        {
            //Arrange
            string condition = "succeeded(), variables['Build.SourceBranch'], 'refs/heads/master'";

            //Act
            List<string> results = ConditionsProcessing.SplitContents(condition);

            //Assert
            Assert.AreEqual(3, results.Count);
        }


        [TestMethod]
        public void ComplexDoubleSplitWithNesterBracketTest()
        {
            //Arrange
            string text = @"succeeded1(),or(succeeded2(),succeeded3())";

            //Act
            List<string> results = ConditionsProcessing.SplitContents(text);

            //Assert
            Assert.AreEqual(2, results.Count);
        }

        [TestMethod]
        public void ComplexTripleSplitWithDoubleNestedBracketsTest()
        {
            //Arrange
            string text = @"succeeded(),eq('ABCDE', 'BCD'), ne(0, 1)";

            //Act
            List<string> results = ConditionsProcessing.SplitContents(text);

            //Assert
            Assert.AreEqual(3, results.Count);
        }

    }
}
