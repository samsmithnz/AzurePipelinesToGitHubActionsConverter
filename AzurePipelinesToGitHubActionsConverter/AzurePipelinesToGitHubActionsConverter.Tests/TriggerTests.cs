using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class TriggerTests
    {
        [TestMethod]
        public void TestTriggerSimpleString()
        {
            //Arrange
            string input = "trigger:" + Environment.NewLine +
                           "- master";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "on:" + Environment.NewLine +
                                    "  push:" + Environment.NewLine +
                                    "    branches:" + Environment.NewLine +
                                    "    - master";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TestTriggerSimpleWithMultipleBranchesString()
        {
            //Arrange
            string input = "trigger:" + Environment.NewLine +
                            "- master" + Environment.NewLine +
                            "- develop";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "on:" + Environment.NewLine +
                                    "  push:" + Environment.NewLine +
                                    "    branches:" + Environment.NewLine +
                                    "    - master" + Environment.NewLine +
                                    "    - develop";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TestTriggerComplexString()
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
    - v1.*";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "on:" + Environment.NewLine +
                        "  push:" + Environment.NewLine +
                        "    branches:" + Environment.NewLine +
                        "    - features/*" + Environment.NewLine +
                        "    paths:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "    tags:" + Environment.NewLine +
                        "    - v1" + Environment.NewLine +
                        "    - v1.*";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TestTriggerComplexWithPRString()
        {
            //Arrange
            string input = @"
pr:
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
    - v1.*";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "on:" + Environment.NewLine +
                        "  pull-request:" + Environment.NewLine +
                        "    branches:" + Environment.NewLine +
                        "    - features/*" + Environment.NewLine +
                        "    paths:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "    tags:" + Environment.NewLine +
                        "    - v1" + Environment.NewLine +
                        "    - v1.*";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TestTriggerComplexWithIgnoresString()
        {
            //Arrange
            string input = @"
trigger:
  batch: true
  branches:
    exclude:
    - features/experimental/*
  paths:
    exclude:
    - README.md
  tags:
    exclude:
    - v1             
    - v1.*";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "on:" + Environment.NewLine +
                        "  push:" + Environment.NewLine +
                        "    branches-ignore:" + Environment.NewLine +
                        "    - features/experimental/*" + Environment.NewLine +
                        "    paths-ignore:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "    tags-ignore:" + Environment.NewLine +
                        "    - v1" + Environment.NewLine +
                        "    - v1.*";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TestTriggerComplexPRWithIgnoresString()
        {
            //Arrange
            string input = @"
pr:
  batch: true
  branches:
    exclude:
    - features/experimental/*
  paths:
    exclude:
    - README.md
  tags:
    exclude:
    - v1             
    - v1.*";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "on:" + Environment.NewLine +
                        "  pull-request:" + Environment.NewLine +
                        "    branches-ignore:" + Environment.NewLine +
                        "    - features/experimental/*" + Environment.NewLine +
                        "    paths-ignore:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "    tags-ignore:" + Environment.NewLine +
                        "    - v1" + Environment.NewLine +
                        "    - v1.*";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TestTriggerComplexMasterAndPRWithIgnoresString()
        {
            //Arrange
            string input = @"
trigger:
  batch: true
  branches:
    exclude:
    - features/experimental/*
  paths:
    exclude:
    - README.md
pr:
  batch: true
  branches:
    exclude:
    - features/experimental/*
  paths:
    exclude:
    - README.md
  tags:
    exclude:
    - v1             
    - v1.*";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "on:" + Environment.NewLine +
                        "  push:" + Environment.NewLine +
                        "    branches-ignore:" + Environment.NewLine +
                        "    - features/experimental/*" + Environment.NewLine +
                        "    paths-ignore:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "  pull-request:" + Environment.NewLine +
                        "    branches-ignore:" + Environment.NewLine +
                        "    - features/experimental/*" + Environment.NewLine +
                        "    paths-ignore:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "    tags-ignore:" + Environment.NewLine +
                        "    - v1" + Environment.NewLine +
                        "    - v1.*";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}