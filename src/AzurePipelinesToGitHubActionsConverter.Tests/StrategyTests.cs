using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StrategyTests
    {

        [TestMethod]
        public void StrategyTest()
        {
            //strategy:
            //  matrix:
            //    linux:
            //      imageName: "ubuntu-16.04"
            //    mac:
            //      imageName: "macos-10.13"
            //    windows:
            //      imageName: "vs2017-win2016"

            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master
strategy:
  maxParallel: 3
  parallel: 1
  matrix:
    linux:
      imageName: ubuntu-16.04
    mac:
      imageName: macos-10.13
    windows:
      imageName: vs2017-win2016
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: $(imageName)
  variables:
    buildConfiguration: Debug
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build part 1
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            //Note that we are using the longer form, as sequence flow (showing an array like: [ubuntu-16.04, macos-10.13, vs2017-win2016]), doesn't exist in this YAML Serializer yet.
            string expected = @"
on:
  push:
    branches:
    - master
jobs:
  Build:
    name: Build job
    runs-on: ${{ matrix.imageName }}
    strategy:
      matrix:
        imageName:
        - ubuntu-16.04
        - macos-10.13
        - vs2017-win2016
      max-parallel: 3
    env:
      buildConfiguration: Debug
    steps:
    - uses: actions/checkout@v2
    - name: dotnet build part 1
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void StrategyWithMaxParallelOnlyTest()
        {
            //strategy:
            //  matrix:
            //    linux:
            //      imageName: "ubuntu-16.04"
            //    mac:
            //      imageName: "macos-10.13"
            //    windows:
            //      imageName: "vs2017-win2016"

            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- master
strategy:
  maxParallel: 3
jobs:
- job: Build
  displayName: Build job
  pool: 
    vmImage: $(imageName)
  variables:
    buildConfiguration: Debug
  steps: 
  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
    displayName: dotnet build part 1
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            //Note that we are using the longer form, as sequence flow (showing an array like: [ubuntu-16.04, macos-10.13, vs2017-win2016]), doesn't exist in this YAML Serializer yet.
            string expected = @"
on:
  push:
    branches:
    - master
jobs:
  Build:
    name: Build job
    runs-on: ${{ env.imageName }}
    strategy:
      max-parallel: 3
    env:
      buildConfiguration: Debug
    steps:
    - uses: actions/checkout@v2
    - name: dotnet build part 1
      run: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration ${{ env.buildConfiguration }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void StrategyRunOnceSimpleEnvironmentDeploymentTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string input = @"
jobs:
  - deployment: DeployInfrastructure
    displayName: Deploy job
    environment: Dev
    pool:
      vmImage: windows-latest
    strategy:
      runOnce:
        deploy:
          steps:
          - task: PowerShell@2
            displayName: 'Test'
            inputs:
              targetType: inline
              script: |
                Write-Host ""Hello world""";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps
jobs:
  DeployInfrastructure:
    # 'Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps'
    name: Deploy job
    runs-on: windows-latest
    environment:
      name: Dev
    steps:
    - name: Test
      run: Write-Host ""Hello world""
      shell: powershell";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            
        }

        [TestMethod]
        public void StrategyRunOnceWithComplexEnvironmentsDeploymentTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string input = @"
jobs:
  - deployment: DeployInfrastructure
    displayName: Deploy job
    environment: 
      name: windows-server
      resourceName: rName
      resourceId: rId
      resourceType: VirtualMachine
      tags: web
    pool:
      vmImage: windows-latest
    strategy:
      runOnce:
        deploy:
          steps:
          - task: PowerShell@2
            displayName: 'Test'
            inputs:
              targetType: inline
              script: |
                Write-Host ""Hello world""";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps
jobs:
  DeployInfrastructure:
    # 'Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps'
    name: Deploy job
    runs-on: windows-latest
    environment:
      name: windows-server
    steps:
    - name: Test
      run: Write-Host ""Hello world""
      shell: powershell";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void StrategyRunOnceDeploymentTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string input = @"
jobs:
  # Track deployments on the environment.
- deployment: DeployWeb
  displayName: deploy Web App
  pool:
    vmImage: 'Ubuntu-16.04'
  # Creates an environment if it doesn't exist.
  environment: 'smarthotel-dev'
  strategy:
    # Default deployment strategy, more coming...
    runOnce:
      preDeploy:
        steps:
        - download: current
          artifact: drop
        - script: echo initialize, cleanup, backup, install certs
      deploy:
        pool: 
          name: smarthotel-devPool  
        steps:
        - script: echo Deploy application to Website
      routeTraffic:
        steps:
        - script: echo routing traffic
      postRouteTraffic:
        steps:
        - script: echo health check post-route traffic
      on:
        failure:
          steps:
          - script: echo Restore from backup! This is on failure
        success:
          steps:
          - script: echo Notify! This is on success
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps
jobs:
  DeployWeb:
    # 'Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps'
    name: deploy Web App
    runs-on: Ubuntu-16.04
    environment:
      name: smarthotel-dev
    steps:
    - run: echo Deploy application to Website
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }

        [TestMethod]
        public void StrategyRollingDeploymentTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string input = @"
jobs: 
- deployment: VMDeploy
  displayName: web
  environment:
    name: smarthotel-dev
    resourceType: VirtualMachine
  strategy:
    rolling:
      maxParallel: 5  #for percentages, mention as x%
      preDeploy:
        steps:
        - download: current
          artifact: drop
        - script: echo initialize, cleanup, backup, install certs
      deploy:
        pool: 
          name: smarthotel-devPool  
        steps:
        - script: echo Deploy application to Website
      routeTraffic:
        steps:
        - script: echo routing traffic
      postRouteTraffic:
        steps:
        - script: echo health check post-route traffic
      on:
        failure:
          steps:
          - script: echo Restore from backup! This is on failure
        success:
          steps:
          - script: echo Notify! This is on success";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: Azure DevOps strategy>rolling does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps
jobs:
  VMDeploy:
    # 'Note: Azure DevOps strategy>rolling does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps'
    name: web
    environment:
      name: smarthotel-dev
    steps:
    - run: echo Deploy application to Website
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            
        }

        [TestMethod]
        public void StrategyCanaryDeploymentTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string input = @"
jobs: 
- deployment: VMDeploy
  environment: smarthotel-dev.bookings
  pool: 
    name: smarthotel-devPool
  strategy:
    canary:      
      increments: [10,20]  
      preDeploy:
        steps:
        - download: current
          artifact: drop
        - script: echo initialize, cleanup, backup, install certs
      deploy:
        pool: 
          name: smarthotel-devPool    
        steps:
        - script: echo Deploy application to Website
      routeTraffic:
        steps:
        - script: echo routing traffic
      postRouteTraffic:
        steps:
        - script: echo health check post-route traffic
      on:
        failure:
          steps:
          - script: echo Restore from backup! This is on failure
        success:
          steps:
          - script: echo Notify! This is on success
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Assert
            string expected = @"
#Note: Azure DevOps strategy>canary does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps
jobs:
  VMDeploy:
    # 'Note: Azure DevOps strategy>canary does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps'
    runs-on: smarthotel-devPool
    environment:
      name: smarthotel-dev.bookings
    steps:
    - run: echo Deploy application to Website
";
            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
            
        }
    }
}