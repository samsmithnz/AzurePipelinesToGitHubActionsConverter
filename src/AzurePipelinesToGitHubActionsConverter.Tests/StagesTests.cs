using AzurePipelinesToGitHubActionsConverter.Core;
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
- main
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
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  dependsOn: Build
  jobs:
  - job: Deploy
    displayName: 'Deploy job'
    continueOnError: true
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
#Note: the 'AZURE_SP' secret is required to be added into GitHub Secrets. See this blog post for details: https://samlearnsazure.blog/2019/12/13/github-actions/
on:
  push:
    branches:
    - main
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
      run: |
        dotnet publish FeatureFlags/FeatureFlags.Service/FeatureFlags.Service.csproj --configuration ${{ env.buildConfiguration }} --output ${{ github.workspace }} -p:Version=${{ env.buildNumber }} 
        dotnet publish FeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj --configuration ${{ env.buildConfiguration }} --output ${{ github.workspace }} -p:Version=${{ env.buildNumber }}
    - name: Publish Artifact
      uses: actions/upload-artifact@v2
      with:
        path: ${{ github.workspace }}
  Deploy_Stage_Deploy:
    name: Deploy job
    runs-on: ${{ env.vmImage }}
    needs:
    - Build_Stage_Build
    env:
      AppSettings.Environment: data
      ArmTemplateResourceGroupLocation: eu
      ResourceGroupName: MyProjectFeatureFlags
      WebsiteName: featureflags-data-eu-web
      WebServiceName: featureflags-data-eu-service
    if: (success() && (github.ref == 'refs/heads/main'))
    continue-on-error: true
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
    - name: 'Azure App Service Deploy: web service'
      uses: Azure/webapps-deploy@v2
      with:
        app-name: ${{ env.WebServiceName }}
        package: ${{ github.workspace }}/drop/FeatureFlags.Service.zip
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

        //Test that stages translate to jobs correctly.
        [TestMethod]
        public void StagingNoJobsPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
- stage: BuildA
  displayName: 'BuildA Stage'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
#Note that although having no jobs is valid YAML, it is not a valid GitHub Action.
jobs: {}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }


        [TestMethod]
        public void VariablesWithStageAndConditionalStatementsVariationTest()
        {
            //Arrange
            string input = @"
stages:
- stage: Deploy
  variables:
    ${{ if ne(variables['Build.SourceBranchName'], 'main') }}:
      prId: '00A'
    ${{ if eq(variables['Build.SourceBranchName'], 'main') }}:
      prId: '00B'
    prUC: '002'
    prLC: '003'
  jobs:
  - job: Build1
    displayName: 'Build1 job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world 1!""
";
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
jobs:
  Deploy_Stage_Build1:
    name: Build1 job
    runs-on: windows-latest
    env:
      prId: 00B
      prUC: 002
      prLC: 003
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 1!""
      shell: powershell
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void StagingConditionalVariablesPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
- stage: Deploy
  variables:
    ${{ if ne(variables['Build.SourceBranchName'], 'main') }}:
      prId: ""$(System.PullRequest.PullRequestId)""
    ${{ if eq(variables['Build.SourceBranchName'], 'main') }}:
      prId: '000'
    prUC: ""PR$(prId)""
    prLC: ""pr$(prId)""
  jobs:
  - job: Build1
    displayName: 'Build1 job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world 1!""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  Deploy_Stage_Build1:
    name: Build1 job
    runs-on: windows-latest
    env:
      prId: 000
      prUC: PR${{ env.prId }}
      prLC: pr${{ env.prId }}
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 1!""
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void StagingGenericPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
- stage: Deploy
  variables:
    ${{ if ne(variables['Build.SourceBranchName'], 'main') }}:
      prId: '000'
    var1: ""value1""
    var2: ""value2""
  jobs:
  - job: Build1
    displayName: 'Build1 job'
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world 1!""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  Deploy_Stage_Build1:
    name: Build1 job
    runs-on: windows-latest
    env:
      prId: 000
      var1: value1
      var2: value2
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 1!""
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }


        [TestMethod]
        public void StagingSimpleDependsOnPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
- stage: Deploy
  jobs:
  - job: Build1
    displayName: 'Build1 job'
    dependsOn: Build0
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world 1!""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  Deploy_Stage_Build1:
    name: Build1 job
    runs-on: windows-latest
    needs:
    - Deploy_Stage_Build0
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 1!""
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }
        [TestMethod]
        public void StagingSimpleDependsOnWithParallelStagesPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
- stage: Deploy
  jobs:
  - job: Build1
    displayName: 'Build1 job'
    dependsOn: Build0
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world 1!""
- stage: Deploy2
  jobs:
  - job: Build2
    displayName: 'Build2 job'
    dependsOn: []    # this removes the implicit dependency on previous stage and causes this to run in parallel
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world 2!""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  Deploy_Stage_Build1:
    name: Build1 job
    runs-on: windows-latest
    needs:
    - Deploy_Stage_Build0
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 1!""
      shell: powershell
  Deploy2_Stage_Build2:
    name: Build2 job
    runs-on: windows-latest
    needs: []
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 2!""
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }


        [TestMethod]
        public void StagingComplexDependsOnPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
- stage: Deploy
  jobs:
  - job: Build1
    displayName: 'Build1 job'
    dependsOn: 
    - Build0a 
    - Build0b
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world 1!""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  Deploy_Stage_Build1:
    name: Build1 job
    runs-on: windows-latest
    needs:
    - Deploy_Stage_Build0a
    - Deploy_Stage_Build0b
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world 1!""
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }
        [TestMethod]
        public void StagingComplexDependsOnInJobsPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
stages:
- stage: BuildStage
  jobs:
  - job: BuildJob1
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world build 1!""
  - job: BuildJob2
    displayName: 'Build2 job'
    dependsOn: 
    - BuildJob1 
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world build 2!""

- stage: DeployStage
  dependsOn: 
  - BuildStage
  jobs:
  - job: DeployJob1
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world deploy 1!""
  - job: DeployJob2
    dependsOn: 
    - DeployJob1 
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world deploy 2!""
  - job: DeployJob3
    dependsOn: 
    - DeployJob1
    pool:
      vmImage: windows-latest
    steps:
    - task: PowerShell@2
      inputs:
        targetType: 'inline'
        script: Write-Host ""Hello world deploy 3!""
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
jobs:
  BuildStage_Stage_BuildJob1:
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world build 1!""
      shell: powershell
  BuildStage_Stage_BuildJob2:
    name: Build2 job
    runs-on: windows-latest
    needs:
    - BuildStage_Stage_BuildJob1
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world build 2!""
      shell: powershell
  DeployStage_Stage_DeployJob1:
    runs-on: windows-latest
    needs:
    - BuildStage_Stage_BuildJob1
    - BuildStage_Stage_BuildJob2
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world deploy 1!""
      shell: powershell
  DeployStage_Stage_DeployJob2:
    runs-on: windows-latest
    needs:
    - BuildStage_Stage_BuildJob1
    - BuildStage_Stage_BuildJob2
    - DeployStage_Stage_DeployJob1
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world deploy 2!""
      shell: powershell
  DeployStage_Stage_DeployJob3:
    runs-on: windows-latest
    needs:
    - BuildStage_Stage_BuildJob1
    - BuildStage_Stage_BuildJob2
    - DeployStage_Stage_DeployJob1
    steps:
    - uses: actions/checkout@v2
    - run: Write-Host ""Hello world deploy 3!""
      shell: powershell
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

    }
}