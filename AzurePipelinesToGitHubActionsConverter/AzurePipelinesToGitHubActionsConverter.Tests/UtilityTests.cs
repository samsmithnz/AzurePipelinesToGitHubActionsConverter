using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
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
            string results0 = Utility.GenerateSpaces(number0);
            string results1 = Utility.GenerateSpaces(number1);
            string results4 = Utility.GenerateSpaces(number4);
            string results9 = Utility.GenerateSpaces(number9);

            //Assert
            Assert.AreEqual( "", results0);
            Assert.AreEqual( " ", results1);
            Assert.AreEqual( "    ", results4);
            Assert.AreEqual( "         ", results9);
        }

        public static string TrimNewLines(string input)
        {
            //Trim off any leading or trailing new lines 
            input = input.TrimStart('\r', '\n');
            input = input.TrimEnd('\r', '\n');

            return input;
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
