using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class TriggerTests
    {
        [TestMethod]
        public void TriggerSimpleStringTest()
        {
            //Arrange
            string input = @"
trigger:
- main
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - main
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }


        [TestMethod]
        public void TriggerAndPRNoneSimpleStringTest()
        {
            //Arrange
            string input = @"
trigger: none
pr: none
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - none
  pull-request:
    branches:
    - none
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void TriggerSimpleWithMultipleBranchesStringTest()
        {
            //Arrange
            string input = @"
trigger:
- main
- develop
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - main
    - develop
";
            expected = UtilityTests.TrimNewLines(expected);
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - features/*
    paths:
    - README.md
    tags:
    - v1
    - v1.*
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PRComplexWithPRStringTest()
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
    - v1.*
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"on:
  pull-request:
    branches:
    - features/*
    paths:
    - README.md
    tags:
    - v1
    - v1.*
";
            expected = UtilityTests.TrimNewLines(expected);
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
    - v1.*
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"on:
  push:
    branches-ignore:
    - features/experimental/*
    paths-ignore:
    - README.md
    tags-ignore:
    - v1
    - v1.*
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void PRTriggerComplexPRWithIgnoresStringTest()
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
    - v1.*
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  pull-request:
    branches-ignore:
    - features/experimental/*
    paths-ignore:
    - README.md
    tags-ignore:
    - v1
    - v1.*
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void TriggerComplexMasterAndPRWithIgnoresStringTest()
        {
            //Arrange
            string input = @"
trigger:
  batch: true
  autoCancel: true
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
    - v1.*
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches-ignore:
    - features/experimental/*
    paths-ignore:
    - README.md
  pull-request:
    branches-ignore:
    - features/experimental/*
    paths-ignore:
    - README.md
    tags-ignore:
    - v1
    - v1.*
";
            expected = UtilityTests.TrimNewLines(expected);
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
        public void SchedulesCronRemoveExtraStringTest()
        {
            //Arrange
            string input = @"
schedules:
- cron: '0 0 * * *'
- cron: '0 2 * * *'
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

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
    include: [ main ] 
    exclude: 
    - 'features/experimental/*'
  always: true";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  schedule:
  - cron: '0 0 * **'
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }



        [TestMethod]
        public void OnPushAndScheduleCronTriggerTest()
        {
            //Arrange
            string input = @"
trigger:
- main
schedules:
- cron: '0 0 3/4 ? * * *'
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - main
  schedule:
  - cron: '0 0 3/4 ? * * *'
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void TriggerWorkflowDispatchTest()
        {
            //Arrange
            string input = @"
trigger:
- main
";
            bool addWorkFlowDispatch = true;
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input, addWorkFlowDispatch);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - main
  workflow_dispatch:
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void TriggerWorkflowDispatchNoTriggerTest()
        {
            //Arrange
            string input = @"
name: test trigger pipelines
";
            bool addWorkFlowDispatch = true;
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input, addWorkFlowDispatch);

            //Assert
            string expected = @"
name: test trigger pipelines
on:
  workflow_dispatch:
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}