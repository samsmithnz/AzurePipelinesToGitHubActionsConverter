using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class CompletePipelineTests
    {
        //Test that the ARM template result includes the AZURE login and download artifacts tasks
        [TestMethod]
        public void ARMTemplatePipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
jobs:
- job: Deploy
  displayName: Deploy job
  pool:
    vmImage: ubuntu-latest
  variables:
    AppSettings.Environment: 'data'
    ArmTemplateResourceGroupLocation: 'eu'
    ResourceGroupName: 'MyProjectRG'
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
      azureSubscription: 'connection to Azure Portal'
      resourceGroupName: $(ResourceGroupName)
      location: '[resourceGroup().location]'
      csmFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.json'
      csmParametersFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.parameters.json'
      overrideParameters: '-environment $(AppSettings.Environment) -locationShort $(ArmTemplateResourceGroupLocation)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: 'AZURE_SP' secret is required to be setup and added into GitHub Secrets: https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets
jobs:
  Deploy:
    name: Deploy job
    runs-on: ubuntu-latest
    env:
      AppSettings.Environment: data
      ArmTemplateResourceGroupLocation: eu
      ResourceGroupName: MyProjectRG
    steps:
    - uses: actions/checkout@v2
    - #: ""Note: 'AZURE_SP' secret is required to be setup and added into GitHub Secrets: https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets""
      name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_SP }}
    - name: Download the build artifacts
      uses: actions/download-artifact@v1.0.0
      with:
        name: drop
    - name: Deploy ARM Template to resource group
      uses: Azure/github-actions/arm@master
      env:
        AZURE_RESOURCE_GROUP: ${{ env.ResourceGroupName }}
        AZURE_TEMPLATE_LOCATION: ${GITHUB_WORKSPACE}/drop/ARMTemplates/azuredeploy.json
        AZURE_TEMPLATE_PARAM_FILE: ${GITHUB_WORKSPACE}/drop/ARMTemplates/azuredeploy.parameters.json
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //Check that the results include the Setup Java step
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
jobs:
  build:
    runs-on: macos-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup JDK 1.8
      uses: actions/setup-java@v1
      with:
        java-version: 1.8
    - run: chmod +x gradlew
    - run: ./gradlew build
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //Test that the result includes the setup Java step
        [TestMethod]
        public void AntPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
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
    testResultsFiles: '**/TEST -*.xml'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup JDK 1.8
      uses: actions/setup-java@v1
      with:
        java-version: 1.8
    - run: ant -noinput -buildfile build.xml
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //TODO: Move to step, doesn't need to be here.
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
  solution: 'WindowsFormsApp1.sln'
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
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget
on:
  push:
    branches:
    - master
env:
  solution: WindowsFormsApp1.sln
  buildPlatform: Any CPU
  buildConfiguration: Release
jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - uses: microsoft/setup-msbuild@v1.0.0
    - #: 'Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget'
      uses: warrenbuckley/Setup-Nuget@v1
    - run: nuget  ${{ env.solution }}
      shell: powershell
    - run: msbuild '${{ env.solution }}' /p:configuration='${{ env.buildConfiguration }}' /p:platform='${{ env.buildPlatform }}'
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //TODO: Move to step, doesn't need to be here.
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
env:
  GOBIN: ${{ env.GOPATH }}/bin
  GOROOT: /usr/local/go1.11
  GOPATH: ${{ env.system.defaultWorkingDirectory }}/gopath
  modulePath: ${{ env.GOPATH }}/src/github.com/${{ env.build.repository.name }}
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Set up the Go workspace
      run: |
        mkdir -p '${{ env.GOBIN }}'
        mkdir -p '${{ env.GOPATH }}/pkg'
        mkdir -p '${{ env.modulePath }}'
        shopt -s extglob
        shopt -s dotglob
        mv !(gopath) '${{ env.modulePath }}'
        echo '##vso[task.prependpath]${{ env.GOBIN }}'
        echo '##vso[task.prependpath]${{ env.GOROOT }}/bin'
    - name: Get dependencies, then build
      run: |
        go version
        go get -v -t -d ./...
        if [ -f Gopkg.toml ]; then
            curl https://raw.githubusercontent.com/golang/dep/master/install.sh | sh
            dep ensure
        fi
        go build -v .
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //Check that the results include the setup java step
        [TestMethod]
        public void MavenPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://raw.githubusercontent.com/microsoft/azure-pipelines-yaml/master/templates/python-django.yml
            string yaml = @"
trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: Maven@3
  inputs:
    mavenPomFile: 'Maven/pom.xml'
    mavenOptions: '-Xmx3072m'
    javaHomeOption: 'JDKVersion'
    jdkVersionOption: '1.8'
    jdkArchitectureOption: 'x64'
    publishJUnitResults: true
    testResultsFiles: '**/surefire-reports/TEST-*.xml'
    goals: 'package'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup JDK 1.8
      uses: actions/setup-java@v1
      with:
        java-version: 1.8
    - run: mvn -B package --file Maven/pom.xml
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //Check that the results include the setup Node step
        [TestMethod]
        public void NodeJSPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://raw.githubusercontent.com/microsoft/azure-pipelines-yaml/master/templates/python-django.yml
            string yaml = @"
trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: NodeTool@0
  inputs:
    versionSpec: '10.x'
  displayName: 'Install Node.js'

- script: |
    npm install
    npm start
  displayName: 'npm install and start'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Install Node.js
      uses: actions/setup-node@v1
      with:
        node-version: 10.x
    - name: npm install and start
      run: |
        npm install
        npm start
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //TODO: Move to step, doesn't need to be here.
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
    projects: MyProject/MyProject.Models/MyProject.Models.csproj

- task: DotNetCoreCLI@2
  displayName: Build
  inputs:
    projects: MyProject/MyProject.Models/MyProject.Models.csproj
    arguments: '--configuration $(BuildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: Publish
  inputs:
    command: publish
    publishWebProjects: false
    projects: MyProject/MyProject.Models/MyProject.Models.csproj
    arguments: '--configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)'
    zipAfterPublish: false

- task: DotNetCoreCLI@2
  displayName: 'dotnet pack'
  inputs:
    command: pack
    packagesToPack: MyProject/MyProject.Models/MyProject.Models.csproj
    versioningScheme: byEnvVar
    versionEnvVar: BuildVersion

- task: PublishBuildArtifacts@1
  displayName: 'Publish Artifact'
  inputs:
    PathtoPublish: '$(build.artifactstagingdirectory)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
env:
  BuildConfiguration: Release
  BuildPlatform: Any CPU
  BuildVersion: 1.1.${{ env.Build.BuildId }}
jobs:
  build:
    runs-on: windows-latest
    container: {}
    steps:
    - uses: actions/checkout@v2
    - name: Restore
      run: dotnet restore MyProject/MyProject.Models/MyProject.Models.csproj
    - name: Build
      run: dotnet MyProject/MyProject.Models/MyProject.Models.csproj --configuration ${{ env.BuildConfiguration }}
    - name: Publish
      run: dotnet publish MyProject/MyProject.Models/MyProject.Models.csproj --configuration ${{ env.BuildConfiguration }} --output ${GITHUB_WORKSPACE}
    - name: dotnet pack
      run: dotnet pack
    - name: Publish Artifact
      uses: actions/upload-artifact@master
      with:
        path: ${GITHUB_WORKSPACE}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //Check that the results include the setup Python step
        [TestMethod]
        public void PythonPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://raw.githubusercontent.com/microsoft/azure-pipelines-yaml/master/templates/python-django.yml
            string yaml = @"
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
    addToPath: true
    architecture: 'x64'
- task: PythonScript@0
  inputs:
    scriptSource: 'filePath'
    scriptPath: 'Python/Hello.py'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
jobs:
  build:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        PYTHON_VERSION:
        - 3.5
        - 3.6
        - 3.7
      max-parallel: 3
    steps:
    - uses: actions/checkout@v2
    - name: Setup Python ${{ matrix.PYTHON_VERSION }}
      uses: actions/setup-python@v1
      with:
        python-version: ${{ matrix.PYTHON_VERSION }}
    - run: python Python/Hello.py
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //TODO: Move to step, doesn't need to be here.
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#TODO: Container conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39
on:
  push:
    branches:
    - master
jobs:
  build:
    runs-on: ubuntu-16.04
    container:
      image: redis
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //Test that the result includes the setup Ruby step
        [TestMethod]
        public void RubyPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseRubyVersion@0
  inputs:
    versionSpec: '>= 2.5'
- script: ruby HelloWorld.rb
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - master
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Setup Ruby >= 2.5
      uses: actions/setup-ruby@v1
      with:
        ruby-version: '>= 2.5'
    - run: ruby HelloWorld.rb
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //TODO: Move to step, doesn't need to be here.
        [TestMethod]
        public void TestHTMLPipeline()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
# HTML
# Archive your static HTML project and save it with the build record.

trigger:
- master

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: ArchiveFiles@2
  inputs:
    rootFolderOrFile: '$(build.sourcesDirectory)'
    includeRootFolder: false
- task: PublishBuildArtifacts@1";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: This is a third party action: https://github.com/marketplace/actions/create-zip-file
on:
  push:
    branches:
    - master
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - #: 'Note: This is a third party action: https://github.com/marketplace/actions/create-zip-file'
      uses: montudor/action-zip@v0.1.0
      with:
        args: zip -qq -r  ${{ env.build.sourcesDirectory }}
    - uses: actions/upload-artifact@master
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

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
    - uses: actions/checkout@v2
    - name: dotnet build part 1
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
  Build2:
    name: Build job
    runs-on: ubuntu-latest
    needs: Build
    env:
      myJobVariable: data
    steps:
    - uses: actions/checkout@v2
    - name: dotnet build part 2
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
    - name: dotnet build part 3
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget
on:
  push:
    branches:
    - master
env:
  buildConfiguration: Release
  outputDirectory: ${{ env.build.binariesDirectory }}/${{ env.buildConfiguration }}
jobs:
  build:
    runs-on: macos-latest
    steps:
    - uses: actions/checkout@v2
    - #: 'Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget'
      uses: warrenbuckley/Setup-Nuget@v1
    - run: nuget  **/*.sln
      shell: powershell
    - run: |
        cd Blank
        nuget restore
        cd Blank.Android
        msbuild **/*droid*.csproj /verbosity:normal /t:Rebuild /p:Configuration=${{ env.buildConfiguration }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //TODO: Move to step, doesn't need to be here.
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget
on:
  push:
    branches:
    - master
jobs:
  build:
    runs-on: macos-latest
    steps:
    - uses: actions/checkout@v2
    - name: Select the Xamarin SDK version
      run: sudo $AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh 5_12_0
    - #: 'Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget'
      uses: warrenbuckley/Setup-Nuget@v1
    - run: nuget  **/*.sln
      shell: powershell
    - run: |
        cd Blank
        nuget restore
        cd Blank.Android
        msbuild  /verbosity:normal /t:Rebuild /p:Platform=iPhoneSimulator /p:Configuration=Release
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}