using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

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
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "on:" + Environment.NewLine +
                                    "  push:" + Environment.NewLine +
                                    "    branches:" + Environment.NewLine +
                                    "    - master" + Environment.NewLine);
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
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "on:" + Environment.NewLine +
                                    "  push:" + Environment.NewLine +
                                    "    branches:" + Environment.NewLine +
                                    "    - master" + Environment.NewLine +
                                    "    - develop" + Environment.NewLine);
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
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "on:" + Environment.NewLine +
                        "  push:" + Environment.NewLine +
                        "    branches:" + Environment.NewLine +
                        "    - features/*" + Environment.NewLine +
                        "    paths:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "    tags:" + Environment.NewLine +
                        "    - v1" + Environment.NewLine +
                        "    - v1.*" + Environment.NewLine);
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
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "on:" + Environment.NewLine +
                        "  pull-request:" + Environment.NewLine +
                        "    branches:" + Environment.NewLine +
                        "    - features/*" + Environment.NewLine +
                        "    paths:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "    tags:" + Environment.NewLine +
                        "    - v1" + Environment.NewLine +
                        "    - v1.*" + Environment.NewLine);
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
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "on:" + Environment.NewLine +
                        "  push:" + Environment.NewLine +
                        "    branches-ignore:" + Environment.NewLine +
                        "    - features/experimental/*" + Environment.NewLine +
                        "    paths-ignore:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "    tags-ignore:" + Environment.NewLine +
                        "    - v1" + Environment.NewLine +
                        "    - v1.*" + Environment.NewLine);
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
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "on:" + Environment.NewLine +
                        "  pull-request:" + Environment.NewLine +
                        "    branches-ignore:" + Environment.NewLine +
                        "    - features/experimental/*" + Environment.NewLine +
                        "    paths-ignore:" + Environment.NewLine +
                        "    - README.md" + Environment.NewLine +
                        "    tags-ignore:" + Environment.NewLine +
                        "    - v1" + Environment.NewLine +
                        "    - v1.*" + Environment.NewLine);
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
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            Assert.AreEqual(output, "on:" + Environment.NewLine +
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
                        "    - v1.*" + Environment.NewLine);
        }

    }
}