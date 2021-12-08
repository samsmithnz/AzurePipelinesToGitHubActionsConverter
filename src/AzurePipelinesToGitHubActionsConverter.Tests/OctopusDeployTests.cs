using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class OctopusDeployTests
    {

        [TestMethod]
        public void OctopusPackTaskTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: octopusdeploy.octopus-deploy-build-release-tasks.octopus-pack.OctopusPack@4
  displayName: 'Package OctopusSamples.OctoPetShop.Database'
  inputs:
    PackageId: OctopusSamples.OctoPetShop.Database
    PackageFormat: Zip
    PackageVersion: '$(Build.BuildNumber)'
    SourcePath: '$(build.artifactstagingdirectory)\output\OctoPetShop.Database\'
    OutputPath: '$(Build.SourcesDirectory)\output'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
    - name: Package OctopusSamples.OctoPetShop.Database
      run: |
        octo pack --id=OctopusSamples.OctoPetShop.Database --format=Zip --version=$PACKAGE_VERSION --basePath=""$GITHUB_WORKSPACE\output\OctoPetShop.Database\"" --outFolder=""$GITHUB_WORKSPACE\output""
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void OctopusPushTaskTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: octopusdeploy.octopus-deploy-build-release-tasks.octopus-push.OctopusPush@4
  displayName: 'Push Packages to Octopus'
  inputs:
    OctoConnectedServiceName: OctopusWebinars
    Space: 'Spaces-222'
    Package: '$(Build.SourcesDirectory)/output/*.zip'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- # 'Note: requires secrets OCTOPUSSERVERURL and OCTOPUSSERVERAPIKEY to be configured'
  name: Push Packages to Octopus
  run: octo push --package=${{ github.workspace }}/output/*.zip --space=Spaces-222 --server=""${{ secrets.OCTOPUSSERVERURL }}"" --apiKey=""${{ secrets.OCTOPUSSERVERAPIKEY }}""
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void OctopusPipelineTest()
        {           
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
# ASP.NET Core (.NET Framework)
# Build and test ASP.NET Core projects targeting the full .NET Framework.
# Add steps that publish symbols, save build artifacts, and more:
# https://docs.microsoft.com/azure/devops/pipelines/languages/dotnet-core

trigger:
- master

pool:
  vmImage: 'windows-2019'
  demands:
  - msbuild
  - visualstudio

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  displayName: Restore
  inputs:
    command: restore
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  displayName: Build
  inputs:
    projects: '**/*.csproj'
    arguments: '--configuration $(BuildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: Test
  inputs:
    command: test
    projects: '**/*[Tt]ests/*.csproj'
    arguments: '--configuration $(BuildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Publish Web'
  inputs:
    command: publish
    publishWebProjects: false
    projects: '$(Build.SourcesDirectory)\OctopusSamples.OctoPetShop.Web/OctopusSamples.OctoPetShop.Web.csproj'
    arguments: '--configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)\output\OctoPetShop.Web\'
    zipAfterPublish: false
    modifyOutputPath: false

- task: DotNetCoreCLI@2
  displayName: 'Publish Product Service API'
  inputs:
    command: publish
    publishWebProjects: false
    projects: '$(Build.SourcesDirectory)\OctopusSamples.OctoPetShop.ProductService/OctopusSamples.OctoPetShop.ProductService.csproj'
    arguments: '--configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)\output\OctoPetShop.ProductService\'
    zipAfterPublish: false
    modifyOutputPath: false

- task: DotNetCoreCLI@2
  displayName: 'Publish database'
  inputs:
    command: publish
    publishWebProjects: false
    projects: '$(Build.SourcesDirectory)\OctopusSamples.OctoPetShop.Database\OctopusSamples.OctoPetShop.Database.csproj'
    arguments: '--configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)\output\OctoPetShop.Database\ --runtime win-x64'
    zipAfterPublish: false
    modifyOutputPath: false

- task: DotNetCoreCLI@2
  displayName: 'Publish Shopping Cart Service'
  inputs:
    command: publish
    publishWebProjects: false
    projects: '$(Build.SourcesDirectory)\OctopusSamples.OctoPetShop.ShoppingCartService\OctopusSamples.OctoPetShop.ShoppingCartService.csproj'
    arguments: '--configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)\output\OctoPetShop.ShoppingCartService\'
    zipAfterPublish: false
    modifyOutputPath: false

- task: octopusdeploy.octopus-deploy-build-release-tasks.octopus-pack.OctopusPack@4
  displayName: 'Package OctopusSamples.OctoPetShop.Database'
  inputs:
    PackageId: OctopusSamples.OctoPetShop.Database
    PackageFormat: Zip
    PackageVersion: '$(Build.BuildNumber)'
    SourcePath: '$(build.artifactstagingdirectory)\output\OctoPetShop.Database\'
    OutputPath: '$(Build.SourcesDirectory)\output'

- task: octopusdeploy.octopus-deploy-build-release-tasks.octopus-pack.OctopusPack@4
  displayName: 'Package OctopusSamples.OctoPetShop.Web'
  inputs:
    PackageId: OctopusSamples.OctoPetShop.Web
    PackageFormat: Zip
    PackageVersion: '$(Build.BuildNumber)'
    SourcePath: '$(build.artifactstagingdirectory)\output\OctoPetShop.Web\'
    OutputPath: '$(Build.SourcesDirectory)\output'

- task: octopusdeploy.octopus-deploy-build-release-tasks.octopus-pack.OctopusPack@4
  displayName: 'Package OctopusSamples.OctoPetShop.ProductService'
  inputs:
    PackageId: OctopusSamples.OctoPetShop.ProductService
    PackageFormat: Zip
    PackageVersion: '$(Build.BuildNumber)'
    SourcePath: '$(build.artifactstagingdirectory)\output\OctoPetShop.ProductService\'
    OutputPath: '$(Build.SourcesDirectory)\output'

- task: octopusdeploy.octopus-deploy-build-release-tasks.octopus-pack.OctopusPack@4
  displayName: 'Package OctopusSamples.Octopetshop.ShoppingCartService'
  inputs:
    PackageId: OctopusSamples.OctoPetShop.ShoppingCartService
    PackageFormat: Zip
    PackageVersion: '$(Build.BuildNumber)'
    SourcePath: '$(build.artifactstagingdirectory)\output\OctoPetShop.ShoppingCartService\'
    OutputPath: '$(Build.SourcesDirectory)\output'

- task: octopusdeploy.octopus-deploy-build-release-tasks.octopus-push.OctopusPush@4
  displayName: 'Push Packages to Octopus'
  inputs:
    OctoConnectedServiceName: OctopusWebinars
    Space: 'Spaces-222'
    Package: '$(Build.SourcesDirectory)/output/*.zip'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"

";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    

    }
}