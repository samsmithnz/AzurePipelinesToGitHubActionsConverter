using AzurePipelinesToGitHubActionsConverter.Core;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class JobsTests
    {

        //Check that when using stages, the jobs are created as expected, with Azure logins and checkouts where needed.
        [TestMethod]
        public void BuildJobTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
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
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineJobToGitHubActionJob(yaml);

            //Assert
            string expected = @"
  Build_Stage_Build:
    name: Build job
    runs-on: ${{ env.vmImage }}
    steps:
    - uses: actions/checkout@v2
    - name: Publish dotnet core projects
      run: dotnet publish FeatureFlags/FeatureFlags.Service/FeatureFlags.Service.csprojFeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj --configuration ${{ env.buildConfiguration }} --output ${GITHUB_WORKSPACE} -p:Version=${{ env.buildNumber }}
    - name: Publish Artifact
      uses: actions/upload-artifact@v2
      with:
        path: ${GITHUB_WORKSPACE}";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }
        //Check that when using stages, the jobs are created as expected, with Azure logins and checkouts where needed.
        [TestMethod]
        public void DeployJobTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
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
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineJobToGitHubActionJob(yaml);

            //Assert
            string expected = @"
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
    continue-on-error: true
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

    }
}