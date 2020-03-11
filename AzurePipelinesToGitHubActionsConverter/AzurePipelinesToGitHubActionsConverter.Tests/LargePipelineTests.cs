using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class LargePipelineTests
    {

        [TestMethod]
        public void LargePipelineIsFullyProcessedTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
# ASP.NET Core
# Build and test ASP.NET Core projects targeting .NET Core.
# Add steps that run tests, create a NuGet package, deploy, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core
trigger:
- master
pr:
  branches:
    include:
    - '*'  
variables:
  vmImage: 'windows-latest'
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'
  buildNumber: '1.1.0.0'
stages:
- stage: Build
  displayName: 'Build/Test Stage'
  jobs:
  - job: Build
    displayName: 'Build job'
    pool:
      vmImage: $(vmImage)
    steps:
    - task: UseDotNet@2
      displayName: 'Use .NET Core sdk'
      inputs:
        packageType: sdk
        version: 2.2.203
    - task: PowerShell@2
      displayName: 'Generate build version number'
      inputs:
        targetType: 'inline'
        script: |
         Write-Host ""Generating Build Number""
         #Get the version from the csproj file
         $xml = [Xml] (Get-Content FeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj)
         $initialVersion = [Version] $xml.Project.PropertyGroup.Version
         Write-Host ""Initial Version: "" $version
         $spliteVersion = $initialVersion -Split ""\.""
         #Get the build number (number of days since January 1, 2000)
         $baseDate = [datetime]""01/01/2000""
         $currentDate = $(Get-Date)
         $interval = (NEW-TIMESPAN -Start $baseDate -End $currentDate)
         $buildNumber = $interval.Days
         #Get the revision number (number seconds (divided by two) into the day on which the compilation was performed)
         $StartDate=[datetime]::Today
         $EndDate=(GET-DATE)
         $revisionNumber = [math]::Round((New-TimeSpan -Start $StartDate -End $EndDate).TotalSeconds / 2,0)
         #Final version number
         $finalBuildVersion = ""$($spliteVersion[0]).$($spliteVersion[1]).$($buildNumber).$($revisionNumber)""
         Write-Host ""Major.Minor,Build,Revision""
         Write-Host ""Final build number: "" $finalBuildVersion
         #Writing final version number back to Azure DevOps variable
         Write-Host ""##vso[task.setvariable variable=buildNumber]$finalBuildVersion""

    - task: CopyFiles@2
      displayName: 'Copy environment ARM template files to: $(build.artifactstagingdirectory)'
      inputs:
        SourceFolder: '$(system.defaultworkingdirectory)\FeatureFlags\FeatureFlags.ARMTemplates'
        Contents: '**\*' # **\* = Copy all files and all files in sub directories
        TargetFolder: '$(build.artifactstagingdirectory)\ARMTemplates'

    - task: DotNetCoreCLI@2
      displayName: 'Test dotnet code projects'
      inputs:
        command: test
        projects: |
         FeatureFlags/FeatureFlags.Tests/FeatureFlags.Tests.csproj
        arguments: '--configuration $(buildConfiguration) --logger trx --collect ""Code coverage"" --settings:$(Build.SourcesDirectory)\FeatureFlags\FeatureFlags.Tests\CodeCoverage.runsettings'

    - task: DotNetCoreCLI@2
      displayName: 'Publish dotnet core projects'
      inputs:
        command: publish
        publishWebProjects: false
        projects: |
         FeatureFlags/FeatureFlags.Service/FeatureFlags.Service.csproj
         FeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj
        arguments: '--configuration $(buildConfiguration) --output $(build.artifactstagingdirectory) -p:Version=$(buildNumber)'
        zipAfterPublish: true

    - task: DotNetCoreCLI@2
      displayName: 'Publish dotnet core functional tests project'
      inputs:
        command: publish
        publishWebProjects: false
        projects: |
         FeatureFlags/FeatureFlags.FunctionalTests/FeatureFlags.FunctionalTests.csproj
        arguments: '--configuration $(buildConfiguration) --output $(build.artifactstagingdirectory)/FunctionalTests'
        zipAfterPublish: false

    - task: CopyFiles@2
      displayName: 'Copy Selenium Files to: $(build.artifactstagingdirectory)/FunctionalTests/FeatureFlags.FunctionalTests'
      inputs:
        SourceFolder: 'FeatureFlags/FeatureFlags.FunctionalTests/bin/$(buildConfiguration)/netcoreapp3.0'
        Contents: '*chromedriver.exe*'
        TargetFolder: '$(build.artifactstagingdirectory)/FunctionalTests/FeatureFlags.FunctionalTests'

    # Publish the artifacts
    - task: PublishBuildArtifacts@1
      displayName: 'Publish Artifact'
      inputs:
        PathtoPublish: '$(build.artifactstagingdirectory)'";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("unknown") == -1);
        }

        [TestMethod]
        public void LargeMultiStagePipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
# ASP.NET Core
# Build and test ASP.NET Core projects targeting .NET Core.
# Add steps that run tests, create a NuGet package, deploy, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core

trigger:
- master
pr:
  branches:
    include:
    - '*'

variables:
  vmImage: 'windows-latest'
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'
  buildNumber: '1.1.0.0'

stages:
- stage: Build
  displayName: 'Build/Test Stage'
  jobs:
  - job: Build
    displayName: 'Build job'
    pool:
      vmImage: $(vmImage)
    steps:
    - task: PowerShell@2
      displayName: 'Generate build version number'
      inputs:
        targetType: 'inline'
        script: |
         Write-Host ""Generating Build Number""
         #Get the version from the csproj file
         $xml = [Xml] (Get-Content FeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj)
         $initialVersion = [Version] $xml.Project.PropertyGroup.Version
         Write-Host ""Initial Version: "" $version
         $spliteVersion = $initialVersion -Split ""\.""
         #Get the build number (number of days since January 1, 2000)
         $baseDate = [datetime]""01/01/2000""
         $currentDate = $(Get-Date)
         $interval = (NEW-TIMESPAN -Start $baseDate -End $currentDate)
         $buildNumber = $interval.Days
         #Get the revision number (number seconds (divided by two) into the day on which the compilation was performed)
         $StartDate=[datetime]::Today
         $EndDate=(GET-DATE)
         $revisionNumber = [math]::Round((New-TimeSpan -Start $StartDate -End $EndDate).TotalSeconds / 2,0)
         #Final version number
         $finalBuildVersion = ""$($spliteVersion[0]).$($spliteVersion[1]).$($buildNumber).$($revisionNumber)""
         Write-Host ""Major.Minor,Build,Revision""
         Write-Host ""Final build number: "" $finalBuildVersion
         #Writing final version number back to Azure DevOps variable
         Write-Host ""##vso[task.setvariable variable=buildNumber]$finalBuildVersion""

    - task: CopyFiles@2
      displayName: 'Copy environment ARM template files to: $(build.artifactstagingdirectory)'
      inputs:
        SourceFolder: '$(system.defaultworkingdirectory)\FeatureFlags\FeatureFlags.ARMTemplates'
        Contents: '**\*' # **\* = Copy all files and all files in sub directories
        TargetFolder: '$(build.artifactstagingdirectory)\ARMTemplates'

    - task: DotNetCoreCLI@2
      displayName: 'Test dotnet code projects'
      inputs:
        command: test
        projects: |
         FeatureFlags/FeatureFlags.Tests/FeatureFlags.Tests.csproj
        arguments: '--configuration $(buildConfiguration) --logger trx --collect ""Code coverage"" --settings:$(Build.SourcesDirectory)\FeatureFlags\FeatureFlags.Tests\CodeCoverage.runsettings'

    - task: DotNetCoreCLI@2
      displayName: 'Publish dotnet core projects'
      inputs:
        command: publish
        publishWebProjects: false
        projects: |
         FeatureFlags/FeatureFlags.Service/FeatureFlags.Service.csproj
         FeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj
        arguments: '--configuration $(buildConfiguration) --output $(build.artifactstagingdirectory) -p:Version=$(buildNumber)'
        zipAfterPublish: true

    - task: DotNetCoreCLI@2
      displayName: 'Publish dotnet core functional tests project'
      inputs:
        command: publish
        publishWebProjects: false
        projects: |
         FeatureFlags/FeatureFlags.FunctionalTests/FeatureFlags.FunctionalTests.csproj
        arguments: '--configuration $(buildConfiguration) --output $(build.artifactstagingdirectory)/FunctionalTests'
        zipAfterPublish: false

    - task: CopyFiles@2
      displayName: 'Copy Selenium Files to: $(build.artifactstagingdirectory)/FunctionalTests/FeatureFlags.FunctionalTests'
      inputs:
        SourceFolder: 'FeatureFlags/FeatureFlags.FunctionalTests/bin/$(buildConfiguration)/netcoreapp3.0'
        Contents: '*chromedriver.exe*'
        TargetFolder: '$(build.artifactstagingdirectory)/FunctionalTests/FeatureFlags.FunctionalTests'

    # Publish the artifacts
    - task: PublishBuildArtifacts@1
      displayName: 'Publish Artifact'
      inputs:
        PathtoPublish: '$(build.artifactstagingdirectory)'

- stage: Deploy
  displayName: 'Deploy Prod'
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/master'))
  jobs:
  - job: Deploy
    displayName: 'Deploy job'
    pool:
      vmImage: $(vmImage)   
    variables:
      AppSettings.Environment: 'data'
      ArmTemplateResourceGroupLocation: 'eu'
      ResourceGroupName: 'SamLearnsAzureFeatureFlags'
      WebsiteName: 'featureflags-data-eu-web'
      WebServiceName: 'featureflags-data-eu-service'
    steps:
    - task: DownloadBuildArtifacts@0
      displayName: 'Download the build artifacts'
      inputs:
        buildType: 'current'
        downloadType: 'single'
        artifactName: 'drop'
        downloadPath: '$(build.artifactstagingdirectory)'
    - task: AzureResourceGroupDeployment@2
      displayName: 'Deploy ARM Template to resource group'
      inputs:
        azureSubscription: 'SamLearnsAzure connection to Azure Portal'
        resourceGroupName: $(ResourceGroupName)
        location: '[resourceGroup().location]'
        csmFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.json'
        csmParametersFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.parameters.json'
        overrideParameters: '-environment $(AppSettings.Environment) -locationShort $(ArmTemplateResourceGroupLocation)'
    - task: AzureRmWebAppDeployment@3
      displayName: 'Azure App Service Deploy: web service'
      inputs:
        azureSubscription: 'SamLearnsAzure connection to Azure Portal'
        WebAppName: $(WebServiceName)
        DeployToSlotFlag: true
        ResourceGroupName: $(ResourceGroupName)
        SlotName: 'staging'
        Package: '$(build.artifactstagingdirectory)/drop/FeatureFlags.Service.zip'
        TakeAppOfflineFlag: true
        JSONFiles: '**/appsettings.json'
    - task: AzureRmWebAppDeployment@3
      displayName: 'Azure App Service Deploy: web site'
      inputs:
        azureSubscription: 'SamLearnsAzure connection to Azure Portal'
        WebAppName: $(WebsiteName)
        DeployToSlotFlag: true
        ResourceGroupName: $(ResourceGroupName)
        SlotName: 'staging'
        Package: '$(build.artifactstagingdirectory)/drop/FeatureFlags.Web.zip'
        TakeAppOfflineFlag: true
        JSONFiles: '**/appsettings.json'        
    - task: VSTest@2
      displayName: 'Run functional smoke tests on website and web service'
      inputs:
        searchFolder: '$(build.artifactstagingdirectory)'
        testAssemblyVer2: |
          **\FeatureFlags.FunctionalTests\FeatureFlags.FunctionalTests.dll
        uiTests: true
        runSettingsFile: '$(build.artifactstagingdirectory)/drop/FunctionalTests/FeatureFlags.FunctionalTests/test.runsettings'
        overrideTestrunParameters: |
         -ServiceUrl ""https://$(WebServiceName)-staging.azurewebsites.net/"" 
         -WebsiteUrl ""https://$(WebsiteName)-staging.azurewebsites.net/"" 
         -TestEnvironment ""$(AppSettings.Environment)"" 
    - task: AzureAppServiceManage@0
      displayName: 'Swap Slots: web service'
      inputs:
        azureSubscription: 'SamLearnsAzure connection to Azure Portal'
        WebAppName: $(WebServiceName)
        ResourceGroupName: $(ResourceGroupName)
        SourceSlot: 'staging'
    - task: AzureAppServiceManage@0
      displayName: 'Swap Slots: website'
      inputs:
        azureSubscription: 'SamLearnsAzure connection to Azure Portal'
        WebAppName: $(WebsiteName)
        ResourceGroupName: $(ResourceGroupName)
        SourceSlot: 'staging'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.IsTrue(gitHubOutput.comments.Count == 0);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }


        [TestMethod]
        public void StagingPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
- stage: Build
  displayName: 'Build Stage'
  jobs:
  - job: Build
    displayName: 'Build job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: |
         Write-Host ""Hello world!""

  - job: Build2
    displayName: 'Build job 2'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: |
         Write-Host ""Hello world 2!""
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  Build_Stage_Build:
    name: Build job
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v1
    - run: 
        Write-Host ""Hello world!""
      shell: powershell
  Build_Stage_Build2:
    name: Build job 2
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v1
    - run: 
        Write-Host ""Hello world 2!""
      shell: powershell";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void NuGetPackagePipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
resources:
- repo: self
  containers:
  - container: test123

trigger:
- master

pool:
  vmImage: 'windows-latest'

variables:
  BuildConfiguration: 'Release'
  BuildPlatform : 'Any CPU'
  BuildVersion: 1.1.$(Build.BuildId)

steps:
- task: DotNetCoreCLI@2
  displayName: Restore
  inputs:
    command: restore
    projects: SamLearnsAzure/SamLearnsAzure.Models/SamLearnsAzure.Models.csproj

- task: DotNetCoreCLI@2
  displayName: Build
  inputs:
    projects: SamLearnsAzure/SamLearnsAzure.Models/SamLearnsAzure.Models.csproj
    arguments: '--configuration $(BuildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: Publish
  inputs:
    command: publish
    publishWebProjects: false
    projects: SamLearnsAzure/SamLearnsAzure.Models/SamLearnsAzure.Models.csproj
    arguments: '--configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)'
    zipAfterPublish: false

- task: DotNetCoreCLI@2
  displayName: 'dotnet pack'
  inputs:
    command: pack
    packagesToPack: SamLearnsAzure/SamLearnsAzure.Models/SamLearnsAzure.Models.csproj
    versioningScheme: byEnvVar
    versionEnvVar: BuildVersion

- task: PublishBuildArtifacts@1
  displayName: 'Publish Artifact'
  inputs:
    PathtoPublish: '$(build.artifactstagingdirectory)'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.IsTrue(gitHubOutput.comments.Count == 0);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }


        [TestMethod]
        public void ConditionOnStagePipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master

stages:
- stage: Deploy
  displayName: 'Deploy Prod'
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/master'))
  jobs:
  - job: Deploy
    displayName: 'Deploy job'
    pool:
      vmImage: ubuntu-latest  
    steps:
    - task: DownloadBuildArtifacts@0
      displayName: 'Download the build artifacts'
      inputs:
        buildType: 'current'
        downloadType: 'single'
        artifactName: 'drop'
        downloadPath: '$(build.artifactstagingdirectory)'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(0, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }

        [TestMethod]
        public void ResourcesContainersPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master

pool:
  vmImage: 'ubuntu-16.04'

container: 'mcr.microsoft.com/dotnet/core/sdk:2.2'

resources:
  containers:
  - container: redis
    image: redis
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }

        [TestMethod]
        public void DotNetDesktopPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/.net-desktop.yml
            string yaml = @"
# .NET Desktop
# Build and run tests for .NET Desktop or Windows classic desktop solutions.
# Add steps that publish symbols, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/apps/windows/dot-net

trigger:
- master

pool:
  vmImage: 'windows-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

steps:
- task: NuGetToolInstaller@1

- task: NuGetCommand@2
  inputs:
    restoreSolution: '$(solution)'

- task: VSBuild@1
  inputs:
    solution: '$(solution)'
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'

- task: VSTest@2
  inputs:
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }


        [TestMethod]
        public void AspDotNetFrameworkPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/asp.net-core-.net-framework.yml
            string yaml = @"
# ASP.NET Core (.NET Framework)
# Build and test ASP.NET Core projects targeting the full .NET Framework.
# Add steps that publish symbols, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core

trigger:
- master

pool:
  vmImage: 'windows-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

steps:
- task: NuGetToolInstaller@1

- task: NuGetCommand@2
  inputs:
    restoreSolution: '$(solution)'

- task: VSBuild@1
  inputs:
    solution: '$(solution)'
    msbuildArgs: '/p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:DesktopBuildPackageLocation=""$(build.artifactStagingDirectory)\WebApp.zip"" /p:DeployIisAppPath=""Default Web Site""'
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'

- task: VSTest@2
  inputs:
    platform: '$(buildPlatform)'
    configuration: '$(buildConfiguration)'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }

        [TestMethod]
        public void DockerBuildPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/docker-build.yml
            string yaml = @"
# Docker
# Build a Docker image 
# https://docs.microsoft.com/azure/devops/pipelines/languages/docker

trigger:
- master

resources:
- repo: self

variables:
  tag: '$(Build.BuildId)'

stages:
- stage: Build
  displayName: Build image
  jobs:  
  - job: Build
    displayName: Build
    pool:
      vmImage: 'ubuntu-latest'
    steps:
    - task: Docker@2
      displayName: Build an image
      inputs:
        command: build
        dockerfile: '{{ dockerfilePath }}'
        tags: |
          $(tag)
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(0, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }


        [TestMethod]
        public void AspDotNetCoreFunctionsPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://github.com/microsoft/azure-pipelines-yaml/blob/master/templates/asp.net-core-functionapp-to-windows-on-azure.yml
            string yaml = @"
# .NET Core Function App to Windows on Azure
# Build a .NET Core function app and deploy it to Azure as a Windows function App.
# Add steps that analyze code, save build artifacts, deploy, and more:
# https://docs.microsoft.com/en-us/azure/devops/pipelines/languages/dotnet-core

trigger:
- master

variables:
  # Azure Resource Manager connection created during pipeline creation
  azureSubscription: '{{ azureRmConnection.Id }}'

  # Function app name
  functionAppName: '{{ functionAppName }}'

  # Agent VM image name
  vmImageName: 'vs2017-win2016'

  # Working Directory
  workingDirectory: '{{ workingDirectory }}'

stages:
- stage: Build
  displayName: Build stage

  jobs:
  - job: Build
    displayName: Build
    pool:
      vmImage: $(vmImageName)

    steps:
    - task: DotNetCoreCLI@2
      displayName: Build
      inputs:
        command: 'build'
        projects: |
          $(workingDirectory)/*.csproj
        arguments: --output $(System.DefaultWorkingDirectory)/publish_output --configuration Release

    - task: ArchiveFiles@2
      displayName: 'Archive files'
      inputs:
        rootFolderOrFile: '$(System.DefaultWorkingDirectory)/publish_output'
        includeRootFolder: false
        archiveType: zip
        archiveFile: $(Build.ArtifactStagingDirectory)/$(Build.BuildId).zip
        replaceExistingArchive: true

    - publish: $(Build.ArtifactStagingDirectory)/$(Build.BuildId).zip
      artifact: drop

- stage: Deploy
  displayName: Deploy stage
  dependsOn: Build
  condition: succeeded()

  jobs:
  - deployment: Deploy
    displayName: Deploy
    environment: 'development'
    pool:
      vmImage: $(vmImageName)

    strategy:
      runOnce:
        deploy:

          steps:
          - task: PowerShell@2
            displayName: 'Test'
            inputs:
              targetType: inline
              script: |
                Write-Host ""Hello world""
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(2, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }

        [TestMethod]
        public void AndroidPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://raw.githubusercontent.com/microsoft/azure-pipelines-yaml/master/templates/android.yml
            string yaml = @"
# Android
# Build your Android project with Gradle.
# Add steps that test, sign, and distribute the APK, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/android

trigger:
- master

pool:
  vmImage: 'macos-latest'

steps:
- task: Gradle@2
  inputs:
    workingDirectory: ''
    gradleWrapperFile: 'gradlew'
    gradleOptions: '-Xmx3072m'
    publishJUnitResults: false
    testResultsFiles: '**/TEST-*.xml'
    tasks: 'assembleDebug'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") >= 0);
        }


        [TestMethod]
        public void AntPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://raw.githubusercontent.com/microsoft/azure-pipelines-yaml/master/templates/ant.yml
            string yaml = @"
# Ant
# Build your Java projects and run tests with Apache Ant.
# Add steps that save build artifacts and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/java

trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: Ant@1
  inputs:
    workingDirectory: ''
    buildFile: 'build.xml'
    javaHomeOption: 'JDKVersion'
    jdkVersionOption: '1.8'
    jdkArchitectureOption: 'x64'
    publishJUnitResults: true
    testResultsFiles: '**/TEST-*.xml'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(1, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") >= 0);
        }

        [TestMethod]
        public void GoPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://raw.githubusercontent.com/microsoft/azure-pipelines-yaml/master/templates/go.yml
            string yaml = @"
# Go
# Build your Go project.
# Add steps that test, save build artifacts, deploy, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/go

trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

variables:
  GOBIN:  '$(GOPATH)/bin' # Go binaries path
  GOROOT: '/usr/local/go1.11' # Go installation path
  GOPATH: '$(system.defaultWorkingDirectory)/gopath' # Go workspace path
  modulePath: '$(GOPATH)/src/github.com/$(build.repository.name)' # Path to the module's code

steps:
- script: |
    mkdir -p '$(GOBIN)'
    mkdir -p '$(GOPATH)/pkg'
    mkdir -p '$(modulePath)'
    shopt -s extglob
    shopt -s dotglob
    mv !(gopath) '$(modulePath)'
    echo '##vso[task.prependpath]$(GOBIN)'
    echo '##vso[task.prependpath]$(GOROOT)/bin'
  displayName: 'Set up the Go workspace'

- script: |
    go version
    go get -v -t -d ./...
    if [ -f Gopkg.toml ]; then
        curl https://raw.githubusercontent.com/golang/dep/master/install.sh | sh
        dep ensure
    fi
    go build -v .
  workingDirectory: '$(modulePath)'
  displayName: 'Get dependencies, then build'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(0, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") == -1);
        }

        [TestMethod]
        public void PythonPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://raw.githubusercontent.com/microsoft/azure-pipelines-yaml/master/templates/python-django.yml
            string yaml = @"
# Python Django
# Test a Django project on multiple versions of Python.
# Add steps that analyze code, save build artifacts, deploy, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/python

trigger:
- master

pool:
  vmImage: 'ubuntu-latest'
strategy:
  matrix:
    Python35:
      PYTHON_VERSION: '3.5'
    Python36:
      PYTHON_VERSION: '3.6'
    Python37:
      PYTHON_VERSION: '3.7'
  maxParallel: 3

steps:
- task: UsePythonVersion@0
  inputs:
    versionSpec: '$(PYTHON_VERSION)'
    architecture: 'x64'

- task: PythonScript@0
  displayName: 'Export project path'
  inputs:
    scriptSource: 'inline'
    script: |
      """"Search all subdirectories for `manage.py`.""""
      from glob import iglob
      from os import path
      # Python >= 3.5
      manage_py = next(iglob(path.join('**', 'manage.py'), recursive=True), None)
      if not manage_py:
          raise SystemExit('Could not find a Django project')
      project_location = path.dirname(path.abspath(manage_py))
      print('Found Django project in', project_location)
      print('##vso[task.setvariable variable=projectRoot]{}'.format(project_location))

- script: |
    python -m pip install --upgrade pip setuptools wheel
    pip install -r requirements.txt
    pip install unittest-xml-reporting
  displayName: 'Install prerequisites'

- script: |
    pushd '$(projectRoot)'
    python manage.py test --testrunner xmlrunner.extra.djangotestrunner.XMLTestRunner --no-input
  displayName: 'Run tests'

- task: PublishTestResults@2
  inputs:
    testResultsFiles: ""**/TEST-*.xml""
    testRunTitle: 'Python $(PYTHON_VERSION)'
  condition: succeededOrFailed()
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(3, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") >= 0);
        }


        [TestMethod]
        public void XamariniOSPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://raw.githubusercontent.com/microsoft/azure-pipelines-yaml/master/templates/xamarin.ios.yml
            string yaml = @"
# Xamarin.iOS
# Build a Xamarin.iOS project.
# Add steps that install certificates, test, sign, and distribute an app, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/xamarin

trigger:
- master

pool:
  vmImage: 'macos-latest'

steps:
# To manually select a Xamarin SDK version on the Microsoft-hosted macOS agent,
# configure this task with the *Mono* version that is associated with the
# Xamarin SDK version that you need, and set the ""enabled"" property to true.
# See https://go.microsoft.com/fwlink/?linkid=871629
- script: sudo $AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh 5_12_0
  displayName: 'Select the Xamarin SDK version'
  enabled: false

- task: NuGetToolInstaller@1

- task: NuGetCommand@2
  inputs:
    restoreSolution: '**/*.sln'

- task: XamariniOS@2
  inputs:
    solutionFile: '**/*.sln'
    configuration: 'Release'
    buildForSimulator: true
    packageApp: false
        ";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(2, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") >= 0);
        }

        [TestMethod]
        public void XamarinAndroidPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: 
            string yaml = @"
# Xamarin.Android
# Build a Xamarin.Android project.
# Add steps that test, sign, and distribute an app, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/xamarin

trigger:
- master

pool:
  vmImage: 'macos-latest'

variables:
  buildConfiguration: 'Release'
  outputDirectory: '$(build.binariesDirectory)/$(buildConfiguration)'

steps:
- task: NuGetToolInstaller@1

- task: NuGetCommand@2
  inputs:
    restoreSolution: '**/*.sln'

- task: XamarinAndroid@1
  inputs:
    projectFile: '**/*droid*.csproj'
    outputDirectory: '$(outputDirectory)'
    configuration: '$(buildConfiguration)'
        ";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            Assert.AreEqual(2, gitHubOutput.comments.Count);
            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") >= 0);
        }

        //        [TestMethod]
        //        public void TemplatePipelineTest()
        //        {
        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            //Source is: 
        //            string yaml = @"

        //";

        //            //Act
        //            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

        //            //Assert
        //            Assert.AreEqual(1, gitHubOutput.comments.Count);
        //            Assert.IsTrue(gitHubOutput.actionsYaml.IndexOf("This step does not have a conversion path yet") >= 0);
        //        }

    }
}