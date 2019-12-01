using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class GlobalTest
    {
        [TestMethod]
        public void GetGlobalHeaderTest()
        {
            //Arrange
            string expected = "# converted to GitHub Actions by https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter on " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:sstt");

            //Act
            string result = Global.GetHeaderComment();

            //Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetLineCommentTest()
        {
            //Arrange

            //Act
            string result = Global.GetLineComment();

            //Assert
            Assert.IsTrue(string.IsNullOrEmpty(result) == false);
        }

        [TestMethod]
        public void GenerateSpacesTest()
        {
            //Arrange
            int number0 = 0;
            int number1 = 1;
            int number4 = 4;
            int number9 = 9;

            //Act
            string results0 = Global.GenerateSpaces(number0);
            string results1 = Global.GenerateSpaces(number1);
            string results4 = Global.GenerateSpaces(number4);
            string results9 = Global.GenerateSpaces(number9);

            //Assert
            Assert.AreEqual( "", results0);
            Assert.AreEqual( " ", results1);
            Assert.AreEqual( "    ", results4);
            Assert.AreEqual( "         ", results9);
        }

    }
}


//# ASP.NET Core
//# Build and test ASP.NET Core projects targeting .NET Core.
//# Add steps that run tests, create a NuGet package, deploy, and more:
//# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core

//trigger:
//- master

//pool:
//  vmImage: 'ubuntu-latest'

//variables:
//  buildConfiguration: 'Release'

//steps:
//- script: dotnet build --configuration $(buildConfiguration)
//  displayName: 'dotnet build $(buildConfiguration)'
