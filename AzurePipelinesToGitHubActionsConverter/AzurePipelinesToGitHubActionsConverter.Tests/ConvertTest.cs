using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Cryptography.X509Certificates;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class ConversionTest
    {
        //[TestMethod]
        //public void TestComment()
        //{
        //    //Arrange
        //    bool showGlobalHeaderComment = false;
        //    string input = "   #";
        //    Conversion conversion = new Conversion();

        //    //Act
        //    string output = conversion.ConvertPipelineToAction(input, showGlobalHeaderComment);

        //    //Assert
        //    Assert.AreEqual(input, output);
        //}

        [TestMethod]
        public void TestPoolUbuntuLatestString()
        {
            //Arrange
            bool showGlobalHeaderComment = false;
            string input = "pool:" + Environment.NewLine +
                           "  vmImage: 'ubuntu-latest'";
            Conversion conversion = new Conversion();

            //Act
            string output = conversion.ConvertPipelineToAction(input, showGlobalHeaderComment);

            //Assert
            Assert.AreEqual(output, "runs-on: ubuntu-latest");
        }

        [TestMethod]
        public void TestPoolWindowsLatestString()
        {
            //Arrange
            bool showGlobalHeaderComment = false;
            string input = "pool: " + Environment.NewLine +
                           "  vmImage: 'windows-latest' ";
            Conversion conversion = new Conversion();

            //Act
            string output = conversion.ConvertPipelineToAction(input, showGlobalHeaderComment);

            //Assert
            Assert.AreEqual(output, "runs-on: windows-latest");
        }

        [TestMethod]
        public void TestTriggerString()
        {
            //Arrange
            bool showGlobalHeaderComment = false;
            string input = "trigger:" + Environment.NewLine +
                           "- master";
            Conversion conversion = new Conversion();

            //Act
            string output = conversion.ConvertPipelineToAction(input, showGlobalHeaderComment);

            //Assert
            Assert.AreEqual(output, "on: [push]");
        }

        [TestMethod]
        public void TestGitHubActionString()
        {
            //Arrange
            Conversion conversion = new Conversion();

            //Act
             conversion.CreateGitHubAction();

            //Assert
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestBasicDotNetPipelineString()
        {
            //Arrange
            bool showGlobalHeaderComment = false;
            string input = "" + Environment.NewLine +
                "trigger:" + Environment.NewLine +
                "- master" + Environment.NewLine +
                "    " + Environment.NewLine +
                "pool:" + Environment.NewLine +
                "  vmImage: 'ubuntu-latest'" + Environment.NewLine +
                "variables:" + Environment.NewLine +
                "  buildConfiguration: 'Release'" + Environment.NewLine +
                 "   " + Environment.NewLine +
                 "   " + Environment.NewLine +
                "steps:" + Environment.NewLine +
                "- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj" + Environment.NewLine +
                "  displayName: 'dotnet build $(buildConfiguration)'";
            Conversion conversion = new Conversion();

            //Act
            string output = conversion.ConvertPipelineToAction(input, showGlobalHeaderComment);

            //Assert
            Assert.AreEqual(input, "on: [push]");
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




//name: CI
// 
//on: [push]
// 
//jobs:
//  build:
// 
//    runs-on: ubuntu-latest
// 
//    steps:
//    # checkout the repo
//    - uses: actions/checkout @v1
// 
//    # install dependencies, build, and test
//    - name: Setup.NET Core
//      uses: actions/setup-dotnet @v1
//      with:
//        dotnet-version: 3.0.100
//    - name: Build with dotnet
//      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
//    - name: Publish with dotnet
//      run: dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release
//    - name: publish build artifacts back to GitHub
//      uses: actions/upload-artifact @master
//      with:
//        name: serviceapp
//        path: WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish




