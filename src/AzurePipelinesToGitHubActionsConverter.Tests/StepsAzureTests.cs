using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StepsAzureTests
    {
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

        [TestMethod]
        public void ArmTemplateV3DeploymentStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzureResourceManagerTemplateDeployment@3
  inputs:
    deploymentScope: 'Resource Group'
    azureResourceManagerConnection: 'copy-connection'
    subscriptionId: '00000000-0000-0000-0000-000000000000'
    action: 'Create Or Update Resource Group'
    resourceGroupName: $(ResourceGroupName)
    location: 'West US'
    templateLocation: 'URL of the file'
    csmFileLink: '$(AzureFileCopy.StorageContainerUri)templates/mainTemplate.json$(AzureFileCopy.StorageContainerSasToken)'
    csmParametersFileLink: '$(AzureFileCopy.StorageContainerUri)templates/mainTemplate.parameters.json$(AzureFileCopy.StorageContainerSasToken)'
    deploymentMode: 'Incremental'
    deploymentName: 'deploy1'
    overrideParameters: '-environment $(AppSettings.Environment) -locationShort $(ArmTemplateResourceGroupLocation)' 
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- uses: Azure/cli@v1.0.0
  with:
    inlineScript: az deployment group create --resource-group ${{ env.ResourceGroupName }} --template-file ${{ env.AzureFileCopy.StorageContainerUri }}templates/mainTemplate.json${{ env.AzureFileCopy.StorageContainerSasToken }} --parameters  -environment ${{ env.AppSettings.Environment }} -locationShort ${{ env.ArmTemplateResourceGroupLocation }}
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
        public void AzureFunctionAppContainerIndividualStepTest()
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
        public void AzureKeyVault2IndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: AzureKeyVault@2
  displayName: 'Get Azure Keyvault secrets 2' 
  inputs:
    connectedServiceName: 'Service Connection2 to Azure Portal'
    keyVaultName: myKeyvaultName
    secretsFilter: '*'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Get Azure Keyvault secrets 2
  uses: azure/cli@v1.0.0
  with:
    inlineScript: az keyvault secret list --subscription Service Connection2 to Azure Portal --vault-name myKeyvaultName
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
        public void AzurePowershellNullVersionIndividualStepTest()
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
        public void AzureSQLDacPacDeployStepTest()
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
        public void AzureSwapSlotsIndividualStepTest()
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
        public void AzureWebAppDeployment3IndividualStepTest()
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
        public void AzureWebAppDeployment4IndividualStepTest()
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
        public void AzureWebAppContainerDeployIndividualStepTest()
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

    }
}