﻿using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    //TODO: Docker is very much in progress.
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StepsDockerTests
    {

        [TestMethod]
        public void Docker1BuildStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
  - task: Docker@1
    displayName: 'Build an image'
    inputs:
      azureSubscriptionEndpoint: '$(Azure.ServiceConnectionId)'
      azureContainerRegistry: '$(ACR.FullName)'
      imageName: '$(ACR.ImageName)'
      command: build
      dockerFile: '**/Dockerfile'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Build an image
  run: docker build . --file **/Dockerfile
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void Docker2BuildStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Docker@2
  displayName: Build
  inputs:
    command: build
    repository: contosoRepository
    tags: tag1
    arguments: --secret id=mysecret,src=mysecret.txt
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Build
  run: docker build . contosoRepository --tags tag1 --secret id=mysecret,src=mysecret.txt
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void Docker1PushStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
  - task: Docker@1
    displayName: 'Push an image'
    inputs:
      azureSubscriptionEndpoint: '$(Azure.ServiceConnectionId)'
      azureContainerRegistry: '$(ACR.FullName)'
      imageName: '$(ACR.ImageName)'
      command: push
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Push an image
  run: docker push .
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void Docker2PushStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Docker@2
  displayName: Push image
  inputs:
    containerRegistry: |
      $(dockerHub)
    repository: $(imageName)
    command: push
    tags: |
      test1
      test2
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Push image
  run: docker push . ${{ env.imageName }} --tags test1,test2
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DockerBuildAndPushStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Docker@2
  displayName: Build and Push
  inputs:
    command: buildAndPush
    containerRegistry: dockerRegistryServiceConnection1
    repository: contosoRepository
    tags: |
      tag1
      tag2
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Build and Push
  run: docker build-push . contosoRepository --tags tag1,tag2
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DockerLoginStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Docker@2
  displayName: Login to ACR
  inputs:
    command: login
    containerRegistry: dockerRegistryServiceConnection1
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Login to ACR
  run: docker login .
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DockerLogoutStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Docker@2
  displayName: Logout of ACR
  inputs:
    command: logout
    containerRegistry: dockerRegistryServiceConnection1
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Logout of ACR
  run: docker logout .
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}