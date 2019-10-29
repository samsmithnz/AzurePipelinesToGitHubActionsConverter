using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [TestClass]
    public class LargePipelineTest
    {

        [TestMethod]
        public void TestLargePipeline()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
# ASP.NET Core
# Build and test ASP.NET Core projects targeting .NET Core.
# Add steps that run tests, create a NuGet package, deploy, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core
trigger:
- master
pr:
  branches:
    include:
    - '*'  
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
    - task: UseDotNet@2
      displayName: 'Use .NET Core sdk'
      inputs:
        packageType: sdk
        version: 2.2.203
    - task: PowerShell@2
      displayName: 'Generate build version number'
      inputs:
        targetType: 'inline'
        script: |
         Write-Host ""Generating Build Number""
         #Get the version from the csproj file
         $xml = [Xml] (Get-Content FeatureFlags/FeatureFlags.Web/FeatureFlags.Web.csproj)
         $initialVersion = [Version] $xml.Project.PropertyGroup.Version
         Write-Host ""Initial Version: "" $version
         $spliteVersion = $initialVersion -Split ""\.""
         #Get the build number (number of days since January 1, 2000)
         $baseDate = [datetime]""01/01/2000""
         $currentDate = $(Get-Date)
         $interval = (NEW-TIMESPAN -Start $baseDate -End $currentDate)
         $buildNumber = $interval.Days
         #Get the revision number (number seconds (divided by two) into the day on which the compilation was performed)
         $StartDate=[datetime]::Today
         $EndDate=(GET-DATE)
         $revisionNumber = [math]::Round((New-TimeSpan -Start $StartDate -End $EndDate).TotalSeconds / 2,0)
         #Final version number
         $finalBuildVersion = ""$($spliteVersion[0]).$($spliteVersion[1]).$($buildNumber).$($revisionNumber)""
         Write-Host ""Major.Minor,Build,Revision""
         Write-Host ""Final build number: "" $finalBuildVersion
         #Writing final version number back to Azure DevOps variable
         Write-Host ""##vso[task.setvariable variable=buildNumber]$finalBuildVersion""

    - task: CopyFiles@2
      displayName: 'Copy environment ARM template files to: $(build.artifactstagingdirectory)'
      inputs:
        SourceFolder: '$(system.defaultworkingdirectory)\FeatureFlags\FeatureFlags.ARMTemplates'
        Contents: '**\*' # **\* = Copy all files and all files in sub directories
        TargetFolder: '$(build.artifactstagingdirectory)\ARMTemplates'

    - task: DotNetCoreCLI@2
      displayName: 'Test dotnet code projects'
      inputs:
        command: test
        projects: |
         FeatureFlags/FeatureFlags.Tests/FeatureFlags.Tests.csproj
        arguments: '--configuration $(buildConfiguration) --logger trx --collect ""Code coverage"" --settings:$(Build.SourcesDirectory)\FeatureFlags\FeatureFlags.Tests\CodeCoverage.runsettings'

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

    - task: DotNetCoreCLI@2
      displayName: 'Publish dotnet core functional tests project'
      inputs:
        command: publish
        publishWebProjects: false
        projects: |
         FeatureFlags/FeatureFlags.FunctionalTests/FeatureFlags.FunctionalTests.csproj
        arguments: '--configuration $(buildConfiguration) --output $(build.artifactstagingdirectory)/FunctionalTests'
        zipAfterPublish: false

    - task: CopyFiles@2
      displayName: 'Copy Selenium Files to: $(build.artifactstagingdirectory)/FunctionalTests/FeatureFlags.FunctionalTests'
      inputs:
        SourceFolder: 'FeatureFlags/FeatureFlags.FunctionalTests/bin/$(buildConfiguration)/netcoreapp3.0'
        Contents: '*chromedriver.exe*'
        TargetFolder: '$(build.artifactstagingdirectory)/FunctionalTests/FeatureFlags.FunctionalTests'

    # Publish the artifacts
    - task: PublishBuildArtifacts@1
      displayName: 'Publish Artifact'
      inputs:
        PathtoPublish: '$(build.artifactstagingdirectory)'";

            //Act
            string output = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            //string expectedOutput = @"";
            Assert.IsTrue(output.IndexOf("unknown") == -1);
            //Assert.AreEqual(Environment.NewLine + output, expectedOutput);
      //- name: publish build artifacts back to GitHub
      //  uses: actions/upload-artifact@master
      //  with:
      //    name: console exe
      //    path: /home/runner/work/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter.ConsoleApp/bin/Release/netcoreapp3.0
        }

    }
}