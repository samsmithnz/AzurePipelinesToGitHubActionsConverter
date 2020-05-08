using AzurePipelinesToGitHubActionsConverter.Core.Conversion;
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
- #: 'Note: Error! This step does not have a conversion path yet: invalid fake task'
  run: 'Write-Host Note: Error! This step does not have a conversion path yet: invalid fake task #task: invalid fake task'
  shell: powershell
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
- bash: Write-Host 'some text'
  displayName: test bash
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test bash
  run: Write-Host 'some text'
  shell: bash
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
- name: 'Copy environment ARM template files to: ${GITHUB_WORKSPACE}'
  run: Copy '${{ env.system.defaultworkingdirectory }}\FeatureFlags\FeatureFlags.ARMTemplates/**\*' '${GITHUB_WORKSPACE}\ARMTemplates'
  shell: powershell
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
  if: and(eq('ABCDE', 'BCD'),ne(0, 1))
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
    package: ${GITHUB_WORKSPACE}/drop/MyProject.Web.zip
    slot-name: staging
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
    $testRunSettings = ""/Settings:`""${GITHUB_WORKSPACE}/drop/FunctionalTests/MyProject.FunctionalTests/test.runsettings`"" ""
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
- #: ""Note: Connection string needs to be specified - this is different than Pipelines where the server, database, user, and password were specified separately. It's recommended you use secrets for the connection string.""
  name: Azure SQL dacpac publish
  uses: azure/sql-action@v1
  with:
    server-name: ${{ env.databaseServerName }}.database.windows.net
    connection-string: ${{ secrets.AZURE_SQL_CONNECTION_STRING }}
    dacpac-package: ${GITHUB_WORKSPACE}/drop/MyDatabase.dacpac
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
  uses: Azure/github-actions/arm@master
  env:
    AZURE_RESOURCE_GROUP: ${{ env.ResourceGroupName }}
    AZURE_TEMPLATE_LOCATION: ${GITHUB_WORKSPACE}/drop/ARMTemplates/azuredeploy.json
    AZURE_TEMPLATE_PARAM_FILE: ${GITHUB_WORKSPACE}/drop/ARMTemplates/azuredeploy.parameters.json
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
- run: msbuild '${{ env.solution }}' /p:configuration='${{ env.buildConfiguration }}' /p:platform='${{ env.buildPlatform }}' /p:DeployOnBuild=true /p:WebPublishMethod=Package /p:PackageAsSingleFile=true /p:SkipInvalidConfigurations=true /p:DesktopBuildPackageLocation=""${{ env.build.artifactStagingDirectory }}\WebApp.zip"" /p:DeployIisAppPath=""Default Web Site""
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

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