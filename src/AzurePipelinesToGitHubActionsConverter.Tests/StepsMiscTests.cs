using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StepsMiscTests
    {
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
  uses: actions/cache@v3
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
        public void DownloadBuildArtifactsNullDownloadIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
          - task: DownloadBuildArtifacts@0
            displayName: 'Download Build Artifact'
            inputs:
              buildType: 'current'
              artifactName: $(buildArtifactName)
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Download Build Artifact
  uses: actions/download-artifact@v2
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
  uses: gittools/actions/gitversion/execute@v0.9.11
  with:
    configFilePath: GitVersion.yml
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void GitVersionSetupIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: gitversion/setup@0
  displayName: 'Install GitVersion'
  inputs:
    versionSpec: '5.x'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Install GitVersion
  uses: gittools/actions/gitversion/setup@v0.9.11
  with:
    versionSpec: 5.x
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void GitVersionExecuteIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: gitversion/execute@0
  displayName: 'Evaluate Next Version'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Evaluate Next Version
  uses: gittools/actions/gitversion/execute@v0.9.11
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

        [TestMethod]
        public void InnerPowershellIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: InlinePowershell@1                
  displayName: 'old Powershell task'
  inputs:
    Script: |
      Write-Host 'Hello World'
      Write-Host 'Hello World2'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: old Powershell task
  run: |
    Write-Host 'Hello World'
    Write-Host 'Hello World2'
  shell: powershell
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
        public void NPMNullCommandStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: Npm@1
  displayName: 'npm install'
  inputs:
    workingDir: src/angular7
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: npm install
  run: npm src/angular7
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
            //Only for JUNIT... need .NET samples too.
            //            string expected = @"
            //- uses: EnricoMi/publish-unit-test-result-action@v1
            //  if: always()
            //  with:
            //    files: '**/*.trx'

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

        [TestMethod]
        public void UseNodeStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: UseNode@1
  displayName: 'Use Node.js 8.10.0'
  inputs:
    version: '8.10.0'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Use Node.js 8.10.0
  uses: actions/setup-node@v2
  with:
    node-version: 8.10.0
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