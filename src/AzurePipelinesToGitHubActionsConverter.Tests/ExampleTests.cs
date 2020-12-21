using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class ExampleTests
    {
        [TestMethod]
        public void CDTest()
        {
            //Arrange
            string input = @"
trigger:
- master

variables:
  buildConfiguration: 'Release'
  buildPlatform: 'Any CPU'

jobs:
- job: Deploy
  displayName: ""Deploy job""
  pool:
    vmImage: ubuntu-latest
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/master'))
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
            Conversion conversion = new Conversion();

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: 'AZURE_SP' secret is required to be setup and added into GitHub Secrets: https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets
on:
  push:
    branches:
    - master
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
    if: and(success(), eq(github.ref, 'refs/heads/master'))
    steps:
    - uses: actions/checkout@v2
    - # ""Note: 'AZURE_SP' secret is required to be setup and added into GitHub Secrets: https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets""
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
    }
}
