using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
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
- master
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
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
- master
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
    - master
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
    include: [ master ] 
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


//        [TestMethod]
//        public void TriggerConversionSimpleStringTest()
//        {
//            //Arrange
//            string input = @"
//trigger:
//- master
//";
//            Conversion conversion = new Conversion();

//            //Act
//            string result = conversion.ProcessYAMLTest(input);

//            //Assert
//            string expected = @"
//trigger:
//- master
//";
//            //            string expected = @"
//            //trigger:
//            //  branches:
//            //    include:
//            //    - master
//            //";
//            Assert.AreEqual(expected.Trim(), result.Trim());
//        }

//        [TestMethod]
//        public void TriggerConversionComplexStringTest()
//        {
//            //Arrange
//            string input = @"
//trigger:
//  batch: true
//  branches:
//    include:
//    - features/*
//    exclude:
//    - features/experimental/*
//  paths:
//    include:
//    - README.md
//  tags:
//    include:
//    - v1             
//    - v1.*";
//            Conversion conversion = new Conversion();

//            //Act
//            string result = conversion.ProcessYAMLTest(input);

//            //Assert
//            string expected = @"
//trigger:
//  batch: true
//  branches:
//    include:
//    - features/*
//    exclude:
//    - features/experimental/*
//  paths:
//    include:
//    - README.md
//  tags:
//    include:
//    - v1             
//    - v1.*"; 
//            Assert.AreEqual(expected.Trim(), result.Trim());

//        }

//        [TestMethod]
//        public void TriggerConversionComplex2StringTest()
//        {
//            //Arrange
//            string input = @"
//trigger:
//- master

//pool:
//  vmImage: 'ubuntu-latest'

//steps:
//- task: UseRubyVersion@0
//  inputs:
//    versionSpec: '>= 2.5'
//- script: ruby HelloWorld.rb
//";
//            Conversion conversion = new Conversion();

//            //Act
//            Dictionary<string, string> results = conversion.GetYamlElements(input);

//            //Assert
//            string expected = @"
//trigger:
//- master

//pool:
//  vmImage: 'ubuntu-latest'

//steps:
//- task: UseRubyVersion@0
//  inputs:
//    versionSpec: '>= 2.5'
//- script: ruby HelloWorld.rb
//"; 
//            //Assert.AreEqual(expected.Trim(), result.Trim());

//        }
    }
}