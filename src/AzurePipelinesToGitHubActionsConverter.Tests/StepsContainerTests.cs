using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StepsContainerTests
    {

        [TestMethod]
        public void Docker0BuildStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
  - task: Docker@0
    displayName: 'Build an image'
    inputs:
      azureSubscription: '[Azure DevOps service connection]'
      azureContainerRegistry: '{""loginServer"":""[MyContainerRegistryName].azurecr.io"", ""id"" : ""/subscriptions/[MySubscriptionGuid]/resourceGroups/[MyResourceGroupName]/providers/Microsoft.ContainerRegistry/registries/[MyContainerRegistryName]""}'
      defaultContext: false
      context: ContainerPOC
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Build an image
  run: docker build --file Dockerfile ContainerPOC
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void Docker0PushStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
  - task: Docker@0
    displayName: 'Push an image'
    inputs:
      azureSubscription: '[Azure DevOps service connection]'
      azureContainerRegistry: '{""loginServer"":""[MyContainerRegistryName].azurecr.io"", ""id"" : ""/subscriptions/[MySubscriptionGuid]/resourceGroups/[MyResourceGroupName]/providers/Microsoft.ContainerRegistry/registries/[MyContainerRegistryName]""}'
      action: 'Push an image'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Push an image
  run: docker push --file Dockerfile [MyContainerRegistryName].azurecr.io
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

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
      command: build 
      dockerFile: '**/Dockerfile'
      useDefaultContext: false
      buildContext: ContainerPOC

";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Build an image
  run: docker build --file **/Dockerfile ContainerPOC
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
  run: docker push --file Dockerfile ${{ env.ACR.FullName }} ${{ env.ACR.ImageName }}
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
    Dockerfile: app/Dockerfile
    arguments: --secret id=mysecret,src=mysecret.txt
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Build
  run: docker build --file app/Dockerfile contosoRepository --secret id=mysecret,src=mysecret.txt
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
  run: docker push --file Dockerfile ${{ env.dockerHub }} ${{ env.imageName }} --tags test1,test2
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void Docker2BuildAndPushStepTest()
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
  run: |
    docker build --file Dockerfile dockerRegistryServiceConnection1 contosoRepository --tags tag1,tag2
    docker push --file Dockerfile dockerRegistryServiceConnection1 contosoRepository --tags tag1,tag2
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void Docker2LoginStepTest()
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
  run: docker login dockerRegistryServiceConnection1
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void Docker2LogoutStepTest()
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
  run: docker logout dockerRegistryServiceConnection1
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}