using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StagesTests
    {

        //Check that when using stages, the jobs are created as expected, with Azure logins and checkouts where needed.
        [TestMethod]
        public void LargeMultiStagePipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master
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
      ResourceGroupName: 'MyProjectFeatureFlags'
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
    - task: AzureRmWebAppDeployment@3
      displayName: 'Azure App Service Deploy: web service'
      inputs:
        azureSubscription: 'Connection to Azure Portal'
        WebAppName: $(WebServiceName)
        DeployToSlotFlag: true
        ResourceGroupName: $(ResourceGroupName)
        SlotName: 'staging'
        Package: '$(build.artifactstagingdirectory)/drop/FeatureFlags.Service.zip'
        TakeAppOfflineFlag: true
        JSONFiles: '**/appsettings.json'
    - task: AzureAppServiceManage@0
      displayName: 'Swap Slots: web service'
      inputs:
        azureSubscription: 'Connection to Azure Portal'
        WebAppName: $(WebServiceName)
        ResourceGroupName: $(ResourceGroupName)
        SourceSlot: 'staging'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note: 'AZURE_SP' secret is required to be setup and added into GitHub Secrets: https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets
on:
  push:
    branches:
    - master
env:
  vmImage: windows-latest
  buildConfiguration: Release
  buildPlatform: Any CPU
  buildNumber: 1.1.0.0
jobs:
  Build_Stage_Build:
    name: Build job
    runs-on: ${{ env.vmImage }}
    steps:
    - uses: actions/checkout@v2
    - name: Publish dotnet core projects
      run: dotnet publish FeatureFlags/FeatureFlags.Service/FeatureFlags.Service.csprojFeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj --configuration ${{ env.buildConfiguration }} --output ${GITHUB_WORKSPACE} -p:Version=${{ env.buildNumber }}
    - name: Publish Artifact
      uses: actions/upload-artifact@master
      with:
        path: ${GITHUB_WORKSPACE}
  Deploy_Stage_Deploy:
    name: Deploy job
    runs-on: ${{ env.vmImage }}
    env:
      AppSettings.Environment: data
      ArmTemplateResourceGroupLocation: eu
      ResourceGroupName: MyProjectFeatureFlags
      WebsiteName: featureflags-data-eu-web
      WebServiceName: featureflags-data-eu-service
    if: and(success(),eq(github.ref, 'refs/heads/master'))
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
    - name: 'Azure App Service Deploy: web service'
      uses: Azure/webapps-deploy@v2
      with:
        app-name: ${{ env.WebServiceName }}
        package: ${GITHUB_WORKSPACE}/drop/FeatureFlags.Service.zip
        slot-name: staging
    - name: 'Swap Slots: web service'
      uses: Azure/cli@v1.0.0
      with:
        inlineScript: az webapp deployment slot swap --resource-group ${{ env.ResourceGroupName }} --name ${{ env.WebServiceName }} --slot staging --target-slot production
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        //Test that stages translate to jobs correctly.
        [TestMethod]
        public void StagingPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
- stage: BuildA
  displayName: 'BuildA Stage'
  jobs:
  - job: Build1
    displayName: 'Build1 job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: |
         Write-Host ""Hello world 1!""

  - job: Build2
    displayName: 'Build2 job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world 2!""

- stage: DeployB
  displayName: 'DeployB Stage'
  jobs:
  - job: Deploy3
    displayName: 'Deploy3 job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: |
         Write-Host ""Hello world 3!""
  - job: Deploy4
    displayName: 'Deploy4 job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: |
         Write-Host ""Hello world 4!""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  BuildA_Stage_Build1:
    name: Build1 job
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 1!""
      shell: powershell
  BuildA_Stage_Build2:
    name: Build2 job
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 2!""
      shell: powershell
  DeployB_Stage_Deploy3:
    name: Deploy3 job
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 3!""
      shell: powershell
  DeployB_Stage_Deploy4:
    name: Deploy4 job
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 4!""
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}