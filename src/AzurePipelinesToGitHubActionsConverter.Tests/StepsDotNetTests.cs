using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StepsDotNetTests
    {

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
        public void DotNetBuildIndividualStepTest()
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
        public void DotNetCoreCLIPublishMultiLineIndividualStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: DotNetCoreCLI@2
  displayName: 'Publish dotnet projects'
  inputs:
    command: publish
    publishWebProjects: false
    projects: |
      src/Project.Service/Project.Service.csproj
      src/Project.Web/Project.Web.csproj
    arguments: '--configuration Release --output $(build.artifactstagingdirectory) --runtime win-x64'
    zipAfterPublish: true
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Publish dotnet projects
  run: |
    dotnet publish src/Project.Service/Project.Service.csproj --configuration Release --output ${{ github.workspace }} --runtime win-x64 
    dotnet publish src/Project.Web/Project.Web.csproj --configuration Release --output ${{ github.workspace }} --runtime win-x64
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


    }
}