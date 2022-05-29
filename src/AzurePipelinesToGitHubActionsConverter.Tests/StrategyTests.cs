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
- main
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
    - main
jobs:
  Build:
    name: Build job
    strategy:
      matrix:
        imageName:
        - ubuntu-16.04
        - macos-10.13
        - vs2017-win2016
      max-parallel: 3
    runs-on: ${{ matrix.imageName }}
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
- main
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
    - main
jobs:
  Build:
    name: Build job
    strategy:
      max-parallel: 3
    runs-on: ${{ env.imageName }}
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
    runs-on: smarthotel-devPool
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
    runs-on: smarthotel-devPool
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
    name: smarthotel-mainPool
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



        [TestMethod]
        public void StrategyWithMatrixPipelineTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
trigger:
- main

variables:
  # It's important to put these in a path that won't change
  PARAVIEW_SOURCE_FOLDER: /tmp/paraview_source
  PARAVIEW_BUILD_FOLDER: /tmp/paraview_build

jobs:
- job: Build
  timeoutInMinutes: 0

  strategy:
    matrix:
      Linux:
        imageName: 'ubuntu-18.04'
      Mac:
        imageName: 'macos-10.14'
      Windows:
        imageName: 'vs2017-win2016'
        # The D:\ drive (default) on Windows only has about 4 GB of disk
        # space available, which is not enough to build ParaView.
        # But the C:\ drive has a lot of free space, around 150 GB.
        PARAVIEW_SOURCE_FOLDER: 'C:\paraview_source'
        PARAVIEW_BUILD_FOLDER: 'C:\paraview_build'

  pool:
    vmImage: $(imageName)

  steps:
  - checkout: self
    submodules: true

  - task: UsePythonVersion@0
    inputs:
      versionSpec: 3.8
    displayName: Enable Python 3.8

  - bash: scripts/azure-pipelines/install.sh
    displayName: Install Dependencies

  - bash: scripts/azure-pipelines/install_python_deps.sh
    displayName: Install Python Dependencies

  # Use the OS's native script language for this command
  - script: git clone --recursive https://github.com/openchemistry/paraview $(PARAVIEW_SOURCE_FOLDER)
    displayName: Clone ParaView

  - bash: scripts/azure-pipelines/prepend_paths.sh
    displayName: Prepend Paths

  # This will set up the MSVC environment for future commands
  - task: BatchScript@1
    inputs:
      filename: scripts/azure-pipelines/setup_msvc_env.bat
      modifyEnvironment: True
    condition: eq(variables['Agent.OS'], 'Windows_NT')
    displayName: Setup MSVC Environment

  # Creates a ""deps_md5sum"" variable that, when this has changed,
  # automatically re-build paraview.
  - bash: scripts/azure-pipelines/create_deps_md5sum.sh
    displayName: Create Dependency md5sum

  - task: Cache@2
    inputs:
      # Change the ""v*"" at the end to force a re-build
      key: paraview | $(Agent.OS) | $(deps_md5sum) | v2
      path: $(PARAVIEW_BUILD_FOLDER)
      cacheHitVar: PARAVIEW_BUILD_RESTORED
    displayName: Restore ParaView Build

  - bash: scripts/azure-pipelines/build_paraview.sh
    condition: ne(variables.PARAVIEW_BUILD_RESTORED, 'true')
    displayName: Build ParaView

  - bash: scripts/azure-pipelines/build_tomviz.sh
    displayName: Build Tomviz

  - bash: scripts/azure-pipelines/run_ctest.sh
    displayName: Run CTest

  - bash: scripts/azure-pipelines/run_pytest.sh
    displayName: Run PyTest

- job: clang_format
  pool:
    vmImage: 'ubuntu-18.04'
  steps:
  - bash: scripts/azure-pipelines/run_clang_format_diff.sh
    displayName: Run clang-format

- job: flake8
  pool:
    vmImage: 'ubuntu-18.04'
  steps:
  - bash: scripts/azure-pipelines/run_flake8.sh
    displayName: Run flake8
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(yaml);

            //Assert
            string expected = @"
on:
  push:
    branches:
    - main
env:
  PARAVIEW_SOURCE_FOLDER: /tmp/paraview_source
  PARAVIEW_BUILD_FOLDER: /tmp/paraview_build
jobs:
  Build:
    strategy:
      matrix:
        imageName:
        - ubuntu-18.04
        - macos-10.14
        - vs2017-win2016
    runs-on: ${{ matrix.imageName }}
    steps:
    - uses: actions/checkout@v2
    - name: Enable Python 3.8
      uses: actions/setup-python@v1
      with:
        python-version: 3.8
    - name: Install Dependencies
      run: scripts/azure-pipelines/install.sh
      shell: bash
    - name: Install Python Dependencies
      run: scripts/azure-pipelines/install_python_deps.sh
      shell: bash
    - name: Clone ParaView
      run: git clone --recursive https://github.com/openchemistry/paraview ${{ env.PARAVIEW_SOURCE_FOLDER }}
    - name: Prepend Paths
      run: scripts/azure-pipelines/prepend_paths.sh
      shell: bash
    - name: Setup MSVC Environment
      run: scripts/azure-pipelines/setup_msvc_env.bat
      shell: cmd
      if: (runner.os == 'Windows_NT')
    - name: Create Dependency md5sum
      run: scripts/azure-pipelines/create_deps_md5sum.sh
      shell: bash
    - name: Restore ParaView Build
      uses: actions/cache@v3
      with:
        key: paraview | ${{ runner.os }} | ${{ env.deps_md5sum }} | v2
        restore-keys: 
        path: ${{ env.PARAVIEW_BUILD_FOLDER }}
    - name: Build ParaView
      run: scripts/azure-pipelines/build_paraview.sh
      shell: bash
      if: (variables.PARAVIEW_BUILD_RESTORED != 'true')
    - name: Build Tomviz
      run: scripts/azure-pipelines/build_tomviz.sh
      shell: bash
    - name: Run CTest
      run: scripts/azure-pipelines/run_ctest.sh
      shell: bash
    - name: Run PyTest
      run: scripts/azure-pipelines/run_pytest.sh
      shell: bash
  clang_format:
    strategy:
      matrix:
        imageName:
        - ubuntu-18.04
        - macos-10.14
        - vs2017-win2016
    runs-on: ubuntu-18.04
    steps:
    - uses: actions/checkout@v2
    - name: Run clang-format
      run: scripts/azure-pipelines/run_clang_format_diff.sh
      shell: bash
  flake8:
    strategy:
      matrix:
        imageName:
        - ubuntu-18.04
        - macos-10.14
        - vs2017-win2016
    runs-on: ubuntu-18.04
    steps:
    - uses: actions/checkout@v2
    - name: Run flake8
      run: scripts/azure-pipelines/run_flake8.sh
      shell: bash
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);

        }
    }
}