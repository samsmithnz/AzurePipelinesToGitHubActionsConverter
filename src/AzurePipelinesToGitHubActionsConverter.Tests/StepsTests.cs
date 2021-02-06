﻿using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StepsTests
    {

        [TestMethod]
        public void InvalidStepIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = "- task: invalid fake task";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- # ""Error: the step 'invalid fake task' does not have a conversion path yet""
  run: |
    echo ""Error: the step 'invalid fake task' does not have a conversion path yet""
    #task: invalid fake task
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void CmakeIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
    - task: CMake@1
      inputs:
        workingDirectory: 'build'
        cmakeArgs: '-A x64 -DCMAKE_TOOLCHAIN_FILE=../../vcpkg/scripts/buildsystems/vcpkg.cmake -DSELENE_BUILD_ALL=ON -DSELENE_WARNINGS_AS_ERRORS=ON ..'
      displayName: 'Run CMake'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Run CMake
  uses: ashutoshvarma/action-cmake-build@master
  with:
    build-dir: build
    build-options: -A x64 -DCMAKE_TOOLCHAIN_FILE=../../vcpkg/scripts/buildsystems/vcpkg.cmake -DSELENE_BUILD_ALL=ON -DSELENE_WARNINGS_AS_ERRORS=ON ..
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void CmdLineIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: CmdLine@2
  inputs:
    script: echo your commands here 
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- run: echo your commands here
  shell: cmd
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PowerShellIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- powershell: Write-Host 'some text'
  displayName: test PowerShell
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test PowerShell
  run: Write-Host 'some text'
  shell: powershell
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PwshIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- pwsh: Write-Host 'some text'
  displayName: test pwsh
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test pwsh
  run: Write-Host 'some text'
  shell: pwsh
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void BashIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- bash: echo 'some text'
  displayName: test bash
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test bash
  run: echo 'some text'
  shell: bash
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void BashTaskIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Bash@3
  displayName: test bash
  inputs:
    targetType: 'inline'
    script: echo $MYSECRET
  env:
    MYSECRET: $(Foo)
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test bash
  run: echo $MYSECRET
  shell: bash
  env:
    MYSECRET: ${{ env.Foo }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void BashWithBatchShellTaskIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: ShellScript@2
  displayName: test bash
  inputs:
    scriptPath: myscript.sh
    args: -f 'John Smith'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test bash
  run: myscript.sh -f 'John Smith'
  shell: bash
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void CacheIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Cache@2
  displayName: Cache npm dependencies
  inputs:
    key: 'npm | ""$(Agent.OS)"" | package-lock.json'
    restoreKeys: |
      npm | ""$(Agent.OS)""
      npm
    path: $(NPM_CACHE_FOLDER)
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Cache npm dependencies
  uses: actions/cache@v2
  with:
    key: npm | ""${{ runner.os }}"" | package-lock.json
    restore-keys: 
      npm | ""${{ runner.os }}""
      npm
    path: ${{ env.NPM_CACHE_FOLDER }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void CheckOutIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- checkout: self
  submodules: true
  persistCredentials: true
  fetchDepth: 0
  lfs: false
  clean: true
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- uses: actions/checkout@v2
  with:
    fetch-depth: 0
    persist-credentials: true
    lfs: false
    clean: true
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void CopyIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
    - task: CopyFiles@2
      displayName: 'Copy environment ARM template files to: $(build.artifactstagingdirectory)'
      inputs:
        SourceFolder: '$(system.defaultworkingdirectory)\FeatureFlags\FeatureFlags.ARMTemplates'
        Contents: '**\*' 
        TargetFolder: '$(build.artifactstagingdirectory)\ARMTemplates'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: 'Copy environment ARM template files to: ${{ github.workspace }}'
  run: Copy '${{ github.workspace }}\FeatureFlags\FeatureFlags.ARMTemplates/**\*' '${{ github.workspace }}\ARMTemplates'
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void Copy2IndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: CopyFiles@2
  displayName: Copy production build to artifact stage
  inputs:
    SourceFolder: '$(Build.SourcesDirectory)'
    Contents: dist/**
    TargetFolder: '$(Build.ArtifactStagingDirectory)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Copy production build to artifact stage
  run: Copy '${{ github.workspace }}/dist/**' '${{ github.workspace }}'
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void UseDotNetIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: UseDotNet@2
  displayName: 'Use .NET Core sdk'
  inputs:
    packageType: sdk
    version: 2.2.203";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Use .NET Core sdk
  uses: actions/setup-dotnet@v1
  with:
    dotnet-version: 2.2.203
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void BatchIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
  - task: BatchScript@1
    inputs:
      filename: scripts/azure-pipelines/setup_msvc_env.bat
      modifyEnvironment: True
      arguments: /w
    condition: eq(variables['Agent.OS'], 'Windows_NT')
    displayName: Setup MSVC Environment";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Setup MSVC Environment
  run: scripts/azure-pipelines/setup_msvc_env.bat /w
  shell: cmd
  if: (runner.os == 'Windows_NT')
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void BuildDotNetIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
  displayName: dotnet build $(buildConfiguration) part 1";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: dotnet build ${{ env.buildConfiguration }} part 1
  run: dotnet build --configuration ${{ env.buildConfiguration }} WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void PowershellWithSinglelineIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: PowerShell@2
  displayName: 'PowerShell test task'
  inputs:
    targetType: inline
    script: Write-Host 'Hello World'";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: PowerShell test task
  run: Write-Host 'Hello World'
  shell: powershell
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PowershellWithMultilineIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: PowerShell@2
  displayName: 'PowerShell test task'
  inputs:
    targetType: inline
    script: |
      Write-Host 'Hello World'
      Write-Host 'Hello World2'";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: PowerShell test task
  run: |
    Write-Host 'Hello World'
    Write-Host 'Hello World2'
  shell: powershell
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void PowershellWithFileIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
    - task: PowerShell@2
      displayName: 'PowerShell test task'
      inputs:
        targetType: FilePath
        filePath: MyProject/BuildVersion.ps1
        arguments: -ProjectFile ""MyProject/MyProject.Web/MyProject.Web.csproj""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: PowerShell test task
  run: MyProject/BuildVersion.ps1 -ProjectFile ""MyProject/MyProject.Web/MyProject.Web.csproj""
  shell: powershell
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ScriptWithMultilineIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- script: |
    echo Add other tasks to build, test, and deploy your project.
    echo See https://aka.ms/yaml
  displayName: 'Run a multi-line script'";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Run a multi-line script
  run: |
    echo Add other tasks to build, test, and deploy your project.
    echo See https://aka.ms/yaml
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PowershellWithConditionIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: PowerShell@2
  displayName: 'PowerShell test task'
  condition: and(eq('ABCDE', 'BCD'), ne(0, 1))
  inputs:
    targetType: inline
    script: Write-Host 'Hello World'";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: PowerShell test task
  run: Write-Host 'Hello World'
  shell: powershell
  if: (('ABCDE' == 'BCD') && (0 != 1))
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void AzureCLIIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzureCLI@2
  displayName: 'Deploy infrastructure with ARM templates'
  inputs:
    azureSubscription: 'Service Connection to Azure Portal'
    scriptType: ps
    scriptPath: $(build.artifactstagingdirectory)\drop\EnvironmentARMTemplate\PowerShell\DeployInfrastructureCore.ps1
    arguments: -ResourceGroupName ""$(ResourceGroupName)"" 
  enabled: true
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Deploy infrastructure with ARM templates
  uses: azure/cli@v1.0.0
  with:
    inlineScript: ${{ github.workspace }}\drop\EnvironmentARMTemplate\PowerShell\DeployInfrastructureCore.ps1 -ResourceGroupName ""${{ env.ResourceGroupName }}""
    azcliversion: latest
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void AzureKeyVaultIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzureKeyVault@1
  displayName: 'Get Azure Keyvault secrets' 
  inputs:
    azureSubscription: 'Service Connection to Azure Portal'
    keyVaultName: myKeyvaultName
    secretsFilter: '*'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Get Azure Keyvault secrets
  uses: azure/cli@v1.0.0
  with:
    inlineScript: az keyvault secret list --subscription Service Connection to Azure Portal --vault-name myKeyvaultName
    azcliversion: latest
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void AzurePowershellIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzurePowerShell@4 
  displayName: 'Run Azure PowerShell' 
  inputs:
    azureSubscription: 'Service Connection to Azure Portal'
    ScriptPath: '$(build.artifactstagingdirectory)/drop/EnvironmentARMTemplate/PowerShell/Cleanup.ps1'
    ScriptArguments: '-ResourceGroupName ""$(ResourceGroupName)""'
    azurePowerShellVersion: LatestVersion
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Run Azure PowerShell
  uses: azure/powershell@v1
  with:
    inlineScript: ${{ github.workspace }}/drop/EnvironmentARMTemplate/PowerShell/Cleanup.ps1 -ResourceGroupName ""${{ env.ResourceGroupName }}""
    azPSVersion: latest
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void AzurePowershellWithOtherVersionIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzurePowerShell@4 
  displayName: 'Run Azure PowerShell' 
  inputs:
    azureSubscription: 'Service Connection to Azure Portal'
    ScriptPath: '$(build.artifactstagingdirectory)/drop/EnvironmentARMTemplate/PowerShell/Cleanup.ps1'
    ScriptArguments: '-ResourceGroupName ""$(ResourceGroupName)""'
    azurePowerShellVersion: OtherVersion
    preferredAzurePowerShellVersion: 1.2.3
    errorActionPreference: stop
    FailOnStandardError: false
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Run Azure PowerShell
  uses: azure/powershell@v1
  with:
    inlineScript: ${{ github.workspace }}/drop/EnvironmentARMTemplate/PowerShell/Cleanup.ps1 -ResourceGroupName ""${{ env.ResourceGroupName }}""
    azPSVersion: 1.2.3
    errorActionPreference: stop
    failOnStandardError: false
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void AzurePowershellWithOtherVersionAndTargetAzurePsIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzurePowerShell@4 
  displayName: 'Run Azure PowerShell' 
  inputs:
    azureSubscription: 'Service Connection to Azure Portal'
    ScriptPath: '$(build.artifactstagingdirectory)/drop/EnvironmentARMTemplate/PowerShell/Cleanup.ps1'
    ScriptArguments: '-ResourceGroupName ""$(ResourceGroupName)""'
    azurePowerShellVersion: OtherVersion
    targetAzurePs: 1.2.3
    errorActionPreference: stop
    FailOnStandardError: false
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Run Azure PowerShell
  uses: azure/powershell@v1
  with:
    inlineScript: ${{ github.workspace }}/drop/EnvironmentARMTemplate/PowerShell/Cleanup.ps1 -ResourceGroupName ""${{ env.ResourceGroupName }}""
    azPSVersion: 1.2.3
    errorActionPreference: stop
    failOnStandardError: false
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void AzurePowershellWithOtherVersionAndCustomTargetAzurePsIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzurePowerShell@4 
  displayName: 'Run Azure PowerShell' 
  inputs:
    azureSubscription: 'Service Connection to Azure Portal'
    ScriptPath: '$(build.artifactstagingdirectory)/drop/EnvironmentARMTemplate/PowerShell/Cleanup.ps1'
    ScriptArguments: '-ResourceGroupName ""$(ResourceGroupName)""'
    azurePowerShellVersion: OtherVersion
    customTargetAzurePs: 1.2.3
    errorActionPreference: stop
    FailOnStandardError: false
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Run Azure PowerShell
  uses: azure/powershell@v1
  with:
    inlineScript: ${{ github.workspace }}/drop/EnvironmentARMTemplate/PowerShell/Cleanup.ps1 -ResourceGroupName ""${{ env.ResourceGroupName }}""
    azPSVersion: 1.2.3
    errorActionPreference: stop
    failOnStandardError: false
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void AzureWebAppDeploymentIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzureRmWebAppDeployment@4
  inputs:
    connectionType: 'AzureRM'
    azureSubscription: ""$(azureSubscription)""
    appType: 'functionApp'
    webAppName: ""$(functionappName)""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- uses: Azure/webapps-deploy@v2
  with:
    app-name: ${{ env.functionappName }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void AzureWebAppIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzureWebApp@1
  displayName: 'Deploy Azure Web App- app1'
  inputs:
    azureSubscription: $(azureServiceConnectionId)
    appName: $(webAppName)
    package: $(build.artifactstagingdirectory)/drop/MyProject.Web.zip
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Deploy Azure Web App- app1
  uses: Azure/webapps-deploy@v2
  with:
    app-name: ${{ env.webAppName }}
    package: ${{ github.workspace }}/drop/MyProject.Web.zip
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PublishPipelineArtifactsIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: PublishPipelineArtifact@0
  displayName: Store artifact
  inputs:
    artifactName: 'drop'
    targetPath: 'MyProject/bin/release/netcoreapp2.2/publish/'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Store artifact
  uses: actions/upload-artifact@v2
  with:
    path: MyProject/bin/release/netcoreapp2.2/publish/
    name: drop
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DeployWebAppIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
    - task: AzureRmWebAppDeployment@3
      displayName: 'Azure App Service Deploy: web site'
      inputs:
        azureSubscription: 'MyProject connection to Azure Portal'
        WebAppName: $(WebsiteName)
        DeployToSlotFlag: true
        ResourceGroupName: $(ResourceGroupName)
        SlotName: 'staging'
        Package: '$(build.artifactstagingdirectory)/drop/MyProject.Web.zip'
        TakeAppOfflineFlag: true
        JSONFiles: '**/appsettings.json'  
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: 'Azure App Service Deploy: web site'
  uses: Azure/webapps-deploy@v2
  with:
    app-name: ${{ env.WebsiteName }}
    package: ${{ github.workspace }}/drop/MyProject.Web.zip
    slot-name: staging
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DownloadIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- download: current  # refers to artifacts published by current pipeline
  artifact: WebApp
  patterns: '**/.js'
  displayName: Download artifact WebApp
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Download artifact WebApp
  uses: actions/download-artifact@v2
  with:
    name: WebApp
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DownloadWithPatternsIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- download: current  # refers to artifacts published by current pipeline
  patterns: '**/.js'
  displayName: Download artifact WebApp
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Download artifact WebApp
  uses: actions/download-artifact@v2
  with:
    name: '**/.js'
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DownloadBuildArtifactsIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
          - task: DownloadBuildArtifacts@0
            displayName: 'Download Build Artifact'
            inputs:
              buildType: 'current'
              downloadType: 'single'
              artifactName: $(buildArtifactName)
              downloadPath: '$(Build.SourcesDirectory)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Download Build Artifact
  uses: actions/download-artifact@v2
  with:
    name: ${{ env.buildArtifactName }}
    path: ${{ github.workspace }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DownloadPipelineArtifactsIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: DownloadPipelineArtifact@2
  inputs:
    artifact: 'WebApp'
    path: $(Build.SourcesDirectory)/bin
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- uses: actions/download-artifact@v2
  with:
    name: WebApp
    path: ${{ github.workspace }}/bin
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DeployAzureWebAppContainerIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
  - task: AzureWebAppContainer@1
    displayName: 'Azure Web App on Container Deploy'
    inputs:
      azureSubscription: '$(AzureServiceConnectionId)'
      appName: $(AppName)
      imageName: '$(ACRFullName)/$(ACRImageName)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Azure Web App on Container Deploy
  uses: Azure/webapps-deploy@v2
  with:
    app-name: ${{ env.AppName }}
    images: ${{ env.ACRFullName }}/${{ env.ACRImageName }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DeployFunctionAppContainerIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
  - task: AzureFunctionAppContainer@1
    displayName: 'Azure Function App on Container Deploy'
    inputs:
      azureSubscription: '$(AzureServiceConnectionId)'
      appName: $(AppName)
      imageName: '$(ACRFullName)/$(ACRImageName)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Azure Function App on Container Deploy
  uses: Azure/webapps-deploy@v2
  with:
    app-name: ${{ env.AppName }}
    images: ${{ env.ACRFullName }}/${{ env.ACRImageName }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DotNetCoreCLIRestoreIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: DotNetCoreCLI@2
  displayName: Restore
  inputs:
    command: restore
    projects: MyProject/MyProject.Models/MyProject.Models.csproj
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Restore
  run: dotnet restore MyProject/MyProject.Models/MyProject.Models.csproj
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void DotNetCoreCLIPushIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: DotNetCoreCLI@2
  displayName: 'Push to GitHub Packages'
  condition: and(succeeded(), startsWith(variables['build.sourceBranch'], 'refs/tags/')) # Only run this step when the trigger event is a tag push
  inputs:
    command: 'push'
    packagesToPush: '$(Build.ArtifactStagingDirectory)/*.nupkg'
    nuGetFeedType: 'external'
    publishFeedCredentials: 'GitHub Packages'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Push to GitHub Packages
  run: dotnet nuget push ${{ github.workspace }}/*.nupkg --source ""github""
  if: (success() && startsWith(github.ref, 'refs/tags/'))
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DotNetCoreCLIBuildIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: DotNetCoreCLI@2
  displayName: Build
  inputs:
    projects: MyProject/MyProject.Models/MyProject.Models.csproj
    arguments: '--configuration $(BuildConfiguration)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Build
  run: dotnet MyProject/MyProject.Models/MyProject.Models.csproj --configuration ${{ env.BuildConfiguration }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }



        [TestMethod]
        public void DotNetCoreCLIPublishIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: DotNetCoreCLI@2
  displayName: Publish
  inputs:
    command: publish
    publishWebProjects: false
    projects: MyProject/MyProject.Models/MyProject.Models.csproj
    arguments: '--configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)'
    zipAfterPublish: false
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Publish
  run: dotnet publish MyProject/MyProject.Models/MyProject.Models.csproj --configuration ${{ env.BuildConfiguration }} --output ${{ github.workspace }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }



        [TestMethod]
        public void DotNetCoreCLIPackIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: DotNetCoreCLI@2
  displayName: 'dotnet pack'
  inputs:
    command: pack
    packagesToPack: MyProject/MyProject.Models/MyProject.Models.csproj
    versioningScheme: byEnvVar
    versionEnvVar: BuildVersion
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: dotnet pack
  run: dotnet pack MyProject/MyProject.Models/MyProject.Models.csproj
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void DotNetCoreCLIInvalidIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: DotNetCoreCLI@2
  displayName: 'dotnet build but no inputs'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- # This DotNetCoreCLI task is misconfigured, inputs are required
  name: dotnet build but no inputs
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void SwapSlotsIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
    - task: AzureAppServiceManage@0
      displayName: 'Swap Slots: website'
      inputs:
        azureSubscription: 'MyProject connection to Azure Portal'
        WebAppName: $(WebsiteName)
        ResourceGroupName: $(ResourceGroupName)
        SourceSlot: 'staging'      
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: 'Swap Slots: website'
  uses: Azure/cli@v1.0.0
  with:
    inlineScript: az webapp deployment slot swap --resource-group ${{ env.ResourceGroupName }} --name ${{ env.WebsiteName }} --slot staging --target-slot production
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ExtractFilesIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: ExtractFiles@1
  inputs:
    archiveFilePatterns: '**/*.zip'
    cleanDestinationFolder: true
    overwriteExistingFiles: false
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- # 'Note: This is a third party action and currently only supports Linux: https://github.com/marketplace/actions/create-zip-file'
  uses: montudor/action-zip@v0.1.0
  with:
    args: 'unzip -qq **/*.zip -d '
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void FunctionalTestIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: VSTest@2
  displayName: 'Run Selenium smoke tests on website'
  inputs:
    searchFolder: '$(build.artifactstagingdirectory)'
    testAssemblyVer2: '**\MyProject.FunctionalTests\MyProject.FunctionalTests.dll'
    uiTests: true
    runSettingsFile: '$(build.artifactstagingdirectory)/drop/FunctionalTests/MyProject.FunctionalTests/test.runsettings'    
    overrideTestrunParameters: |
      -ServiceUrl ""https://$(WebServiceName)-staging.azurewebsites.net/"" 
      -WebsiteUrl ""https://$(WebsiteName)-staging.azurewebsites.net/""
      -TestEnvironment ""$(AppSettings.Environment)"" -TestEnvironment2 ""$(AppSettings.Environment)""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Run Selenium smoke tests on website
  run: |
    $vsTestConsoleExe = ""C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\TestPlatform\vstest.console.exe""
    $targetTestDll = ""**\MyProject.FunctionalTests\MyProject.FunctionalTests.dll""
    $testRunSettings = ""/Settings:`""${{ github.workspace }}/drop/FunctionalTests/MyProject.FunctionalTests/test.runsettings`"" ""
    $parameters = "" -- ServiceUrl=""https://${{ env.WebServiceName }}-staging.azurewebsites.net/"" WebsiteUrl=""https://${{ env.WebsiteName }}-staging.azurewebsites.net/"" TestEnvironment=""${{ env.AppSettings.Environment }}"" TestEnvironment2=""${{ env.AppSettings.Environment }}"" ""
    #Note that the `"" is an escape character to quote strings, and the `& is needed to start the command
    $command = ""`& `""$vsTestConsoleExe`"" `""$targetTestDll`"" $testRunSettings $parameters ""
    Write-Host ""$command""
    Invoke-Expression $command
  shell: powershell
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void GitHubReleaseIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: GithubRelease@0 
  displayName: 'Create GitHub Release'      
  inputs:
    gitHubConnection: zenithworks
    repositoryName: zenithworks/javaAppWithMaven
    tagSource: manual
    tag: $(Build.BuildNumber)   
    title: ""Release $(Build.BuildNumber)""
    assets: $(Build.ArtifactStagingDirectory)/*.exe
    releaseNotes: |
      Changes in this Release
      - First Change
      - Second Change
    isDraft: false # Optional
    isPreRelease: false # Optional
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Create GitHub Release
  uses: actions/create-release@v1
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  with:
    tag_name: ${{ github.run_number }}
    release_name: Release ${{ github.run_number }}
    body: 
      Changes in this Release
      - First Change
      - Second Change
    draft: false
    prerelease: false
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void GitVersionIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: GitVersion@5
  name: gitVersion
  displayName: 'Evaluate Next Version'
  inputs:
    runtime: 'core'
    configFilePath: 'GitVersion.yml'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Evaluate Next Version
  uses: gittools/actions/gitversion/execute@v0.9.7
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void HugoIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: HugoTask@1
  displayName: 'Generate Hugo site'
  inputs:
    hugoVersion: latest
    extendedVersion: true
    destination: '$(Build.ArtifactStagingDirectory)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- # 'Note: This is a third party action: https://github.com/peaceiris/actions-hugo'
  name: Generate Hugo site
  uses: peaceiris/actions-hugo@v2
  with:
    hugo-version: latest
    extended: true
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //[TestMethod]
        //public void IISWebManagementStepTest()
        //{
        //    //Arrange
        //    Conversion conversion = new Conversion();
        //    string yaml = @"
        //              - task: IISWebAppManagementOnMachineGroup@0
        //                inputs:
        //                  IISDeploymentType: 'IISWebsite'
        //                  ActionIISWebsite: 'CreateOrUpdateWebsite'
        //                  WebsiteName: 'Spark'
        //                  WebsitePhysicalPath: '%SystemDrive%\inetpub\wwwroot'
        //                  WebsitePhysicalPathAuth: 'WebsiteUserPassThrough'
        //                  AddBinding: true
        //                  CreateOrUpdateAppPoolForWebsite: true
        //                  ConfigureAuthenticationForWebsite: true
        //                  AppPoolNameForWebsite: 'Spark'
        //                  DotNetVersionForWebsite: 'v4.0'
        //                  PipeLineModeForWebsite: 'Integrated'
        //                  AppPoolIdentityForWebsite: 'ApplicationPoolIdentity'
        //                  AnonymousAuthenticationForWebsite: true
        //                  WindowsAuthenticationForWebsite: false
        //                  protocol: 'http' 
        //                  iPAddress: 'All Unassigned'
        //                  port: '80'
        //";

        //    //Act
        //    ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

        //    //Assert
        //    string expected = @"
        //";
        //    expected = UtilityTests.TrimNewLines(expected);
        //    Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //}
        //[TestMethod]
        //public void IISWebDeploymentStepTest()
        //{
        //    //Arrange
        //    Conversion conversion = new Conversion();
        //    string yaml = @"
        //      - task: IISWebAppDeploymentOnMachineGroup@0
        //        inputs:
        //          WebSiteName: 'Spark'
        //          Package: '$(Pipeline.Workspace)\Art.Web.zip'
        //          XmlVariableSubstitution: true
        //";

        //    //Act
        //    ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

        //    //Assert
        //    string expected = @"
        //";
        //    expected = UtilityTests.TrimNewLines(expected);
        //    Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //}

        [TestMethod]
        public void SQLAzureDacPacDeployStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
          - task: SqlAzureDacpacDeployment@1
            displayName: 'Azure SQL dacpac publish'
            inputs:
              azureSubscription: 'my connection to Azure Portal'
              ServerName: '$(databaseServerName).database.windows.net'
              DatabaseName: '$(databaseName)'
              SqlUsername: '$(databaseLoginName)'
              SqlPassword: '$(databaseLoginPassword)'
              DacpacFile: '$(build.artifactstagingdirectory)/drop/MyDatabase.dacpac'
              additionalArguments: '/p:BlockOnPossibleDataLoss=true'   
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- # ""Note: Connection string needs to be specified - this is different than Pipelines where the server, database, user, and password were specified separately. It's recommended you use secrets for the connection string.""
  name: Azure SQL dacpac publish
  uses: azure/sql-action@v1
  with:
    server-name: ${{ env.databaseServerName }}.database.windows.net
    connection-string: ${{ secrets.AZURE_SQL_CONNECTION_STRING }}
    dacpac-package: ${{ github.workspace }}/drop/MyDatabase.dacpac
    arguments: /p:BlockOnPossibleDataLoss=true";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void AntStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- run: ant -noinput -buildfile build.xml
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void ArchiveFilesStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: ArchiveFiles@2
  inputs:
    rootFolderOrFile: '$(build.sourcesDirectory)'
    includeRootFolder: false";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- # 'Note: This is a third party action and currently only supports Linux: https://github.com/marketplace/actions/create-zip-file'
  uses: montudor/action-zip@v0.1.0
  with:
    args: zip -qq -r  ${{ github.workspace }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void ArmTemplateDeploymentStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Deploy ARM Template to resource group
  uses: Azure/cli@v1.0.0
  with:
    inlineScript: az deployment group create --resource-group ${{ env.ResourceGroupName }} --template-file ${{ github.workspace }}/drop/ARMTemplates/azuredeploy.json --parameters  ${{ github.workspace }}/drop/ARMTemplates/azuredeploy.parameters.json -environment ${{ env.AppSettings.Environment }} -locationShort ${{ env.ArmTemplateResourceGroupLocation }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        //        [TestMethod]
        //        public void MSBuildStepTest()
        //        {
        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            string yaml = @"
        //- task: VSBuild@1
        //  inputs:
        //    solution: '$(solution)'
        //    msbuildArgs: '/p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:DesktopBuildPackageLocation=""$(build.artifactStagingDirectory)\WebApp.zip"" /p:DeployIisAppPath=""Default Web Site""'
        //    platform: '$(buildPlatform)'
        //    configuration: '$(buildConfiguration)'
        //";

        //            //Act
        //            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

        //            //Assert
        //            string expected = "- run: \" \\r\\n    #TODO: Fix this uglyness.\\r\\n    $msBuildExe = \\\"C:\\\\Program Files(x86)\\\\Microsoft Visual Studio\\\\2019\\\\Enterprise\\\\MSBuild\\\\Current\\\\Bin\\\\msbuild.exe\\\"\\r\\n    $targetSolution = \\\"${{ env.solution }}\\\"\\r\\n    #Note that the `\\\" is an escape character sequence to quote strings, and `& is needed to start the command\\r\\n    $command = \\\"`& `\\\"$msBuildExe`\\\" `\\\"$targetSolution`\\\" \\r\\n    Write - Host \\\"$command\\\"\\r\\n    Invoke - Expression $command\"\r\n  shell: powershell";
        //            expected = UtilityTests.TrimNewLines(expected);
        //            //gitHubOutput.actionsYaml = gitHubOutput.actionsYaml.Replace("\"", @"""");
        //            //gitHubOutput.actionsYaml = gitHubOutput.actionsYaml.Replace("\\", @"\");

        //            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //        }

        [TestMethod]
        public void NPMInstallStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Npm@1
  displayName: 'npm install'
  inputs:
    command: install
    workingDir: src/angular7
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: npm install
  run: npm install src/angular7
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void NPMToBuildAngularStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Npm@1
  displayName: 'Build Angular'
  inputs:
    command: custom
    customCommand: run build -- --prod
    workingDir: src/angular7
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Build Angular
  run: npm run build -- --prod src/angular7
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void NuGetStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: NuGetCommand@2
  inputs:
    restoreSolution: '$(solution)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- run: nuget restore ${{ env.solution }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void NuGetWithRestoreStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: NuGetCommand@2
  inputs:
    command: restore
    restoreSolution: '$(solution)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- run: nuget restore ${{ env.solution }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void MSBuildStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
        - task: VSBuild@1
          inputs:
            solution: '$(solution)'
            msbuildArgs: '/p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:DesktopBuildPackageLocation=""$(build.artifactStagingDirectory)\WebApp.zip"" /p:DeployIisAppPath=""Default Web Site""'
            platform: '$(buildPlatform)'
            configuration: '$(buildConfiguration)'
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- run: msbuild '${{ env.solution }}' /p:configuration='${{ env.buildConfiguration }}' /p:platform='${{ env.buildPlatform }}' /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:DesktopBuildPackageLocation=""${{ github.workspace }}\WebApp.zip"" /p:DeployIisAppPath=""Default Web Site""";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void MSBuild2StepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: MSBuild@1
  inputs:
    solution: '**/*.sln'
    msbuildArchitecture: 'x86'
    platform: 'Any CPU'
    configuration: 'Release'
    msbuildArguments: '/t:Publish /p:PublishUrl=""publish""'
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- run: msbuild '**/*.sln' /p:configuration='Release' /p:platform='Any CPU' /t:Publish /p:PublishUrl=""publish""
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void PublishTestResultsStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: PublishTestResults@2
  inputs:
    testRunner: VSTest
    testResultsFiles: '**/*.trx'
    failTaskOnFailedTests: true
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- # ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
  run: |
    echo ""Error: the step 'PublishTestResults@2' does not have a conversion path yet""
    #task: PublishTestResults@2
    #inputs:
    #  testrunner: VSTest
    #  testresultsfiles: '**/*.trx'
    #  failtaskonfailedtests: true
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformInstallerStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: terraformInstaller@0
  displayName: Install Terraform
  inputs:
    terraformVersion: '0.12.12'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- uses: hashicorp/setup-terraform@v1
  displayName: Install Terraform
  with:
    terraform_version: 0.12.12
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

//        [TestMethod]
//        public void TerraformTaskStepTest()
//        {
//            //Arrange
//            Conversion conversion = new Conversion();
//            string yaml = @"
//- task: terraform@0
//  displayName: Terraform Init
//  inputs:
//    command: 'init'
//    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
//    backendAzureProviderStorageAccountName: 'mtcdenterraformsandbox'
    
//- task: terraform@0
//  displayName: Terraform Plan
//  inputs:
//    command: 'plan'
//    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
//    args: -var=environment=demo -out=tfplan.out

//- task: terraform@0
//  displayName: Terraform Apply
//  inputs:
//    command: 'apply'
//    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
//    args: tfplan.out

//- task: terraform@0
//  displayName: Execute Terraform CLI Script
//  inputs:
//    command: 'CLI'
//    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
//    backendAzureProviderStorageAccountName: 'mtcdenterraformsandbox'
//    script: |
//      # Validate
//      terraform validate

//      # Plan
//      terraform plan -input=false -out=testplan.tf

//      # Get output
//      STORAGE_ACCOUNT=`terraform output storage_account`

//      # Set storageAccountName variable from terraform output
//      echo ""##vso[task.setvariable variable=storageAccountName]$STORAGE_ACCOUNT""
//        ";

//            //Act
//            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

//            //Assert
//            string expected = @"

//";

//            expected = UtilityTests.TrimNewLines(expected);
//            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
//        }

        [TestMethod]
        public void TimeoutAndContinueOnErrorStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: CmdLine@2
  inputs:
    script: echo your commands here 
  continueOnError: true
  timeoutInMinutes: 12
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- run: echo your commands here
  shell: cmd
  continue-on-error: true
  timeout-minutes: 12
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TemplateStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- template: templates/npm-build-steps.yaml
  parameters:
    extensionName: $(ExtensionName)
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- # There is no conversion path for templates in GitHub Actions
  run: |
    #templates/npm-build-steps.yaml
    extensionName: ${{ env.ExtensionName }}
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //        [TestMethod]
        //        public void KubernetesStepTest()
        //        {
        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            string yaml = @"
        //- task: Kubernetes@1
        //  displayName: kubectl apply
        //  inputs:
        //    connectionType: Azure Resource Manager
        //    azureSubscriptionEndpoint: Contoso
        //    azureResourceGroup: contoso.azurecr.io
        //    kubernetesCluster: Contoso
        //    useClusterAdmin: false
        //";

        //            //Act
        //            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

        //            //Assert
        //            string expected = @"

        //";
        //            expected = UtilityTests.TrimNewLines(expected);
        //            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //        }    



    }
}