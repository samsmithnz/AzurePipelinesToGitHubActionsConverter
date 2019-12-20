using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class ResourcesTests
    {

        [TestMethod]
        public void ContainersTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
pool:
  vmImage: 'ubuntu-16.04'

strategy:
  matrix:
    DotNetCore22:
      containerImage: mcr.microsoft.com/dotnet/core/sdk:2.2
    DotNetCore22Nightly:
      containerImage: mcr.microsoft.com/dotnet/core-nightly/sdk:2.2

container: $[ variables['containerImage'] ]

resources:
  containers:
  - container: redis
    image: redis

services:
  redis: redis

variables:
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true

steps:
- task: DotNetCoreCLI@2
  displayName: Build
  inputs:
    command: build
    projects: '**/*.csproj'
    arguments: '--configuration release'

- task: DotNetCoreCLI@2
  displayName: Test
  inputs:
    command: test
    projects: '**/*Tests.csproj'
    arguments: '--configuration release'
  env:
    CONNECTIONSTRINGS_REDIS: redis:6379

- task: DotNetCoreCLI@2
  displayName: Publish
  inputs:
    command: publish
    projects: 'MyProject/MyProject.csproj'
    publishWebProjects: false
    zipAfterPublish: false
    arguments: '--configuration release'

- task: PublishPipelineArtifact@0
  displayName: Store artifact
  inputs:
    artifactName: 'MyProject'
    targetPath: 'MyProject/bin/release/netcoreapp2.2/publish/'
  condition: and(succeeded(), endsWith(variables['Agent.JobName'], 'DotNetCore22'))
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }    
        
        [TestMethod]
        public void ContainersDetailTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
pool:
  vmImage: 'ubuntu-16.04'

container: $[ 'mcr.microsoft.com/dotnet/core/sdk:2.2' ]

resources:
  containers:
  - container: nginx
    image: nginx
    ports:
    - 8080:80
    env:
      NGINX_PORT: 80
    volumes:
    - mydockervolume:/data/dir
    - /data/dir
    - /src/dir:/dst/dir
    options: --hostname container-test --ip 192.168.0.1

steps:
- task: DotNetCoreCLI@2
  displayName: Build
  inputs:
    command: build
    projects: '**/*.csproj'
    arguments: '--configuration release'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }

//        [TestMethod]
//        public void PipelinesTest()
//        {
//            //Arrange
//            Conversion conversion = new Conversion();
//            string yaml = @"
//pool:
//  vmImage: 'ubuntu-16.04'

//resources: 
//  pipelines:
//  - pipeline: ""pipeline123""  # identifier for the pipeline resource
//    project:  ""project123"" # project for the build pipeline; optional input for current project
//    source: ""source123""  # source pipeline definition name
//    //branch: string  # branch to pick the artifact, optional; defaults to all branches
//    //version: string # pipeline run number to pick artifact; optional; defaults to last successfully completed run
//    //trigger:     # optional; Triggers are not enabled by default.
//    //  branches:  
//    //    include: [string] # branches to consider the trigger events, optional; defaults to all branches.
//    //    exclude: [string] # branches to discard the trigger events, optional; defaults to none.

//        variables:
//  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true

//steps:
//- task: DotNetCoreCLI@2
//  displayName: Build
//  inputs:
//    command: build
//    projects: '**/*.csproj'
//    arguments: '--configuration release'
//";

//            //Act
//            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

//            //Assert
//            Assert.AreEqual(1, gitHubOutput.comments.Count);
//            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
//        }

    }
}