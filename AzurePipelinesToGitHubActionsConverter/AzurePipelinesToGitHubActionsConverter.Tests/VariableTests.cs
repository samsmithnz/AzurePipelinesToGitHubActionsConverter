using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class VariableTests
    {
        [TestMethod]
        public void SimpleVariablesTest()
        {
            //Arrange
            string input = @"
variables:
  configuration: debug
  platform: x64";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
env:
  configuration: debug
  platform: x64";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void ComplexVariablesTest()
        {
            //Arrange
            string input = @"
variables:
- name: myVariable
  value: myValue
- name: myVariable2
  value: myValue2
- group: myVariablegroup
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
env:
  myVariable: myValue
  myVariable2: myValue2";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void SimpleVariablesAndSimpleTriggerTest()
        {
            //Arrange
            string input = @"
trigger:
- master
- develop
variables:
  myVariable: myValue
  myVariable2: myValue2";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
    - develop
env:
  myVariable: myValue
  myVariable2: myValue2";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ComplexVariablesAndComplexTriggerTest()
        {
            //Arrange
            string input = @"
trigger:
  batch: true
  branches:
    include:
    - features/*
    exclude:
    - features/experimental/*
  paths:
    include:
    - README.md
  tags:
    include:
    - v1     
    - v1.*
variables:
- name: myVariable
  value: myValue
- name: myVariable2
  value: myValue2
- group: myVariablegroup
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = 
                        @"
on:
  push:
    branches:
    - features/*
    paths:
    - README.md
    tags:
    - v1
    - v1.*
env:
  myVariable: myValue
  myVariable2: myValue2";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


    }
}