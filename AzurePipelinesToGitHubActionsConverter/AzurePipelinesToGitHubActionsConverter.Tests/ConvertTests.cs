using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class ConversionTest
    {
        [TestMethod]
        public void NameTest()
        {
            //Arrange
            string input = "name: test ci pipelines";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "name: test ci pipelines";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void InvalidStringTest()
        {
            //Arrange
            string input = "     ";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void NullStringTest()
        {
            //Arrange
            string input = null;
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PoolUbuntuLatestStringTest()
        {
            //Arrange
            string input = "pool:" + Environment.NewLine +
                           "  vmImage: ubuntu-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "jobs:" + Environment.NewLine +
                                    "  build:" + Environment.NewLine +
                                    "    runs-on: ubuntu-latest";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PoolWindowsLatestStringTest()
        {
            //Arrange
            string input = "pool: " + Environment.NewLine +
                           "  vmImage: windows-latest";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = "jobs:" + Environment.NewLine +
                                    "  build:" + Environment.NewLine +
                                    "    runs-on: windows-latest";
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void VariablesTest()
        {
            //Arrange
            string input = @"
trigger:
- master
variables:
  buildConfiguration: Release
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: ubuntu-latest
  variables:
    myJobVariable: 'data'
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build $(myJobVariable)";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
env:
  buildConfiguration: Release
jobs:
  Build:
    name: Build job
    runs-on: ubuntu-latest
    env:
      myJobVariable: data
    steps:
    - uses: actions/checkout@v1
    - name: dotnet build ${{ env.myJobVariable }}
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void VariablesWithSpacesTest()
        {
            //Arrange
            string input = @"
trigger:
- master
variables:
  buildConfiguration: Release
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: ubuntu-latest
  variables:
    myJobVariable: 'data'
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $buildConfiguration
    displayName: dotnet build $myJobVariable
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
env:
  buildConfiguration: Release
jobs:
  Build:
    name: Build job
    runs-on: ubuntu-latest
    env:
      myJobVariable: data
    steps:
    - uses: actions/checkout@v1
    - name: dotnet build ${{ env.myJobVariable }}
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void GitHubActionYamlToObjectTest()
        {
            //Arrange
            string yaml = "name: CI" + Environment.NewLine +
                        "on:" + Environment.NewLine +
                        "  push:" + Environment.NewLine +
                        "    branches:" + Environment.NewLine +
                        "    - master" + Environment.NewLine +
                        "jobs:" + Environment.NewLine +
                        "  build:" + Environment.NewLine +
                        "    runs_on: ubuntu-latest" + Environment.NewLine +
                        "    steps:" + Environment.NewLine +
                        "    - uses: actions/checkout @v1" + Environment.NewLine +
                        "    - name: Build with dotnet" + Environment.NewLine +
                        "      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release" + Environment.NewLine +
                        "    - name: Publish with dotnet" + Environment.NewLine +
                        "      run: dotnet publish WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration Release" + Environment.NewLine +
                        "    - name: publish build artifacts back to GitHub" + Environment.NewLine +
                        "      uses: actions/upload-artifact @master" + Environment.NewLine +
                        "      with:" + Environment.NewLine +
                        "        name: serviceapp" + Environment.NewLine +
                        "        path: WebApplication1/WebApplication1.Service/bin/Release/netcoreapp3.0/publish";

            //Act
            GitHubActionsRoot yamlObject = GitHubActionsSerialization.Deserialize(yaml);

            //Assert
            Assert.IsTrue(yamlObject != null);
        }

        //        [TestMethod]
        //        public void TestStagesWithAzurePipelineYamlToObject()
        //        {
        //            //stages:
        //            //- stage: Build
        //            //  displayName: 'Build/Test Stage'
        //            //  jobs:
        //            //  - job: Build
        //            //    displayName: 'Build job'
        //            //    pool:
        //            //      vmImage: $(vmImage)
        //            //    steps:

        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            string yaml =
        //@"name: test dotnet build with stages 
        //trigger: 
        //- master 
        //variables: 
        //  buildConfiguration: Release 
        //  randomVariable: 14 
        //stages: 
        //- stage: Build 
        //  displayName: Build/Test Stage 
        //  jobs: 
        //  - job: Build 
        //    displayName: Build job 
        //    pool: 
        //      vmImage: $(vmImage) 
        //    steps: 
        //    - script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj 
        //      displayName: dotnet build $(buildConfiguration)";

        //            //Act
        //            GitHubConversion gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

        //            //Assert
        //            Assert.IsTrue(output != null);
        //        }

        [TestMethod]
        public void TestJobsWithAzurePipelineYamlToObject()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master
variables:
  buildConfiguration: Release
  vmImage: ubuntu-latest
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: ubuntu-latest
  timeoutInMinutes: 23
  variables:
    buildConfiguration: Debug
    myJobVariable: 'data'
    myJobVariable2: 'data2'
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build part 1
- job: Build2
  displayName: Build job
  dependsOn: Build
  pool: 
    vmImage: ubuntu-latest
  variables:
    myJobVariable: 'data'
  steps:
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build part 2
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build part 3";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
env:
  buildConfiguration: Release
  vmImage: ubuntu-latest
jobs:
  Build:
    name: Build job
    runs-on: ubuntu-latest
    timeout-minutes: 23
    env:
      buildConfiguration: Debug
      myJobVariable: data
      myJobVariable2: data2
    steps:
    - uses: actions/checkout@v1
    - name: dotnet build part 1
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
  Build2:
    name: Build job
    runs-on: ubuntu-latest
    needs: Build
    env:
      myJobVariable: data
    steps:
    - uses: actions/checkout@v1
    - name: dotnet build part 2
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
    - name: dotnet build part 3
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}