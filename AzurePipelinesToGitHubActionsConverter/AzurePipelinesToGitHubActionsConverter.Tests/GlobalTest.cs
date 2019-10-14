using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography.X509Certificates;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class GlobalTest
    {
        [TestMethod]
        public void TestGetGlobalHeader()
        {
            //Arrange
            string expectedString = "# converted to GitHub Actions by https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter on " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:sstt");

            //Act
            string output = Global.GetHeaderComment();

            //Assert
            Assert.AreEqual(output, expectedString);
        }

        [TestMethod]
        public void TestGetLineComment()
        {
            //Arrange

            //Act
            string output = Global.GetLineComment();

            //Assert
            Assert.IsTrue(string.IsNullOrEmpty(output) == false);
        }

        [TestMethod]
        public void TestGenerateSpaces()
        {
            //Arrange
            int number0 = 0;
            int number1 = 1;
            int number4 = 4;
            int number9 = 9;

            //Act
            string output0 = Global.GenerateSpaces(number0);
            string output1 = Global.GenerateSpaces(number1);
            string output4 = Global.GenerateSpaces(number4);
            string output9 = Global.GenerateSpaces(number9);

            //Assert
            Assert.AreEqual(output0, "");
            Assert.AreEqual(output1, " ");
            Assert.AreEqual(output4, "    ");
            Assert.AreEqual(output9, "         ");
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
