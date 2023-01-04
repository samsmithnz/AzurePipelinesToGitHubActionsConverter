using AzurePipelinesToGitHubActionsConverter.Core;
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
#Note: the 'AZURE_SP' secret is required to be added into GitHub Secrets. See this blog post for details: https://samlearnsazure.blog/2019/12/13/github-actions/
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
    - # ""Note: the 'AZURE_SP' secret is required to be added into GitHub Secrets. See this blog post for details: https://samlearnsazure.blog/2019/12/13/github-actions/""
      name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_SP }}
    - name: Download the build artifacts
      uses: actions/download-artifact@v2
      with:
        name: drop
        path: ${{ github.workspace }}
    - name: Deploy ARM Template to resource group
      uses: Azure/cli@v1.0.0
      with:
        inlineScript: az deployment group create --resource-group ${{ env.ResourceGroupName }} --template-file ${{ github.workspace }}/drop/ARMTemplates/azuredeploy.json --parameters  ${{ github.workspace }}/drop/ARMTemplates/azuredeploy.parameters.json -environment ${{ env.AppSettings.Environment }} -locationShort ${{ env.ArmTemplateResourceGroupLocation }}
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
- main

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
    - main
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
- main

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
    - main
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
- main

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
on:
  push:
    branches:
    - main
env:
  solution: WindowsFormsApp1.sln
  buildPlatform: Any CPU
  buildConfiguration: Release
jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - uses: microsoft/setup-msbuild@v1.0.2
    - uses: nuget/setup-nuget@v1
    - run: nuget restore ${{ env.solution }}
    - run: msbuild '${{ env.solution }}' /p:configuration='${{ env.buildConfiguration }}' /p:platform='${{ env.buildPlatform }}'
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

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
- main

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
    - main
env:
  GOBIN: ${{ env.GOPATH }}/bin
  GOROOT: /usr/local/go1.11
  GOPATH: ${{ github.workspace }}/gopath
  modulePath: ${{ env.GOPATH }}/src/github.com/${{ github.repository }}
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
- main

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
    - main
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
- main

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
    - main
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
  - endpoint: endpointA

trigger:
- main

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
    - main
env:
  BuildConfiguration: Release
  BuildPlatform: Any CPU
  BuildVersion: 1.1.${{ github.run_id }}
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
      run: dotnet publish MyProject/MyProject.Models/MyProject.Models.csproj --configuration ${{ env.BuildConfiguration }} --output ${{ github.workspace }}
    - name: dotnet pack
      run: dotnet pack MyProject/MyProject.Models/MyProject.Models.csproj
    - name: Publish Artifact
      uses: actions/upload-artifact@v2
      with:
        path: ${{ github.workspace }}
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
- main

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
    - main
jobs:
  build:
    strategy:
      matrix:
        PYTHON_VERSION:
        - 3.5
        - 3.6
        - 3.7
      max-parallel: 3
    runs-on: ubuntu-latest
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


        [TestMethod]
        public void ResourcesContainersPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- main

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
    - main
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
- main

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
    - main
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

        [TestMethod]
        public void TestHTMLPipeline()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
# HTML
# Archive your static HTML project and save it with the build record.

trigger:
- main

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
#Note: This is a third party action and currently only supports Linux: https://github.com/marketplace/actions/create-zip-file
on:
  push:
    branches:
    - main
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - # 'Note: This is a third party action and currently only supports Linux: https://github.com/marketplace/actions/create-zip-file'
      uses: montudor/action-zip@v0.1.0
      with:
        args: zip -qq -r  ${{ github.workspace }}
    - uses: actions/upload-artifact@v2
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
- main
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
    - main
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
- main

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
on:
  push:
    branches:
    - main
env:
  buildConfiguration: Release
  outputDirectory: ${{ env.build.binariesDirectory }}/${{ env.buildConfiguration }}
jobs:
  build:
    runs-on: macos-latest
    steps:
    - uses: actions/checkout@v2
    - uses: nuget/setup-nuget@v1
    - run: nuget restore **/*.sln
    - run: |
        cd Blank
        nuget restore
        cd Blank.Android
        msbuild **/*droid*.csproj /verbosity:normal /t:Rebuild /p:Configuration=${{ env.buildConfiguration }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

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
- main

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
on:
  push:
    branches:
    - main
jobs:
  build:
    runs-on: macos-latest
    steps:
    - uses: actions/checkout@v2
    - name: Select the Xamarin SDK version
      run: sudo $AGENT_HOMEDIRECTORY/scripts/select-xamarin-sdk.sh 5_12_0
    - uses: nuget/setup-nuget@v1
    - run: nuget restore **/*.sln
    - run: |
        cd Blank
        nuget restore
        cd Blank.Android
        msbuild  /verbosity:normal /t:Rebuild /p:Platform=iPhoneSimulator /p:Configuration=Release
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

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
#There is no conversion path for templates in GitHub Actions
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
    - # There is no conversion path for templates in GitHub Actions
      run: |
        #templates/npm-build-steps.yaml
        extensionName: ${{ env.ExtensionName }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }


        [TestMethod]
        public void JRPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            //Source is: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/128
            string yaml = @"
trigger:
- main

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
#Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps
#Error (line 37): the step 'IISWebAppManagementOnMachineGroup@0' does not have a conversion path yet
#Error (line 59): the step 'IISWebAppDeploymentOnMachineGroup@0' does not have a conversion path yet
on:
  push:
    branches:
    - main
env:
  solution: '**/*.sln'
  buildPlatform: Any CPU
  buildConfiguration: Release
jobs:
  Build_Stage_BuildSpark:
    runs-on: Pipeline-Demo-Windows
    steps:
    - uses: actions/checkout@v2
    - uses: microsoft/setup-msbuild@v1.0.2
    - uses: nuget/setup-nuget@v1
    - run: nuget restore ${{ env.solution }}
    - run: msbuild '${{ env.solution }}' /p:configuration='${{ env.buildConfiguration }}' /p:platform='${{ env.buildPlatform }}' /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:PackageLocation=""${{ github.workspace }}""
    - uses: actions/upload-artifact@v2
      with:
        path: ${{ github.workspace }}
  Deploy_Stage_job1:
    # 'Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps'
    environment:
      name: windows-server
    env:
      Art: Server=.;Database=Art;Trusted_Connection=True;
    steps:
    - uses: actions/download-artifact@v2
    - run: |
        echo Write your commands here

        DIR
      shell: cmd
    - # ""Error: the step 'IISWebAppManagementOnMachineGroup@0' does not have a conversion path yet""
      run: |
        echo ""Error: the step 'IISWebAppManagementOnMachineGroup@0' does not have a conversion path yet""
        #task: IISWebAppManagementOnMachineGroup@0
        #inputs:
        #  iisdeploymenttype: IISWebsite
        #  actioniiswebsite: CreateOrUpdateWebsite
        #  websitename: Spark
        #  websitephysicalpath: '%SystemDrive%\inetpub\wwwroot'
        #  websitephysicalpathauth: WebsiteUserPassThrough
        #  addbinding: true
        #  createorupdateapppoolforwebsite: true
        #  configureauthenticationforwebsite: true
        #  apppoolnameforwebsite: Spark
        #  dotnetversionforwebsite: v4.0
        #  pipelinemodeforwebsite: Integrated
        #  apppoolidentityforwebsite: ApplicationPoolIdentity
        #  anonymousauthenticationforwebsite: true
        #  windowsauthenticationforwebsite: false
        #  protocol: http
        #  ipaddress: All Unassigned
        #  port: 80
    - # ""Error: the step 'IISWebAppDeploymentOnMachineGroup@0' does not have a conversion path yet""
      run: |
        echo ""Error: the step 'IISWebAppDeploymentOnMachineGroup@0' does not have a conversion path yet""
        #task: IISWebAppDeploymentOnMachineGroup@0
        #inputs:
        #  websitename: Spark
        #  package: ${{ env.Pipeline.Workspace }}\Art.Web.zip
        #  xmlvariablesubstitution: true
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

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
    ${{ if ne(variables['Build.SourceBranchName'], 'main') }}:
      prId: ""$(System.PullRequest.PullRequestId)""
    ${{ if eq(variables['Build.SourceBranchName'], 'main') }}:
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
    needs: []
    env:
      prId: 000
      prUC: PR${{ env.prId }}
      prLC: pr${{ env.prId }}
    if: (success() && (variables['Build.Reason'] == 'PullRequest') && (variables['System.PullRequest.PullRequestId'] != 'Null'))
    steps:
    - uses: actions/checkout@v2
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }


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
#Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps
#Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps
#Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps
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
    # 'Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps'
    name: Deploy functional tests to ${{ env.environment }} job
    runs-on: ${{ env.vmImage }}
    environment:
      name: ${{ env.environment }}
    steps:
    - name: Download the build artifacts
      uses: actions/download-artifact@v2
      with:
        name: drop
        path: ${{ github.workspace }}
  DeployTests2:
    # 'Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps'
    name: Deploy functional tests to ${{ env.environment }} job
    runs-on: ${{ env.vmImage }}
    environment:
      name: ${{ env.environment }}
    steps:
    - name: Download the build artifacts
      uses: actions/download-artifact@v2
      with:
        name: drop
        path: ${{ github.workspace }}
  DeployTests3:
    # 'Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps'
    name: Deploy functional tests to ${{ env.environment }} job
    runs-on: ${{ env.vmImage }}
    needs:
    - DeployTests1
    - DeployTests2
    environment:
      name: ${{ env.environment }}
    steps:
    - name: Download the build artifacts
      uses: actions/download-artifact@v2
      with:
        name: drop
        path: ${{ github.workspace }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

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
    - name: Script
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }


        [TestMethod]
        public void JLPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
  branches:
    include:
    - main
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
          eq(variables['Build.SourceBranch'], 'refs/heads/main'),
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
#Error (line 58): the step 'PublishTestResults@2' does not have a conversion path yet
#Error (line 61): the step 'PublishTestResults@2' does not have a conversion path yet
#Error (line 84): the step 'PublishTestResults@2' does not have a conversion path yet
#Error (line 87): the step 'PublishTestResults@2' does not have a conversion path yet
#Error (line 256): the step 'PublishCodeCoverageResults@1' does not have a conversion path yet
on:
  push:
    branches:
    - main
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
    - name: Install GitVersion
      uses: gittools/actions/gitversion/setup@v0.9.11
      with:
        versionSpec: 5.x
    - name: Evaluate Next Version
      uses: gittools/actions/gitversion/execute@v0.9.11
      with:
        configFilePath: GitVersion.yml
    - name: Build & Package Module
      shell: powershell
      env:
        ModuleVersion: ${{ env.gitVersion.NuGetVersionV2 }}
    - name: Publish Build Artifact
      uses: actions/upload-artifact@v2
      with:
        path: ${{ env.buildFolderName }}/
        name: ${{ env.buildArtifactName }}
  Test_Stage_Test_HQRM:
    name: HQRM
    runs-on: windows-2019
    needs:
    - Build_Stage_Package_Module
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v2
      with:
        name: output
        path: ${{ github.workspace }}
    - name: Run HQRM Test
      shell: powershell
    - # ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
      name: Publish Test Results
      run: |
        echo ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
        #task: PublishTestResults@2
        #displayName: Publish Test Results
        #condition: succeededOrFailed()
        #inputs:
        #  testresultsformat: NUnit
        #  testresultsfiles: output/testResults/NUnit*.xml
        #  testruntitle: HQRM
      if: (${{ job.status }} != 'cancelled')
  Test_Stage_Test_Unit:
    name: Unit
    runs-on: windows-2019
    needs:
    - Build_Stage_Package_Module
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v2
      with:
        name: ${{ env.buildArtifactName }}
        path: ${{ github.workspace }}
    - name: Run Unit Test
      shell: powershell
    - # ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
      name: Publish Test Results
      run: |
        echo ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
        #task: PublishTestResults@2
        #displayName: Publish Test Results
        #condition: succeededOrFailed()
        #inputs:
        #  testresultsformat: NUnit
        #  testresultsfiles: ${{ env.buildFolderName }}/${{ env.testResultFolderName }}/NUnit*.xml
        #  testruntitle: Unit (Windows Server Core)
      if: (${{ job.status }} != 'cancelled')
    - name: Publish Test Artifact
      uses: actions/upload-artifact@v2
      with:
        path: ${{ env.buildFolderName }}/${{ env.testResultFolderName }}/
        name: ${{ env.testArtifactName }}
  Test_Stage_Test_Integration_SQL2016:
    name: Integration (SQL2016)
    runs-on: windows-2019
    needs:
    - Build_Stage_Package_Module
    env:
      CI: true
      configuration: Integration_SQL2016
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v2
      with:
        name: ${{ env.buildArtifactName }}
        path: ${{ github.workspace }}
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
    - # ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
      name: Publish Test Results
      run: |
        echo ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
        #task: PublishTestResults@2
        #displayName: Publish Test Results
        #condition: succeededOrFailed()
        #inputs:
        #  testresultsformat: NUnit
        #  testresultsfiles: ${{ env.buildFolderName }}/${{ env.testResultFolderName }}/NUnit*.xml
        #  testruntitle: Integration (SQL Server 2016 / Windows Server 2019)
      if: (${{ job.status }} != 'cancelled')
  Test_Stage_Test_Integration_SQL2017:
    name: Integration (SQL2017)
    runs-on: windows-2019
    needs:
    - Build_Stage_Package_Module
    env:
      CI: true
      configuration: Integration_SQL2017
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v2
      with:
        name: ${{ env.buildArtifactName }}
        path: ${{ github.workspace }}
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
    - # ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
      name: Publish Test Results
      run: |
        echo ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
        #task: PublishTestResults@2
        #displayName: Publish Test Results
        #condition: succeededOrFailed()
        #inputs:
        #  testresultsformat: NUnit
        #  testresultsfiles: ${{ env.buildFolderName }}/${{ env.testResultFolderName }}/NUnit*.xml
        #  testruntitle: Integration (Windows Server Core)
      if: (${{ job.status }} != 'cancelled')
  Test_Stage_Code_Coverage:
    name: Publish Code Coverage
    runs-on: ubuntu 16.04
    needs:
    - Build_Stage_Package_Module
    - Test_Stage_Test_Unit
    steps:
    - uses: actions/checkout@v2
    - name: Set Environment Variables
      run: |
        $repositoryOwner,$repositoryName = $env:BUILD_REPOSITORY_NAME -split '/'
        echo ""##vso[task.setvariable variable=RepositoryOwner;isOutput=true]$repositoryOwner""
        echo ""##vso[task.setvariable variable=RepositoryName;isOutput=true]$repositoryName""
      shell: pwsh
    - name: Download Build Artifact
      uses: actions/download-artifact@v2
      with:
        name: ${{ env.buildArtifactName }}
        path: ${{ github.workspace }}
    - name: Download Test Artifact
      uses: actions/download-artifact@v2
      with:
        name: ${{ env.testArtifactName }}
        path: ${{ github.workspace }}/${{ env.buildFolderName }}
    - # ""Error: the step 'PublishCodeCoverageResults@1' does not have a conversion path yet""
      name: Publish Azure Code Coverage
      run: |
        echo ""Error: the step 'PublishCodeCoverageResults@1' does not have a conversion path yet""
        #task: PublishCodeCoverageResults@1
        #displayName: Publish Azure Code Coverage
        #condition: succeededOrFailed()
        #inputs:
        #  codecoveragetool: JaCoCo
        #  summaryfilelocation: ${{ env.buildFolderName }}/${{ env.testResultFolderName }}/JaCoCo_coverage.xml
        #  pathtosources: ${{ github.workspace }}/${{ env.buildFolderName }}/${{ env.dscBuildVariable.RepositoryName }}
      if: (${{ job.status }} != 'cancelled')
    - name: Upload to Codecov.io
      run: bash <(curl -s https://codecov.io/bash) -f ""./${{ env.buildFolderName }}/${{ env.testResultFolderName }}/JaCoCo_coverage.xml"" -F unit
      if: (${{ job.status }} != 'cancelled')
  Deploy_Stage_Deploy_Module:
    name: Deploy Module
    runs-on: ubuntu 16.04
    needs:
    - Test_Stage_Test_HQRM
    - Test_Stage_Test_Unit
    - Test_Stage_Test_Integration_SQL2016
    - Test_Stage_Test_Integration_SQL2017
    - Test_Stage_Code_Coverage
    if: (success() && ((github.ref == 'refs/heads/main') || startsWith(github.ref, 'refs/tags/')) && contains(variables['System.TeamFoundationCollectionUri'], 'dsccommunity'))
    steps:
    - uses: actions/checkout@v2
    - name: Download Build Artifact
      uses: actions/download-artifact@v2
      with:
        name: ${{ env.buildArtifactName }}
        path: ${{ github.workspace }}
    - name: Publish Release
      shell: powershell
      env:
        GitHubToken: ${{ env.GitHubToken }}
        GalleryApiToken: ${{ env.GalleryApiToken }}
    - name: Send Changelog PR
      shell: powershell
      env:
        GitHubToken: ${{ env.GitHubToken }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void CDTest()
        {
            //Arrange
            string input = @"
trigger:
- main

variables:
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'

jobs:
- job: Deploy
  displayName: ""Deploy job""
  pool:
    vmImage: ubuntu-latest
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  variables:
    AppSettings.Environment: 'data'
    ArmTemplateResourceGroupLocation: 'eu'
    ResourceGroupName: 'MyProjectRG'
    WebsiteName: 'myproject-web'
  steps:
  - task: DownloadBuildArtifacts@0
    displayName: 'Download the build artifacts'
    inputs:
      buildType: 'current'
      downloadType: 'single'
      artifactName: 'drop'
      downloadPath: '$(build.artifactstagingdirectory)'
  - task: AzureRmWebAppDeployment@3
    displayName: 'Azure App Service Deploy: web site'
    inputs:
      azureSubscription: 'connection to Azure Portal'
      WebAppName: $(WebsiteName)
      DeployToSlotFlag: true
      ResourceGroupName: $(ResourceGroupName)
      SlotName: 'staging'
      Package: '$(build.artifactstagingdirectory)/drop/MyProject.Web.zip'
      TakeAppOfflineFlag: true
      JSONFiles: '**/appsettings.json'        
  - task: AzureAppServiceManage@0
    displayName: 'Swap Slots: website'
    inputs:
      azureSubscription: 'connection to Azure Portal'
      WebAppName: $(WebsiteName)
      ResourceGroupName: $(ResourceGroupName)
      SourceSlot: 'staging'";
            bool addWorkFlowDispatch = true;
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input, addWorkFlowDispatch);

            //Assert
            string expected = @"
#Note: the 'AZURE_SP' secret is required to be added into GitHub Secrets. See this blog post for details: https://samlearnsazure.blog/2019/12/13/github-actions/
on:
  push:
    branches:
    - main
  workflow_dispatch:
env:
  buildConfiguration: Release
  buildPlatform: Any CPU
jobs:
  Deploy:
    name: Deploy job
    runs-on: ubuntu-latest
    env:
      AppSettings.Environment: data
      ArmTemplateResourceGroupLocation: eu
      ResourceGroupName: MyProjectRG
      WebsiteName: myproject-web
    if: (success() && (github.ref == 'refs/heads/main'))
    steps:
    - uses: actions/checkout@v2
    - # ""Note: the 'AZURE_SP' secret is required to be added into GitHub Secrets. See this blog post for details: https://samlearnsazure.blog/2019/12/13/github-actions/""
      name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_SP }}
    - name: Download the build artifacts
      uses: actions/download-artifact@v2
      with:
        name: drop
        path: ${{ github.workspace }}
    - name: 'Azure App Service Deploy: web site'
      uses: Azure/webapps-deploy@v2
      with:
        app-name: ${{ env.WebsiteName }}
        package: ${{ github.workspace }}/drop/MyProject.Web.zip
        slot-name: staging
    - name: 'Swap Slots: website'
      uses: Azure/cli@v1.0.0
      with:
        inlineScript: az webapp deployment slot swap --resource-group ${{ env.ResourceGroupName }} --name ${{ env.WebsiteName }} --slot staging --target-slot production
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }



        [TestMethod]
        public void TerraformTest()
        {
            //Arrange
            string input = @"
trigger:
- main

pool:
  vmImage: windows-latest

steps:
- task: ms-devlabs.custom-terraform-tasks.custom-terraform-installer-task.TerraformInstaller@0
  displayName: 'Install Terraform 0.12.3'
- task: ms-devlabs.custom-terraform-tasks.custom-terraform-release-task.TerraformTaskV1@0
  displayName: 'Terraform : azure init'
  inputs:
    command: init
    backendServiceArm: '[Azure connection]'
    backendAzureRmResourceGroupName: Terraform
    backendAzureRmStorageAccountName: ssterraformstorage
    backendAzureRmContainerName: terraformcontainer
    backendAzureRmKey: terraform.tfstate
- task: ms-devlabs.custom-terraform-tasks.custom-terraform-release-task.TerraformTaskV1@0
  displayName: 'Terraform : azure plan'
  inputs:
    command: plan
    commandOptions: '-var-file=""terraform.tfvars""'
    environmentServiceNameAzureRM: '[Azure connection]'
    backendServiceArm: '[Azure connection]'
    backendAzureRmResourceGroupName: Terraform
    backendAzureRmStorageAccountName: ssterraformstorage
    backendAzureRmContainerName: terraformcontainer
    backendAzureRmKey: terraform.tfstate
- task: ms-devlabs.custom-terraform-tasks.custom-terraform-release-task.TerraformTaskV1@0
  displayName: 'Terraform : azure validate and apply'
  inputs:
    command: apply
    commandOptions: '-var-file=""terraform.tfvars""'
    environmentServiceNameAzureRM: '[Azure connection]'
    backendServiceArm: '[Azure connection]'
    backendAzureRmResourceGroupName: Terraform
    backendAzureRmStorageAccountName: ssterraformstorage
    backendAzureRmContainerName: terraformcontainer
    backendAzureRmKey: terraform.tfstate
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: the 'AZURE_SP' secret is required to be added into GitHub Secrets. See this blog post for details: https://samlearnsazure.blog/2019/12/13/github-actions/
on:
  push:
    branches:
    - main
jobs:
  build:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - # ""Note: the 'AZURE_SP' secret is required to be added into GitHub Secrets. See this blog post for details: https://samlearnsazure.blog/2019/12/13/github-actions/""
      name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_SP }}
    - name: Install Terraform 0.12.3
      uses: hashicorp/setup-terraform@v1
    - name: 'Terraform : azure init'
      run: terraform init
    - name: 'Terraform : azure plan'
      run: terraform plan -var-file=""terraform.tfvars""
    - name: 'Terraform : azure validate and apply'
      run: terraform apply -var-file=""terraform.tfvars"" -auto-approve
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DockerTest()
        {
            //Arrange
            string input = @"
trigger:
- main

resources:
- repo: self

variables:
  # Container registry service connection established during pipeline creation
  dockerRegistryServiceConnection: '{{ containerRegistryConnection.Id }}'
  imageRepository: '{{#toAlphaNumericString imageRepository 50}}{{/toAlphaNumericString}}'
  containerRegistry: '{{ containerRegistryConnection.Authorization.Parameters.loginServer }}'
  dockerfilePath: '{{ dockerfilePath }}'
  tag: '$(Build.BuildId)'

  # Agent VM image name
  vmImageName: 'ubuntu-latest'

stages:
- stage: Build
  displayName: Build and push stage
  jobs:
  - job: Build
    displayName: Build
    pool:
      vmImage: ubuntu-latest
    steps:
    - task: Docker@2
      displayName: Build and push an image to container registry
      inputs:
        command: buildAndPush
        repository: $(imageRepository)
        dockerfile: $(dockerfilePath)
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(tag)
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: login-server needs to be manually set, and the 'REGISTRY_USERNAME' and 'REGISTRY_PASSWORD' secrets are required to be added into GitHub Secrets. See these docs for details: https://github.com/Azure/docker-login
on:
  push:
    branches:
    - main
env:
  dockerRegistryServiceConnection: '{{ containerRegistryConnection.Id }}'
  imageRepository: '{{#toAlphaNumericString imageRepository 50}}{{/toAlphaNumericString}}'
  containerRegistry: '{{ containerRegistryConnection.Authorization.Parameters.loginServer }}'
  dockerfilePath: '{{ dockerfilePath }}'
  tag: ${{ github.run_id }}
  vmImageName: ubuntu-latest
jobs:
  Build_Stage_Build:
    name: Build
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - # ""Note: login-server needs to be manually set, and the 'REGISTRY_USERNAME' and 'REGISTRY_PASSWORD' secrets are required to be added into GitHub Secrets. See these docs for details: https://github.com/Azure/docker-login""
      name: Docker Login
      uses: azure/docker-login@v1
      with:
        login-server: ''
        username: ${{ secrets.REGISTRY_USERNAME }}
        password: ${{ secrets.REGISTRY_PASSWORD }}
    - name: Build and push an image to container registry
      run: |
        docker build --file ${{ env.dockerfilePath }} ${{ env.dockerRegistryServiceConnection }} ${{ env.imageRepository }} --tags ${{ env.tag }}
        docker push --file ${{ env.dockerfilePath }} ${{ env.dockerRegistryServiceConnection }} ${{ env.imageRepository }} --tags ${{ env.tag }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void GitHubVersionTest()
        {
            //Arrange
            string input = @"
trigger:
- main

vmImageName: 'ubuntu-latest'

jobs:
- job: Build
  displayName: Build
  pool:
    vmImage: ubuntu-latest
  steps:
  - task: GitVersion@5
    name: gitVersion
    displayName: 'Evaluate Next Version'
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
jobs:
  Build:
    name: Build
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v2
    - name: Install GitVersion
      uses: gittools/actions/gitversion/setup@v0.9.11
      with:
        versionSpec: 5.x
    - name: Evaluate Next Version
      uses: gittools/actions/gitversion/execute@v0.9.11
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void SachinTest()
        {
            //Arrange
            string input = @"
name: $(date:yyyyMMdd)$(rev:.r) - PR - main - Zircon Legacy Build
resources:
  repositories:
  - repository: self
    type: git
    ref: develop
jobs:
- job: Job_1
  displayName: Zircon Build Agent Job
  pool:
    name: Zircon Build Pool
  steps:
  - checkout: self
    clean: true
    fetchTags: false
  - task: CopyFiles@2
    displayName: 'Copy Files to: D:\ZirconTfs\Build'
    inputs:
      SourceFolder: $(agent.builddirectory)
      TargetFolder: D:\ZirconTfs\Build
      CleanTargetFolder: true
      OverWrite: true
  - task: CmdLine@2
    displayName: MKS Packaging script
    inputs:
      script: ""cd %ZirconBase%\nnant genInterface \nattrib -s -h -r %ZirconBase%\\MKS\\*.* /s /d\nattrib -s -h -r %ZirconBase%\\ProvisionIt\\WebSite\\Bin\\*.* /s /d\nattrib -s -h -r %ZirconBase%\\PITUtil\\PITUtil\\PITUtil\\bin\\*.* /s /d\ndel /f/s/q %ZirconBase%\\ReportItGUI\\WebSite\\Bin\\*\nrd /s/q %ZirconBase%\\ReportItGUI\\WebSite\\Bin\nmd %ZirconBase%\\ReportItGUI\\WebSite\\Bin\ncopy %ZirconBase%\\ReportItGUI\\ThirdParty\\AjaxControlToolkit.dll  %ZirconBase%\\ReportItGUI\\Website\\Bin\\AjaxControlToolkit.dll\ncopy %ZirconBase%\\ReportItGUI\\ThirdParty\\DocumentFormat.OpenXml.dll  %ZirconBase%\\ReportItGUI\\Website\\Bin\\DocumentFormat.OpenXml.dll\ndel /f/s/q d:\\Stage\\Apps\\BUILD\\*\nrd /s/q d:\\Stage\\Apps\\BUILD\ncd %ZirconBase%\\MKS\nperl package.p\n""
  - task: CopyFiles@2
    displayName: 'Copy Files to: D:\stage\apps\TestBuild'
    inputs:
      SourceFolder: D:\stage\apps\build
      TargetFolder: D:\stage\apps\TestBuild
      CleanTargetFolder: true
      OverWrite: true
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"name: ${{ env.date:yyyyMMdd }}${{ env.rev:.r }} - PR - main - Zircon Legacy Build
jobs:
  Job_1:
    name: Zircon Build Agent Job
    runs-on: Zircon Build Pool
    steps:
    - uses: actions/checkout@v2
    - uses: actions/checkout@v2
      with:
        clean: true
    - name: 'Copy Files to: D:\ZirconTfs\Build'
      run: Copy '${{ env.agent.builddirectory }}/' 'D:\ZirconTfs\Build'
    - name: MKS Packaging script
      run: |
        cd %ZirconBase%
        nant genInterface 
        attrib -s -h -r %ZirconBase%\MKS\*.* /s /d
        attrib -s -h -r %ZirconBase%\ProvisionIt\WebSite\Bin\*.* /s /d
        attrib -s -h -r %ZirconBase%\PITUtil\PITUtil\PITUtil\bin\*.* /s /d
        del /f/s/q %ZirconBase%\ReportItGUI\WebSite\Bin\*
        rd /s/q %ZirconBase%\ReportItGUI\WebSite\Bin
        md %ZirconBase%\ReportItGUI\WebSite\Bin
        copy %ZirconBase%\ReportItGUI\ThirdParty\AjaxControlToolkit.dll  %ZirconBase%\ReportItGUI\Website\Bin\AjaxControlToolkit.dll
        copy %ZirconBase%\ReportItGUI\ThirdParty\DocumentFormat.OpenXml.dll  %ZirconBase%\ReportItGUI\Website\Bin\DocumentFormat.OpenXml.dll
        del /f/s/q d:\Stage\Apps\BUILD\*
        rd /s/q d:\Stage\Apps\BUILD
        cd %ZirconBase%\MKS
        perl package.p
      shell: cmd
    - name: 'Copy Files to: D:\stage\apps\TestBuild'
      run: Copy 'D:\stage\apps\build/' 'D:\stage\apps\TestBuild'";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}