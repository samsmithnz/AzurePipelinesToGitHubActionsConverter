using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class TriggerTests
    {
        [TestMethod]
        public void TriggerSimpleStringTest()
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
        public void TriggerSimpleWithMultipleBranchesStringTest()
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
        public void TriggerComplexStringTest()
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
        public void TriggerComplexWithPRStringTest()
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
        public void TriggerComplexWithIgnoresStringTest()
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
        public void TriggerComplexPRWithIgnoresStringTest()
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
        public void TriggerComplexMasterAndPRWithIgnoresStringTest()
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

        [TestMethod]
        public void ScheduleCronTriggerTest()
        {
            //Arrange
            string input = @"
schedules:
- cron: '0 0 3/4 ? * * *'
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  schedule:
  - cron: '0 0 3/4 ? * * *'
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ScheduleCronWithDoubleQuotesTriggerTest()
        {
            //Arrange
            string input = @"
schedules:
- cron: ""0 0 1/4 ? * * *""
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  schedule:
  - cron: '0 0 1/4 ? * * *'
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void CronRemoveExtraStringTest()
        {
            //Arrange
            string input = @"
schedules:
- cron: '0 0 * * *'
- cron: '0 2 * * *'
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  schedule:
  - cron: '0 0 * * *'
  - cron: '0 2 * * *'
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ScheduleTriggerTest()
        {
            //Arrange
            string input = @"
schedules:
- cron: '0 0 * **'
  displayName: Test schedule
  branches:
    include: [ master ] 
    exclude: 
    - 'features/experimental/*'
  always: true";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  schedule:
  - cron: '0 0 * **'
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}