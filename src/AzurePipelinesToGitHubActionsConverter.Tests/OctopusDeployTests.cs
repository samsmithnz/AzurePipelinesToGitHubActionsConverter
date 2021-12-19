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
  run: octo pack --id=OctopusSamples.OctoPetShop.Database --format=Zip --version=${{ github.run_number }} --basePath=${{ github.workspace }}\output\OctoPetShop.Database\ --outFolder=${{ github.workspace }}\output
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
#Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet
#Note: requires secrets OCTOPUSSERVERURL and OCTOPUSSERVERAPIKEY to be configured
on:
  push:
    branches:
    - master
env:
  solution: '**/*.sln'
  buildPlatform: Any CPU
  buildConfiguration: Release
jobs:
  build:
    runs-on: windows-2019
    steps:
    - uses: actions/checkout@v2
    - uses: OctopusDeploy/install-octocli@v1
      with:
        version: 7.4.2
    - name: Restore
      run: dotnet restore **/*.csproj
    - name: Build
      run: dotnet **/*.csproj --configuration ${{ env.BuildConfiguration }}
    - name: Test
      run: dotnet test **/*[Tt]ests/*.csproj --configuration ${{ env.BuildConfiguration }}
    - name: Publish Web
      run: dotnet publish ${{ github.workspace }}\OctopusSamples.OctoPetShop.Web/OctopusSamples.OctoPetShop.Web.csproj --configuration ${{ env.BuildConfiguration }} --output ${{ github.workspace }}\output\OctoPetShop.Web\
    - name: Publish Product Service API
      run: dotnet publish ${{ github.workspace }}\OctopusSamples.OctoPetShop.ProductService/OctopusSamples.OctoPetShop.ProductService.csproj --configuration ${{ env.BuildConfiguration }} --output ${{ github.workspace }}\output\OctoPetShop.ProductService\
    - name: Publish database
      run: dotnet publish ${{ github.workspace }}\OctopusSamples.OctoPetShop.Database\OctopusSamples.OctoPetShop.Database.csproj --configuration ${{ env.BuildConfiguration }} --output ${{ github.workspace }}\output\OctoPetShop.Database\ --runtime win-x64
    - name: Publish Shopping Cart Service
      run: dotnet publish ${{ github.workspace }}\OctopusSamples.OctoPetShop.ShoppingCartService\OctopusSamples.OctoPetShop.ShoppingCartService.csproj --configuration ${{ env.BuildConfiguration }} --output ${{ github.workspace }}\output\OctoPetShop.ShoppingCartService\
    - name: Package OctopusSamples.OctoPetShop.Database
      run: octo pack --id=OctopusSamples.OctoPetShop.Database --format=Zip --version=${{ github.run_number }} --basePath=${{ github.workspace }}\output\OctoPetShop.Database\ --outFolder=${{ github.workspace }}\output
    - name: Package OctopusSamples.OctoPetShop.Web
      run: octo pack --id=OctopusSamples.OctoPetShop.Web --format=Zip --version=${{ github.run_number }} --basePath=${{ github.workspace }}\output\OctoPetShop.Web\ --outFolder=${{ github.workspace }}\output
    - name: Package OctopusSamples.OctoPetShop.ProductService
      run: octo pack --id=OctopusSamples.OctoPetShop.ProductService --format=Zip --version=${{ github.run_number }} --basePath=${{ github.workspace }}\output\OctoPetShop.ProductService\ --outFolder=${{ github.workspace }}\output
    - name: Package OctopusSamples.Octopetshop.ShoppingCartService
      run: octo pack --id=OctopusSamples.OctoPetShop.ShoppingCartService --format=Zip --version=${{ github.run_number }} --basePath=${{ github.workspace }}\output\OctoPetShop.ShoppingCartService\ --outFolder=${{ github.workspace }}\output
    - # 'Note: requires secrets OCTOPUSSERVERURL and OCTOPUSSERVERAPIKEY to be configured'
      name: Push Packages to Octopus
      run: octo push --package=${{ github.workspace }}/output/*.zip --space=Spaces-222 --server=""${{ secrets.OCTOPUSSERVERURL }}"" --apiKey=""${{ secrets.OCTOPUSSERVERAPIKEY }}""
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}