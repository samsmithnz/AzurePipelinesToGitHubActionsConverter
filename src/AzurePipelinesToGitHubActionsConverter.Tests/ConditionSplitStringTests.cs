using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class ConditionSplitStringTests
    {

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
