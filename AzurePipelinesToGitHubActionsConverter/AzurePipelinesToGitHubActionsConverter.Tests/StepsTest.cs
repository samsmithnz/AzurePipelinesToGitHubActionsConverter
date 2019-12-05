using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class StepsTest
    {
        //TODO: Resolve difference between Ubuntu and Windows
        //[TestMethod]
        //public void InvalidStepIndividualStepTest()
        //{
        //    //Arrange
        //    Conversion conversion = new Conversion();
        //    string yaml = "- task: invalid fake task";

        //    //Act
        //    ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

        //    //Assert
        //    string expected = "- #: 'NOTE: This step does not have a conversion path yet: invalid fake task'\r\n  run: '#task: invalid fake task'\r\n  shell: powershell";

        //    string actualBytes = "";
        //    foreach (byte b in System.Text.Encoding.UTF8.GetBytes(gitHubOutput.actionsYaml.ToCharArray()))
        //    {
        //        actualBytes += b.ToString();
        //    }
        //    string expectedBytes = "";
        //    foreach (byte b in System.Text.Encoding.UTF8.GetBytes(expected.ToCharArray()))
        //    {
        //        expectedBytes += b.ToString();
        //    }
        //    Assert.AreEqual(expectedBytes, actualBytes);

        //    byte[] byteArray = new byte[] { 0x31, 31 };

        //    string result = System.Text.Encoding.UTF8.GetString(byteArray);

        //    expected = TestUtility.TrimNewLines(expected);
        //    Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //}

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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- run: echo your commands here
  shell: cmd
";
            expected = TestUtility.TrimNewLines(expected);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test PowerShell
  run: Write-Host 'some text'
  shell: powershell
";
            expected = TestUtility.TrimNewLines(expected);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test pwsh
  run: Write-Host 'some text'
  shell: pwsh
";
            expected = TestUtility.TrimNewLines(expected);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: test bash
  run: Write-Host 'some text'
  shell: bash
";
            expected = TestUtility.TrimNewLines(expected);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Use .NET Core sdk
  uses: actions/setup-dotnet@v1
  with:
    dotnet-version: 2.2.203
";
            expected = TestUtility.TrimNewLines(expected);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: dotnet build ${{ env.buildConfiguration }} part 1
  run: dotnet build --configuration ${{ env.buildConfiguration }} WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
";
            expected = TestUtility.TrimNewLines(expected);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            Assert.IsTrue(string.IsNullOrEmpty(gitHubOutput.actionsYaml) == false);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            Assert.IsTrue(string.IsNullOrEmpty(gitHubOutput.actionsYaml) == false);
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
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            Assert.IsTrue(string.IsNullOrEmpty(gitHubOutput.actionsYaml) == false);
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
    artifactName: 'MyProject'
    targetPath: 'MyProject/bin/release/netcoreapp2.2/publish/'
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Store artifact
  uses: actions/upload-artifact@master
  with:
    path: MyProject/bin/release/netcoreapp2.2/publish/
";
            expected = TestUtility.TrimNewLines(expected);
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
        azureSubscription: 'SamLearnsAzure connection to Azure Portal'
        WebAppName: $(WebsiteName)
        DeployToSlotFlag: true
        ResourceGroupName: $(ResourceGroupName)
        SlotName: 'staging'
        Package: '$(build.artifactstagingdirectory)/drop/MyProject.Web.zip'
        TakeAppOfflineFlag: true
        JSONFiles: '**/appsettings.json'        
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: 'Azure App Service Deploy: web site'
  uses: Azure/webapps-deploy@v1
  with:
    app-name: ${{ env.WebsiteName }}
    package: ${GITHUB_WORKSPACE}/drop/MyProject.Web.zip
    slot-name: staging
";
            expected = TestUtility.TrimNewLines(expected);
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
        azureSubscription: 'SamLearnsAzure connection to Azure Portal'
        WebAppName: $(WebsiteName)
        ResourceGroupName: $(ResourceGroupName)
        SourceSlot: 'staging'      
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: 'Swap Slots: website'
  uses: Azure/cli@v1.0.0
  with:
    inlineScript: az webapp deployment slot swap --resource-group ${{ env.ResourceGroupName }} --name ${{ env.WebsiteName }} --slot staging --target-slot production
";
            expected = TestUtility.TrimNewLines(expected);
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
    testAssemblyVer2: |
      **\MyProject.FunctionalTests\MyProject.FunctionalTests.dll
    uiTests: true
    runSettingsFile: '$(build.artifactstagingdirectory)/drop/FunctionalTests/MyProject.FunctionalTests/test.runsettings'    
    overrideTestrunParameters: |
      -ServiceUrl ""https://$(WebServiceName)-staging.azurewebsites.net/"" 
      -WebsiteUrl ""https://$(WebsiteName)-staging.azurewebsites.net/""
      -TestEnvironment ""$(AppSettings.Environment)"" -TestEnvironment2 ""$(AppSettings.Environment)""
";

            //Act
            ConversionResult gitHubOutput = conversion.ConvertAzurePinelineTaskToGitHubActionTask(yaml);

            //Assert
            //            string expected = @"
            //- name: Run Selenium smoke tests on website
            //  run: |
            //        $vsTestConsoleExe = ""C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\TestPlatform\vstest.console.exe""
            //        $targetTestDll = ""**\MyProject.FunctionalTests\MyProject.FunctionalTests.dll""
            //        $testRunSettings = ""/Settings:`""${GITHUB_WORKSPACE}/drop/FunctionalTests/MyProject.FunctionalTests/test.runsettings`""
            //        $parameters = """"
            //        #Note that the `"" is an escape character to quote strings, and the `& is needed to start the command
            //        $command = ""`& `""$vsTestConsoleExe`"" `""$targetTestDll`"" $testRunSettings $parameters ""                             
            //        Write-Host ""$command""
            //        Invoke-Expression $command
            //   shell: powershell
            //";
            //expected = TestUtility.TrimNewLines(expected);
            //Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            Assert.IsTrue(string.IsNullOrEmpty(gitHubOutput.actionsYaml) == false);
        }

    }
}