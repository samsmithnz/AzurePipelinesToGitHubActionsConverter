using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class ResourcesTests
    {

        [TestMethod]
        public void ResourcesContainersTest()
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
        }

        [TestMethod]
        public void ResourcesContainersDetailTest()
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
        }

        [TestMethod]
        public void ResourcesRepositoriesWithCheckoutTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
resources:
  repositories:
  - repository: MandMCounterRepo 
    type: github
    endpoint: 'GitHub connection'
    name: SamSmithNZ/MandMCounter
    ref: myfeaturebranch
jobs:
  - job: BuildAndTestMandMProject
    pool:
      vmImage: windows-latest
    steps:
    - checkout: MandMCounterRepo
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert        
            string expected = @"
jobs:
  BuildAndTestMandMProject:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - uses: actions/checkout@v2
      with:
        repository: SamSmithNZ/MandMCounter
        ref: myfeaturebranch";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ResourcesRepositoriesNoCheckoutTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
resources:         
  repositories:
  - repository: secondaryRepo      
    type: github
    connection: myGitHubConnection
    name: samsmithnz/mandmcounter
    ref: refs/heads/myfeaturebranch
    endpoint: github.com
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ResourcesRepositoriesMultipleWithCheckoutTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
resources:
  repositories:
  - repository: MandMCounterRepo 
    type: github
    endpoint: 'GitHub connection'
    name: SamSmithNZ/MandMCounter
    ref: myfeaturebranch
  - repository: MandMCounterRepo2 
    type: bitbucket
    endpoint: 'Bitbucket connection'
    name: SamSmithNZ/MandMCounter2
    ref: myfeaturebranch
  - repository: MandMCounterRepo3
    type: git
    name: MandMCounter3
    ref: myfeaturebranch
jobs:
  - job: BuildAndTestMandMProject
    pool:
      vmImage: windows-latest
    steps:
    - checkout: MandMCounterRepo
    - checkout: MandMCounterRepo2
    - checkout: MandMCounterRepo3
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert        
            string expected = @"
#bitbucket repos don't currently have a conversion path in GitHub. This step was converted, but is unlikely to work.
#Azure Repos don't currently have a conversion path in GitHub. This step was converted, but is unlikely to work.
jobs:
  BuildAndTestMandMProject:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - uses: actions/checkout@v2
      with:
        repository: SamSmithNZ/MandMCounter
        ref: myfeaturebranch
    - # bitbucket repos don't currently have a conversion path in GitHub. This step was converted, but is unlikely to work.
      uses: actions/checkout@v2
      with:
        repository: SamSmithNZ/MandMCounter2
        ref: myfeaturebranch
    - # Azure Repos don't currently have a conversion path in GitHub. This step was converted, but is unlikely to work.
      uses: actions/checkout@v2
      with:
        repository: MandMCounter3
        ref: myfeaturebranch";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ResourcesPipelinesTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
resources:
  pipelines:
  - pipeline: SmartHotel
    project: DevOpsProject
    source: SmartHotel-CI
    branch: releases/M142
    version: 1.0
    trigger: 
      autoCancel: true
      batch: true
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("Resource pipelines conversion not yet done") > -1);
        }

    }
}