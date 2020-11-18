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
    - # ""Note: 'AZURE_SP' secret is required to be setup and added into GitHub Secrets: https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets""
      name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_SP }}
    - name: Download the build artifacts
      uses: actions/download-artifact@v1.0.0
      with:
        name: drop
    - name: Deploy ARM Template to resource group
      uses: Azure/cli@v1.0.0
      with:
        inlineScript: az deployment group create --resource-group ${{ env.ResourceGroupName }} --template-file ${GITHUB_WORKSPACE}/drop/ARMTemplates/azuredeploy.json --parameters  ${GITHUB_WORKSPACE}/drop/ARMTemplates/azuredeploy.parameters.json -environment ${{ env.AppSettings.Environment }} -locationShort ${{ env.ArmTemplateResourceGroupLocation }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
    - # 'Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget'
      uses: warrenbuckley/Setup-Nuget@v1
    - run: nuget  ${{ env.solution }}
      shell: powershell
    - run: msbuild '${{ env.solution }}' /p:configuration='${{ env.buildConfiguration }}' /p:platform='${{ env.buildPlatform }}'
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
#TODO: Container conversion not yet done, we need help!: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39
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
    steps:
    - uses: actions/checkout@v2
    - name: Restore
      run: dotnet restore MyProject/MyProject.Models/MyProject.Models.csproj
    - name: Build
      run: dotnet MyProject/MyProject.Models/MyProject.Models.csproj --configuration ${{ env.BuildConfiguration }}
    - name: Publish
      run: dotnet publish MyProject/MyProject.Models/MyProject.Models.csproj --configuration ${{ env.BuildConfiguration }} --output ${GITHUB_WORKSPACE}
    - name: dotnet pack
      run: dotnet pack MyProject/MyProject.Models/MyProject.Models.csproj
    - name: Publish Artifact
      uses: actions/upload-artifact@v2
      with:
        path: ${GITHUB_WORKSPACE}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#TODO: Container conversion not yet done, we need help!: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
        }

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
    - # 'Note: This is a third party action: https://github.com/marketplace/actions/create-zip-file'
      uses: montudor/action-zip@v0.1.0
      with:
        args: zip -qq -r  ${{ env.build.sourcesDirectory }}
    - uses: actions/upload-artifact@v2
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
    needs:
    - Build
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
    - # 'Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget'
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
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
    - # 'Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget'
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
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
        }

        [TestMethod]
        public void PipelineWithWorkspaceAndTemplateStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://raw.githubusercontent.com/microsoft/azure-pipelines-yaml/master/templates/xamarin.ios.yml
            string yaml = @"
name: $(Version).$(rev:r)

variables:
- group: Common Netlify

trigger:
  branches:
    include:
    - dev
    - feature/*
    - hotfix/*
  paths:
    include:
    - 'Netlify/*'
    exclude:
    - 'pipelines/*'
    - 'scripts/*'
    - '.editorconfig'
    - '.gitignore'
    - 'README.md'

stages:
# Build Pipeline
- stage: Build
  jobs:
  - job: HostedVs2017
    displayName: Hosted VS2017
    pool:
      name: Hosted VS2017
      demands: npm
    workspace:
      clean: all
    
    steps:
    - template: templates/npm-build-steps.yaml
      parameters:
        extensionName: $(ExtensionName)
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#There is no conversion path for templates, currently there is no support to call other actions/yaml files from a GitHub Action
name: ${{ env.Version }}.${GITHUB_RUN_NUMBER}
on:
  push:
    branches:
    - dev
    - feature/*
    - hotfix/*
    paths:
    - Netlify/*
    paths-ignore:
    - pipelines/*
    - scripts/*
    - .editorconfig
    - .gitignore
    - README.md
env:
  group: Common Netlify
jobs:
  Build_Stage_HostedVs2017:
    name: Hosted VS2017
    runs-on: Hosted VS2017
    steps:
    - uses: actions/checkout@v2
    - # There is no conversion path for templates, currently there is no support to call other actions/yaml files from a GitHub Action
      run: |
        #templates/npm-build-steps.yaml
        extensionName: ${{ env.ExtensionName }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
        }


        [TestMethod]
        public void JRPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/128
            string yaml = @"
trigger:
- master

pool: 'Pipeline-Demo-Windows'


variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

stages:

- stage: Build
  jobs: 
    - job: BuildSpark
      pool:
        name: 'Pipeline-Demo-Windows'
        demands:
        - Agent.OS -equals Windows_NT
      steps:
      - task: NuGetToolInstaller@1

      - task: NuGetCommand@2
        inputs:
          restoreSolution: '$(solution)'

      - task: VSBuild@1
        inputs:
          solution: '$(solution)'
          msbuildArgs: '/p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation=""$(build.artifactStagingDirectory)""'
          platform: '$(buildPlatform)'
          configuration: '$(buildConfiguration)'

      - task: PublishPipelineArtifact@1
        inputs:
          targetPath: '$(build.artifactStagingDirectory)'
          artifact: 'WebDeploy'
          publishLocation: 'pipeline'

- stage: Deploy
  jobs:
    - deployment: 
      variables:
        - name: Art
          value: ""Server=.;Database=Art;Trusted_Connection=True;""
        
      environment: 
        name: windows-server
        resourceType: VirtualMachine
        tags: web
      strategy:
        runOnce:
          deploy:
            steps:
              - task: DownloadPipelineArtifact@2
                inputs:
                  buildType: 'current'
                  artifactName: 'WebDeploy'
                  targetPath: '$(Pipeline.Workspace)'


              - task: CmdLine@2
                inputs:
                  script: |
                    echo Write your commands here
                    
                    DIR
                  workingDirectory: '$(Pipeline.Workspace)'
                  
              - task: IISWebAppManagementOnMachineGroup@0
                inputs:
                  IISDeploymentType: 'IISWebsite'
                  ActionIISWebsite: 'CreateOrUpdateWebsite'
                  WebsiteName: 'Spark'
                  WebsitePhysicalPath: '%SystemDrive%\inetpub\wwwroot'
                  WebsitePhysicalPathAuth: 'WebsiteUserPassThrough'
                  AddBinding: true
                  CreateOrUpdateAppPoolForWebsite: true
                  ConfigureAuthenticationForWebsite: true
                  AppPoolNameForWebsite: 'Spark'
                  DotNetVersionForWebsite: 'v4.0'
                  PipeLineModeForWebsite: 'Integrated'
                  AppPoolIdentityForWebsite: 'ApplicationPoolIdentity'
                  AnonymousAuthenticationForWebsite: true
                  WindowsAuthenticationForWebsite: false
                  protocol: 'http' 
                  iPAddress: 'All Unassigned'
                  port: '80'
                  
              - task: IISWebAppDeploymentOnMachineGroup@0
                inputs:
                  WebSiteName: 'Spark'
                  Package: '$(Pipeline.Workspace)\Art.Web.zip'
                  XmlVariableSubstitution: true
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: Error! This step does not have a conversion path yet: IISWebAppDeploymentOnMachineGroup@0
#Note: Error! This step does not have a conversion path yet: IISWebAppManagementOnMachineGroup@0
#Note: Error! This step does not have a conversion path yet: DownloadPipelineArtifact@2
#Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yetNote: Azure DevOps job environment does not have an equivalent in GitHub Actions yet
#Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget
on:
  push:
    branches:
    - master
env:
  solution: '**/*.sln'
  buildPlatform: Any CPU
  buildConfiguration: Release
jobs:
  Build_Stage_BuildSpark:
    runs-on: Pipeline-Demo-Windows
    steps:
    - uses: actions/checkout@v2
    - uses: microsoft/setup-msbuild@v1.0.0
    - # 'Note: This is a third party action: https://github.com/warrenbuckley/Setup-Nuget'
      uses: warrenbuckley/Setup-Nuget@v1
    - run: nuget  ${{ env.solution }}
      shell: powershell
    - run: msbuild '${{ env.solution }}' /p:configuration='${{ env.buildConfiguration }}' /p:platform='${{ env.buildPlatform }}' /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation=""${{ env.build.artifactStagingDirectory }}""
    - uses: actions/upload-artifact@v2
      with:
        path: ${{ env.build.artifactStagingDirectory }}
  Deploy_Stage_job1:
    # 'Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yetNote: Azure DevOps job environment does not have an equivalent in GitHub Actions yet'
    env:
      Art: Server=.;Database=Art;Trusted_Connection=True;
    steps:
    - # 'Note: Error! This step does not have a conversion path yet: DownloadPipelineArtifact@2'
      run: 'Write-Host Note: Error! This step does not have a conversion path yet: DownloadPipelineArtifact@2 #task: DownloadPipelineArtifact@2#inputs:#  buildtype: current#  artifactname: WebDeploy#  targetpath: ${{ env.Pipeline.Workspace }}'
      shell: powershell
    - run: |
        echo Write your commands here

        DIR
      shell: cmd
    - # 'Note: Error! This step does not have a conversion path yet: IISWebAppManagementOnMachineGroup@0'
      run: ""Write-Host Note: Error! This step does not have a conversion path yet: IISWebAppManagementOnMachineGroup@0 #task: IISWebAppManagementOnMachineGroup@0#inputs:#  iisdeploymenttype: IISWebsite#  actioniiswebsite: CreateOrUpdateWebsite#  websitename: Spark#  websitephysicalpath: '%SystemDrive%\\inetpub\\wwwroot'#  websitephysicalpathauth: WebsiteUserPassThrough#  addbinding: true#  createorupdateapppoolforwebsite: true#  configureauthenticationforwebsite: true#  apppoolnameforwebsite: Spark#  dotnetversionforwebsite: v4.0#  pipelinemodeforwebsite: Integrated#  apppoolidentityforwebsite: ApplicationPoolIdentity#  anonymousauthenticationforwebsite: true#  windowsauthenticationforwebsite: false#  protocol: http#  ipaddress: All Unassigned#  port: 80""
      shell: powershell
    - # 'Note: Error! This step does not have a conversion path yet: IISWebAppDeploymentOnMachineGroup@0'
      run: 'Write-Host Note: Error! This step does not have a conversion path yet: IISWebAppDeploymentOnMachineGroup@0 #task: IISWebAppDeploymentOnMachineGroup@0#inputs:#  websitename: Spark#  package: ${{ env.Pipeline.Workspace }}\Art.Web.zip#  xmlvariablesubstitution: true'
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
        }

        [TestMethod]
        public void SSParentPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/128
            string yaml = @"
variables:
- group: 'myapp KeyVault'
- name: vmImage #Note this weird name/value syntax if you need to reference a variable group in variables
  value: 'windows-latest'

stages:
- stage: DeployPR
  displayName: 'Deploy PR Stage'
  condition: and(succeeded(), eq(variables['Build.Reason'], 'PullRequest'), ne(variables['System.PullRequest.PullRequestId'], 'Null'))
  dependsOn: Build
  variables:
    ${{ if ne(variables['Build.SourceBranchName'], 'master') }}:
      prId: ""$(System.PullRequest.PullRequestId)""
    ${{ if eq(variables['Build.SourceBranchName'], 'master') }}:
      prId: '000'
    prUC: ""PR$(prId)""
    prLC: ""pr$(prId)""
  jobs:
  - template: azure-pipelines-deployment-template.yml
    parameters:
      #Note that pull request environments use Dev credentials
      applicationInsightsApiKey: '$(ApplicationInsights--APIKeyDev)'
      applicationInsightsApplicationId: '$(ApplicationInsights--ApplicationIdDev)'
      applicationInsightsInstrumentationKey: $(ApplicationInsights--InstrumentationKeyDev)
      applicationInsightsLocation: 'East US'
      appServiceContributerClientSecret: $(appServiceContributerClientSecret)
      ASPNETCOREEnvironmentSetting: 'Development'
      captureStartErrors: true
      cognitiveServicesSubscriptionKey: $(cognitiveServicesSubscriptionKey)
      environment: $(prUC)
      environmentLowercase: $(prLC)
      databaseLoginName: $(databaseLoginNameDev) 
      databaseLoginPassword: $(databaseLoginPasswordDev)
      databaseServerName: 'myapp-$(prLC)-eu-sqlserver'
      godaddy_key: $(GoDaddyAPIKey)
      godaddy_secret: $(GoDaddyAPISecret)
      keyVaultClientId: '$(KeyVaultClientId)'
      keyVaultClientSecret: '$(KeyVaultClientSecret)'
      imagesStorageCDNURL: 'https://myapp-$(prLC)-eu-cdnendpoint.azureedge.net/'
      imagesStorageURL: 'https://myapp$(prLC)eustorage.blob.core.windows.net/'
      redisCacheConnectionString: '$(AppSettings--RedisCacheConnectionStringDev)'
      resourceGroupName: 'myapp$(prUC)'
      resourceGroupLocation: 'East US'
      resourceGroupLocationShort: 'eu'
      myappConnectionString: '$(ConnectionStrings--myappConnectionStringDev)'
      serviceName: 'myapp-$(prLC)-eu-service'
      serviceStagingUrl: 'https://myapp-$(prLC)-eu-service-staging.azurewebsites.net/'
      serviceUrl: 'https://myapp-$(prLC)-eu-service.azurewebsites.net/'
      storageAccountName: 'myapp$(prLC)eustorage'
      storageAccountKey: '$(StorageAccountKeyProd)'
      userPrincipalLogin: $(userPrincipalLogin)
      vmImage: $(vmImage)
      websiteName: 'myapp-$(prLC)-eu-web'
      websiteDomainName: '$(prLC).myapp.com'
      websiteStagingUrl: 'https://myapp-$(prLC)-eu-web-staging.azurewebsites.net/'
      websiteUrl: 'https://myapp-$(prLC)-eu-web.azurewebsites.net/'   
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: Azure DevOps template does not have an equivalent in GitHub Actions yet
env:
  group: myapp KeyVault
  vmImage: windows-latest
jobs:
  DeployPR_Stage_Template:
    # 'Note: Azure DevOps template does not have an equivalent in GitHub Actions yet'
    env:
      prId: 000
      prUC: PR${{ env.prId }}
      prLC: pr${{ env.prId }}
    if: and(success(),eq(variables['Build.Reason'], 'PullRequest'),ne(variables['System.PullRequest.PullRequestId'], 'Null'))
    steps:
    - uses: actions/checkout@v2
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
        }

        //This test doesn't work with V1
        [TestMethod]
        public void SSDeploymentPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/128
            string yaml = @"
parameters:
  #Note that pull request environments use Dev credentials
  applicationInsightsApiKey: '$(ApplicationInsights--APIKeyDev)'
  applicationInsightsApplicationId: '$(ApplicationInsights--ApplicationIdDev)'
  applicationInsightsInstrumentationKey: $(ApplicationInsights--InstrumentationKeyDev)
  applicationInsightsLocation: 'East US'
  appServiceContributerClientSecret: $(appServiceContributerClientSecret)
  ASPNETCOREEnvironmentSetting: 'Development'
  captureStartErrors: true
  cognitiveServicesSubscriptionKey: $(cognitiveServicesSubscriptionKey)
  environment: $(prUC)
  environmentLowercase: $(prLC)
  databaseLoginName: $(databaseLoginNameDev) 
  databaseLoginPassword: $(databaseLoginPasswordDev)
  databaseServerName: 'myapp-$(prLC)-eu-sqlserver'
  godaddy_key: $(GoDaddyAPIKey)
  godaddy_secret: $(GoDaddyAPISecret)
  keyVaultClientId: '$(KeyVaultClientId)'
  keyVaultClientSecret: '$(KeyVaultClientSecret)'
  imagesStorageCDNURL: 'https://myapp-$(prLC)-eu-cdnendpoint.azureedge.net/'
  imagesStorageURL: 'https://myapp$(prLC)eustorage.blob.core.windows.net/'
  redisCacheConnectionString: '$(AppSettings--RedisCacheConnectionStringDev)'
  resourceGroupName: 'myapp$(prUC)'
  resourceGroupLocation: 'East US'
  resourceGroupLocationShort: 'eu'
  myappConnectionString: '$(ConnectionStrings--myappConnectionStringDev)'
  serviceName: 'myapp-$(prLC)-eu-service'
  serviceStagingUrl: 'https://myapp-$(prLC)-eu-service-staging.azurewebsites.net/'
  serviceUrl: 'https://myapp-$(prLC)-eu-service.azurewebsites.net/'
  storageAccountName: 'myapp$(prLC)eustorage'
  storageAccountKey: '$(StorageAccountKeyProd)'
  userPrincipalLogin: $(userPrincipalLogin)
  vmImage: $(vmImage)
  websiteName: 'myapp-$(prLC)-eu-web'
  websiteDomainName: '$(prLC).myapp.com'
  websiteStagingUrl: 'https://myapp-$(prLC)-eu-web-staging.azurewebsites.net/'
  websiteUrl: 'https://myapp-$(prLC)-eu-web.azurewebsites.net/'
 

jobs:

  - deployment: DeployTests1
    displayName: ""Deploy functional tests to ${{parameters.environment}} job""
    environment: ${{parameters.environment}}
    pool:
      vmImage: ${{parameters.vmImage}}        
    strategy:
      runOnce:
        deploy:
          steps:
          - task: DownloadBuildArtifacts@0
            displayName: 'Download the build artifacts'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: 'drop'
              downloadPath: '$(build.artifactstagingdirectory)'

  - deployment: DeployTests2
    displayName: ""Deploy functional tests to ${{parameters.environment}} job""
    environment: ${{parameters.environment}}
    pool:
      vmImage: ${{parameters.vmImage}}        
    strategy:
      runOnce:
        deploy:
          steps:
          - task: DownloadBuildArtifacts@0
            displayName: 'Download the build artifacts'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: 'drop'
              downloadPath: '$(build.artifactstagingdirectory)'

#  - deployment: DeployAppInsightsAnnotation
#    displayName: ""Deploy application insights annotation to ${{parameters.environment}} job""
#    environment: ${{parameters.environment}}
#    dependsOn: DeployInfrastructure
#    pool:
#      vmImage: ${{parameters.vmImage}}        
#    strategy:
#      runOnce:
#        deploy:
#          steps:
#          - task: ms-appinsights.appinsightsreleaseannotations.release-task.ms-appinsights.ReleaseAnnotation@1
#            displayName: 'Add release annotation to Azure Application Insights'
#            inputs:
#              applicationId: '${{parameters.applicationInsightsApplicationId}}'
#              apiKey: '${{parameters.applicationInsightsApiKey}}'

  - deployment: DeployTests3
    displayName: ""Deploy functional tests to ${{parameters.environment}} job""
    environment: ${{parameters.environment}}
    dependsOn: 
    - DeployTests1
    - DeployTests2
    pool:
      vmImage: ${{parameters.vmImage}}        
    strategy:
      runOnce:
        deploy:
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yetNote: Azure DevOps job environment does not have an equivalent in GitHub Actions yet
#Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yetNote: Azure DevOps job environment does not have an equivalent in GitHub Actions yet
#Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yetNote: Azure DevOps job environment does not have an equivalent in GitHub Actions yet
env:
  applicationInsightsApiKey: ${{ env.ApplicationInsights--APIKeyDev }}
  applicationInsightsApplicationId: ${{ env.ApplicationInsights--ApplicationIdDev }}
  applicationInsightsInstrumentationKey: ${{ env.ApplicationInsights--InstrumentationKeyDev }}
  applicationInsightsLocation: East US
  appServiceContributerClientSecret: ${{ env.appServiceContributerClientSecret }}
  ASPNETCOREEnvironmentSetting: Development
  captureStartErrors: true
  cognitiveServicesSubscriptionKey: ${{ env.cognitiveServicesSubscriptionKey }}
  environment: ${{ env.prUC }}
  environmentLowercase: ${{ env.prLC }}
  databaseLoginName: ${{ env.databaseLoginNameDev }}
  databaseLoginPassword: ${{ env.databaseLoginPasswordDev }}
  databaseServerName: myapp-${{ env.prLC }}-eu-sqlserver
  godaddy_key: ${{ env.GoDaddyAPIKey }}
  godaddy_secret: ${{ env.GoDaddyAPISecret }}
  keyVaultClientId: ${{ env.KeyVaultClientId }}
  keyVaultClientSecret: ${{ env.KeyVaultClientSecret }}
  imagesStorageCDNURL: https://myapp-${{ env.prLC }}-eu-cdnendpoint.azureedge.net/
  imagesStorageURL: https://myapp${{ env.prLC }}eustorage.blob.core.windows.net/
  redisCacheConnectionString: ${{ env.AppSettings--RedisCacheConnectionStringDev }}
  resourceGroupName: myapp${{ env.prUC }}
  resourceGroupLocation: East US
  resourceGroupLocationShort: eu
  myappConnectionString: ${{ env.ConnectionStrings--myappConnectionStringDev }}
  serviceName: myapp-${{ env.prLC }}-eu-service
  serviceStagingUrl: https://myapp-${{ env.prLC }}-eu-service-staging.azurewebsites.net/
  serviceUrl: https://myapp-${{ env.prLC }}-eu-service.azurewebsites.net/
  storageAccountName: myapp${{ env.prLC }}eustorage
  storageAccountKey: ${{ env.StorageAccountKeyProd }}
  userPrincipalLogin: ${{ env.userPrincipalLogin }}
  vmImage: ${{ env.vmImage }}
  websiteName: myapp-${{ env.prLC }}-eu-web
  websiteDomainName: ${{ env.prLC }}.myapp.com
  websiteStagingUrl: https://myapp-${{ env.prLC }}-eu-web-staging.azurewebsites.net/
  websiteUrl: https://myapp-${{ env.prLC }}-eu-web.azurewebsites.net/
jobs:
  DeployTests1:
    # 'Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yetNote: Azure DevOps job environment does not have an equivalent in GitHub Actions yet'
    name: Deploy functional tests to ${{ env.environment }} job
    runs-on: ${{ env.vmImage }}
    steps:
    - name: Download the build artifacts
      uses: actions/download-artifact@v1.0.0
      with:
        name: drop
  DeployTests2:
    # 'Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yetNote: Azure DevOps job environment does not have an equivalent in GitHub Actions yet'
    name: Deploy functional tests to ${{ env.environment }} job
    runs-on: ${{ env.vmImage }}
    steps:
    - name: Download the build artifacts
      uses: actions/download-artifact@v1.0.0
      with:
        name: drop
  DeployTests3:
    # 'Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yetNote: Azure DevOps job environment does not have an equivalent in GitHub Actions yet'
    name: Deploy functional tests to ${{ env.environment }} job
    runs-on: ${{ env.vmImage }}
    needs:
    - DeployTests1
    - DeployTests2
    steps:
    - name: Download the build artifacts
      uses: actions/download-artifact@v1.0.0
      with:
        name: drop
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
        }

        [TestMethod]
        public void TADeploymentPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/149
            string yaml = @"
name: PRBuild_$(Year:yy)$(DayOfYear)$(Rev:.r)
trigger: none

stages:
- stage: Stage
  jobs:
  - job: ""Job""
    workspace:
      clean: all
    pool:
      name: PoolName
    steps:
    - task: PowerShell@1
      displayName: 'Script'
      inputs:
        scriptName: 'Script.ps1'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: Error! This step does not have a conversion path yet: PowerShell@1
name: PRBuild_${{ env.Year:yy }}${{ env.DayOfYear }}${{ env.Rev:.r }}
on:
  push:
    branches:
    - none
jobs:
  Stage_Stage_Job:
    runs-on: PoolName
    steps:
    - uses: actions/checkout@v2
    - # 'Note: Error! This step does not have a conversion path yet: PowerShell@1'
      name: Script
      run: 'Write-Host Note: Error! This step does not have a conversion path yet: PowerShell@1 #task: PowerShell@1#displayName: Script#inputs:#  scriptname: Script.ps1'
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
        }


        [TestMethod]
        public void JLTestPipeline()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
  branches:
    include:
    - master
  paths:
    include:
    - source/*
  tags:
    include:
    - ""v*""
    exclude:
    - ""*-*""
variables:
  buildFolderName: output
  buildArtifactName: output
  testResultFolderName: testResults
  testArtifactName: testResults

stages:
  - stage: Build
    jobs:
      - job: Package_Module
        displayName: 'Package Module'
        pool:
          vmImage: 'ubuntu 16.04'
        steps:
          - task: GitVersion@5
            name: gitVersion
            displayName: 'Evaluate Next Version'
            inputs:
              runtime: 'core'
              configFilePath: 'GitVersion.yml'
          - task: PowerShell@2
            name: package
            displayName: 'Build & Package Module'
            inputs:
              filePath: './build.ps1'
              arguments: '-ResolveDependency -tasks pack'
              pwsh: true
            env:
              ModuleVersion: $(gitVersion.NuGetVersionV2)
          - task: PublishBuildArtifacts@1
            displayName: 'Publish Build Artifact'
            inputs:
              pathToPublish: '$(buildFolderName)/'
              artifactName: $(buildArtifactName)
              publishLocation: 'Container'

  - stage: Test
    dependsOn: Build
    jobs:
      - job: Test_HQRM
        displayName: 'HQRM'
        pool:
          vmImage: 'windows-2019'
        timeoutInMinutes: 0
        steps:
          - task: DownloadBuildArtifacts@0
            displayName: 'Download Build Artifact'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: 'output'
              downloadPath: '$(Build.SourcesDirectory)'
          - task: PowerShell@2
            name: test
            displayName: 'Run HQRM Test'
            inputs:
              filePath: './build.ps1'
              arguments: '-Tasks hqrmtest'
              pwsh: false
          - task: PublishTestResults@2
            displayName: 'Publish Test Results'
            condition: succeededOrFailed()
            inputs:
              testResultsFormat: 'NUnit'
              testResultsFiles: 'output/testResults/NUnit*.xml'
              testRunTitle: 'HQRM'

      - job: Test_Unit
        displayName: 'Unit'
        pool:
          vmImage: 'windows-2019'
        timeoutInMinutes: 0
        steps:
          - task: DownloadBuildArtifacts@0
            displayName: 'Download Build Artifact'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: $(buildArtifactName)
              downloadPath: '$(Build.SourcesDirectory)'
          - task: PowerShell@2
            name: test
            displayName: 'Run Unit Test'
            inputs:
              filePath: './build.ps1'
              arguments: ""-Tasks test -PesterScript 'tests/Unit'""
              pwsh: true
          - task: PublishTestResults@2
            displayName: 'Publish Test Results'
            condition: succeededOrFailed()
            inputs:
              testResultsFormat: 'NUnit'
              testResultsFiles: '$(buildFolderName)/$(testResultFolderName)/NUnit*.xml'
              testRunTitle: 'Unit (Windows Server Core)'
          - task: PublishBuildArtifacts@1
            displayName: 'Publish Test Artifact'
            inputs:
              pathToPublish: '$(buildFolderName)/$(testResultFolderName)/'
              artifactName: $(testArtifactName)
              publishLocation: 'Container'

      - job: Test_Integration_SQL2016
        displayName: 'Integration (SQL2016)'
        pool:
          vmImage: 'windows-2019'
        timeoutInMinutes: 0
        variables:
          # This sets environment variable $env:CI.
          CI: true
          # This sets environment variable $env:CONFIGURATION.
          configuration: Integration_SQL2016
        steps:
          - task: DownloadBuildArtifacts@0
            displayName: 'Download Build Artifact'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: $(buildArtifactName)
              downloadPath: '$(Build.SourcesDirectory)'
          - task: PowerShell@2
            name: configureWinRM
            displayName: 'Configure WinRM'
            inputs:
              targetType: 'inline'
              script: 'winrm quickconfig -quiet'
              pwsh: false
          - powershell: |
              ./build.ps1 -Tasks test -CodeCoverageThreshold 0 -PesterScript @(
                  # Run the integration tests in a specific group order.
                  # Group 1
                  'tests/Integration/DSC_SqlSetup.Integration.Tests.ps1'
                  # Group 2
                  'tests/Integration/DSC_SqlAgentAlert.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlServerNetwork.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlLogin.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlEndpoint.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabaseMail.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlRSSetup.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabaseDefaultLocation.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabase.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlAlwaysOnService.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlAgentOperator.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlServiceAccount.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlAgentFailsafe.Integration.Tests.ps1'
                  # Group 3
                  'tests/Integration/DSC_SqlRole.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlRS.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabaseUser.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlReplication.Integration.Tests.ps1'
                  # Group 4
                  'tests/Integration/DSC_SqlScript.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabasePermission.Integration.Tests.ps1'
                  # Group 5
                  'tests/Integration/DSC_SqlSecureConnection.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlScriptQuery.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlProtocol.Integration.Tests.ps1'
                  # Group 6 (tests makes changes that could make SQL Server to loose connectivity)
                  'tests/Integration/DSC_SqlProtocolTcpIp.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabaseObjectPermission.Integration.Tests.ps1'
              )
            name: test
            displayName: 'Run Integration Test'
          - task: PublishTestResults@2
            displayName: 'Publish Test Results'
            condition: succeededOrFailed()
            inputs:
              testResultsFormat: 'NUnit'
              testResultsFiles: '$(buildFolderName)/$(testResultFolderName)/NUnit*.xml'
              testRunTitle: 'Integration (SQL Server 2016 / Windows Server 2019)'

      - job: Test_Integration_SQL2017
        displayName: 'Integration (SQL2017)'
        pool:
          vmImage: 'windows-2019'
        timeoutInMinutes: 0
        variables:
          # This sets environment variable $env:CI.
          CI: true
          # This sets environment variable $env:CONFIGURATION.
          configuration: Integration_SQL2017
        steps:
          - task: DownloadBuildArtifacts@0
            displayName: 'Download Build Artifact'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: $(buildArtifactName)
              downloadPath: '$(Build.SourcesDirectory)'
          - task: PowerShell@2
            name: configureWinRM
            displayName: 'Configure WinRM'
            inputs:
              targetType: 'inline'
              script: 'winrm quickconfig -quiet'
              pwsh: false
          - powershell: |
              ./build.ps1 -Tasks test -CodeCoverageThreshold 0 -PesterScript @(
                  # Run the integration tests in a specific group order.
                  # Group 1
                  'tests/Integration/DSC_SqlSetup.Integration.Tests.ps1'
                  # Group 2
                  'tests/Integration/DSC_SqlAgentAlert.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlLogin.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlEndpoint.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabaseMail.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlRSSetup.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabaseDefaultLocation.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabase.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlAlwaysOnService.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlAgentOperator.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlServiceAccount.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlAgentFailsafe.Integration.Tests.ps1'
                  # Group 3
                  'tests/Integration/DSC_SqlRole.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlRS.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabaseUser.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlReplication.Integration.Tests.ps1'
                  # Group 4
                  'tests/Integration/DSC_SqlScript.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabasePermission.Integration.Tests.ps1'
                  # Group 5
                  'tests/Integration/DSC_SqlSecureConnection.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlScriptQuery.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlProtocol.Integration.Tests.ps1'
                  # Group 6 (tests makes changes that could make SQL Server to loose connectivity)
                  'tests/Integration/DSC_SqlProtocolTcpIp.Integration.Tests.ps1'
                  'tests/Integration/DSC_SqlDatabaseObjectPermission.Integration.Tests.ps1'
              )
            name: test
            displayName: 'Run Integration Test'
          - task: PublishTestResults@2
            displayName: 'Publish Test Results'
            condition: succeededOrFailed()
            inputs:
              testResultsFormat: 'NUnit'
              testResultsFiles: '$(buildFolderName)/$(testResultFolderName)/NUnit*.xml'
              testRunTitle: 'Integration (Windows Server Core)'

      - job: Code_Coverage
        displayName: 'Publish Code Coverage'
        dependsOn: Test_Unit
        pool:
          vmImage: 'ubuntu 16.04'
        timeoutInMinutes: 0
        steps:
          - pwsh: |
              $repositoryOwner,$repositoryName = $env:BUILD_REPOSITORY_NAME -split '/'
              echo ""##vso[task.setvariable variable=RepositoryOwner;isOutput=true]$repositoryOwner""
              echo ""##vso[task.setvariable variable=RepositoryName;isOutput=true]$repositoryName""
            name: dscBuildVariable
            displayName: 'Set Environment Variables'
          - task: DownloadBuildArtifacts@0
            displayName: 'Download Build Artifact'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: $(buildArtifactName)
              downloadPath: '$(Build.SourcesDirectory)'
          - task: DownloadBuildArtifacts@0
            displayName: 'Download Test Artifact'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: $(testArtifactName)
              downloadPath: '$(Build.SourcesDirectory)/$(buildFolderName)'
          - task: PublishCodeCoverageResults@1
            displayName: 'Publish Azure Code Coverage'
            condition: succeededOrFailed()
            inputs:
              codeCoverageTool: 'JaCoCo'
              summaryFileLocation: '$(buildFolderName)/$(testResultFolderName)/JaCoCo_coverage.xml'
              pathToSources: '$(Build.SourcesDirectory)/$(buildFolderName)/$(dscBuildVariable.RepositoryName)'
          - script: |
              bash <(curl -s https://codecov.io/bash) -f ""./$(buildFolderName)/$(testResultFolderName)/JaCoCo_coverage.xml"" -F unit
            displayName: 'Upload to Codecov.io'
            condition: succeededOrFailed()

  - stage: Deploy
    dependsOn: Test
    condition: |
      and(
        succeeded(),
        or(
          eq(variables['Build.SourceBranch'], 'refs/heads/master'),
          startsWith(variables['Build.SourceBranch'], 'refs/tags/')
        ),
        contains(variables['System.TeamFoundationCollectionUri'], 'dsccommunity')
      )
    jobs:
      - job: Deploy_Module
        displayName: 'Deploy Module'
        pool:
          vmImage: 'ubuntu 16.04'
        steps:
          - task: DownloadBuildArtifacts@0
            displayName: 'Download Build Artifact'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: $(buildArtifactName)
              downloadPath: '$(Build.SourcesDirectory)'
          - task: PowerShell@2
            name: publishRelease
            displayName: 'Publish Release'
            inputs:
              filePath: './build.ps1'
              arguments: '-tasks publish'
              pwsh: true
            env:
              GitHubToken: $(GitHubToken)
              GalleryApiToken: $(GalleryApiToken)
          - task: PowerShell@2
            name: sendChangelogPR
            displayName: 'Send Changelog PR'
            inputs:
              filePath: './build.ps1'
              arguments: '-tasks Create_ChangeLog_GitHub_PR'
              pwsh: true
            env:
              GitHubToken: $(GitHubToken)
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: Error! This step does not have a conversion path yet: PublishCodeCoverageResults@1
#PublishTestResults@2 is a Azure DevOps specific task. There is no equivalent in GitHub Actions until there is a testing summary tab. See: https://github.community/t/publishing-test-results/16215
#PublishTestResults@2 is a Azure DevOps specific task. There is no equivalent in GitHub Actions until there is a testing summary tab. See: https://github.community/t/publishing-test-results/16215
#PublishTestResults@2 is a Azure DevOps specific task. There is no equivalent in GitHub Actions until there is a testing summary tab. See: https://github.community/t/publishing-test-results/16215
#PublishTestResults@2 is a Azure DevOps specific task. There is no equivalent in GitHub Actions until there is a testing summary tab. See: https://github.community/t/publishing-test-results/16215
#Note: Error! This step does not have a conversion path yet: GitVersion@5
on:
  push:
    branches:
    - master
    paths:
    - source/*
    tags:
    - v*
    tags-ignore:
    - '*-*'
env:
  buildFolderName: output
  buildArtifactName: output
  testResultFolderName: testResults
  testArtifactName: testResults
jobs:
  Build_Stage_Package_Module:
    name: Package Module
    runs-on: ubuntu 16.04
    steps:
    - uses: actions/checkout@v2
    - # 'Note: Error! This step does not have a conversion path yet: GitVersion@5'
      name: Evaluate Next Version
      run: 'Write-Host Note: Error! This step does not have a conversion path yet: GitVersion@5 #task: GitVersion@5#displayName: Evaluate Next Version#name: gitVersion#inputs:#  runtime: core#  configfilepath: GitVersion.yml'
      shell: powershell
    - name: Build & Package Module
      shell: powershell
    - name: Publish Build Artifact
      uses: actions/upload-artifact@v2
      with:
        path: ${{ env.buildFolderName }}/
        name: ${{ env.buildArtifactName }}
  Test_Stage_Test_HQRM:
    name: HQRM
    runs-on: windows-2019
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: output
    - name: Run HQRM Test
      shell: powershell
    - # 'PublishTestResults@2 is a Azure DevOps specific task. There is no equivalent in GitHub Actions until there is a testing summary tab. See: https://github.community/t/publishing-test-results/16215'
      name: Publish Test Results
      run: echo ""This task equivalent does not yet exist in GitHub Actions""
      if: ne(${{ job.status }}, 'cancelled')
  Test_Stage_Test_Unit:
    name: Unit
    runs-on: windows-2019
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: ${{ env.buildArtifactName }}
    - name: Run Unit Test
      shell: powershell
    - # 'PublishTestResults@2 is a Azure DevOps specific task. There is no equivalent in GitHub Actions until there is a testing summary tab. See: https://github.community/t/publishing-test-results/16215'
      name: Publish Test Results
      run: echo ""This task equivalent does not yet exist in GitHub Actions""
      if: ne(${{ job.status }}, 'cancelled')
    - name: Publish Test Artifact
      uses: actions/upload-artifact@v2
      with:
        path: ${{ env.buildFolderName }}/${{ env.testResultFolderName }}/
        name: ${{ env.testArtifactName }}
  Test_Stage_Test_Integration_SQL2016:
    name: Integration (SQL2016)
    runs-on: windows-2019
    env:
      CI: true
      configuration: Integration_SQL2016
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: ${{ env.buildArtifactName }}
    - name: Configure WinRM
      run: winrm quickconfig -quiet
      shell: powershell
    - name: Run Integration Test
      run: |
        ./build.ps1 -Tasks test -CodeCoverageThreshold 0 -PesterScript @(
            # Run the integration tests in a specific group order.
            # Group 1
            'tests/Integration/DSC_SqlSetup.Integration.Tests.ps1'
            # Group 2
            'tests/Integration/DSC_SqlAgentAlert.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlServerNetwork.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlLogin.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlEndpoint.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabaseMail.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlRSSetup.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabaseDefaultLocation.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabase.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlAlwaysOnService.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlAgentOperator.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlServiceAccount.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlAgentFailsafe.Integration.Tests.ps1'
            # Group 3
            'tests/Integration/DSC_SqlRole.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlRS.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabaseUser.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlReplication.Integration.Tests.ps1'
            # Group 4
            'tests/Integration/DSC_SqlScript.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabasePermission.Integration.Tests.ps1'
            # Group 5
            'tests/Integration/DSC_SqlSecureConnection.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlScriptQuery.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlProtocol.Integration.Tests.ps1'
            # Group 6 (tests makes changes that could make SQL Server to loose connectivity)
            'tests/Integration/DSC_SqlProtocolTcpIp.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabaseObjectPermission.Integration.Tests.ps1'
        )
      shell: powershell
    - # 'PublishTestResults@2 is a Azure DevOps specific task. There is no equivalent in GitHub Actions until there is a testing summary tab. See: https://github.community/t/publishing-test-results/16215'
      name: Publish Test Results
      run: echo ""This task equivalent does not yet exist in GitHub Actions""
      if: ne(${{ job.status }}, 'cancelled')
  Test_Stage_Test_Integration_SQL2017:
    name: Integration (SQL2017)
    runs-on: windows-2019
    env:
      CI: true
      configuration: Integration_SQL2017
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: ${{ env.buildArtifactName }}
    - name: Configure WinRM
      run: winrm quickconfig -quiet
      shell: powershell
    - name: Run Integration Test
      run: |
        ./build.ps1 -Tasks test -CodeCoverageThreshold 0 -PesterScript @(
            # Run the integration tests in a specific group order.
            # Group 1
            'tests/Integration/DSC_SqlSetup.Integration.Tests.ps1'
            # Group 2
            'tests/Integration/DSC_SqlAgentAlert.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlLogin.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlEndpoint.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabaseMail.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlRSSetup.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabaseDefaultLocation.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabase.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlAlwaysOnService.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlAgentOperator.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlServiceAccount.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlAgentFailsafe.Integration.Tests.ps1'
            # Group 3
            'tests/Integration/DSC_SqlRole.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlRS.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabaseUser.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlReplication.Integration.Tests.ps1'
            # Group 4
            'tests/Integration/DSC_SqlScript.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabasePermission.Integration.Tests.ps1'
            # Group 5
            'tests/Integration/DSC_SqlSecureConnection.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlScriptQuery.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlProtocol.Integration.Tests.ps1'
            # Group 6 (tests makes changes that could make SQL Server to loose connectivity)
            'tests/Integration/DSC_SqlProtocolTcpIp.Integration.Tests.ps1'
            'tests/Integration/DSC_SqlDatabaseObjectPermission.Integration.Tests.ps1'
        )
      shell: powershell
    - # 'PublishTestResults@2 is a Azure DevOps specific task. There is no equivalent in GitHub Actions until there is a testing summary tab. See: https://github.community/t/publishing-test-results/16215'
      name: Publish Test Results
      run: echo ""This task equivalent does not yet exist in GitHub Actions""
      if: ne(${{ job.status }}, 'cancelled')
  Test_Stage_Code_Coverage:
    name: Publish Code Coverage
    runs-on: ubuntu 16.04
    needs:
    - Test_Unit
    steps:
    - uses: actions/checkout@v2
    - name: Set Environment Variables
      run: |
        $repositoryOwner,$repositoryName = $env:BUILD_REPOSITORY_NAME -split '/'
        echo ""##vso[task.setvariable variable=RepositoryOwner;isOutput=true]$repositoryOwner""
        echo ""##vso[task.setvariable variable=RepositoryName;isOutput=true]$repositoryName""
      shell: pwsh
    - name: Download Build Artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: ${{ env.buildArtifactName }}
    - name: Download Test Artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: ${{ env.testArtifactName }}
    - # 'Note: Error! This step does not have a conversion path yet: PublishCodeCoverageResults@1'
      name: Publish Azure Code Coverage
      run: 'Write-Host Note: Error! This step does not have a conversion path yet: PublishCodeCoverageResults@1 #task: PublishCodeCoverageResults@1#displayName: Publish Azure Code Coverage#condition: succeededOrFailed()#inputs:#  codecoveragetool: JaCoCo#  summaryfilelocation: ${{ env.buildFolderName }}/${{ env.testResultFolderName }}/JaCoCo_coverage.xml#  pathtosources: ${{ env.Build.SourcesDirectory }}/${{ env.buildFolderName }}/${{ env.dscBuildVariable.RepositoryName }}'
      shell: powershell
      if: ne(${{ job.status }}, 'cancelled')
    - name: Upload to Codecov.io
      run: bash <(curl -s https://codecov.io/bash) -f ""./${{ env.buildFolderName }}/${{ env.testResultFolderName }}/JaCoCo_coverage.xml"" -F unit
      if: ne(${{ job.status }}, 'cancelled')
  Deploy_Stage_Deploy_Module:
    name: Deploy Module
    runs-on: ubuntu 16.04
    if: and(success(),or(eq(github.ref, 'refs/heads/master'),startsWith(github.ref, 'refs/tags/')),contains(variables['System.TeamFoundationCollectionUri'], 'dsccommunity'))
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v1.0.0
      with:
        name: ${{ env.buildArtifactName }}
    - name: Publish Release
      shell: powershell
    - name: Send Changelog PR
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
        }

//        //Test that the result includes the setup Java step
//        [TestMethod]
//        public void DeployKubernatesClusterPipelineTest()
//        {
//            //Arrange
//            Conversion conversion = new Conversion();
//            string yaml = @"
//# Deploy to Azure Kubernetes Service
//# Build and push image to Azure Container Registry; Deploy to Azure Kubernetes Service
//# https://docs.microsoft.com/azure/devops/pipelines/languages/docker

//trigger:
//- master

//stages:

//- stage: Deploy
//  displayName: Deploy stage
//  dependsOn: Build
//  jobs:
//{{#if reviewApp}}
//  - deployment: DeployPullRequest
//    displayName: Deploy Pull request
//    condition: and(succeeded(), startsWith(variables['Build.SourceBranch'], 'refs/pull/'))
//    pool:
//      vmImage: $(vmImageName)
      
//    environment: '{{ k8sResource.EnvironmentReference.Name }}.$(k8sNamespaceForPR)'
//    strategy:
//      runOnce:
//        deploy:
//          steps:
//          - reviewApp: {{ k8sResource.Name }}

//          - task: Kubernetes@1
//            displayName: 'Create a new namespace for the pull request'
//            inputs:
//              command: apply
//              useConfigurationFile: true
//              inline: '{ ""kind"": ""Namespace"", ""apiVersion"": ""v1"", ""metadata"": { ""name"": ""$(k8sNamespaceForPR)"" }}'

//          - task: KubernetesManifest@0
//            displayName: Create imagePullSecret
//            inputs:
//              action: createSecret
//              secretName: $(imagePullSecret)
//              namespace: $(k8sNamespaceForPR)
//              dockerRegistryEndpoint: $(dockerRegistryServiceConnection)
          
//          - task: KubernetesManifest@0
//            displayName: Deploy to the new namespace in the Kubernetes cluster
//            inputs:
//              action: deploy
//              namespace: $(k8sNamespaceForPR)
//              manifests: |
//                $(Pipeline.Workspace)/manifests/deployment.yml
//                $(Pipeline.Workspace)/manifests/service.yml
//              imagePullSecrets: |
//                $(imagePullSecret)
//              containers: |
//                $(containerRegistry)/$(imageRepository):$(tag)
          
//          - task: Kubernetes@1
//            name: get
//            displayName: 'Get services in the new namespace'
//            continueOnError: true
//            inputs:
//              command: get
//              namespace: $(k8sNamespaceForPR)
//              arguments: svc
//              outputFormat: jsonpath='http://{.items[0].status.loadBalancer.ingress[0].ip}:{.items[0].spec.ports[0].port}'
              
//          # Getting the IP of the deployed service and writing it to a variable for posing comment
//          - script: |
//              url=""$(get.KubectlOutput)""
//              message=""Your review app has been deployed""
//              if [ ! -z ""$url"" -a ""$url"" != ""http://:"" ] 
//              then
//                message=""${message} and is available at $url.<br><br>[Learn More](https://aka.ms/testwithreviewapps) about how to test and provide feedback for the app.""
//              fi
//              echo ""##vso[task.setvariable variable=GITHUB_COMMENT]$message""
//{{/if}}

//";

//            //Act
//            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

//            //Assert
//            string expected = @"

//";

//            expected = UtilityTests.TrimNewLines(expected);
//            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
//            Assert.AreEqual(true, gitHubOutput.v2ConversionSuccessful);
//        }

    }
}