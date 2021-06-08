using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [TestClass]
    public class StepsTerraformTests
    {
        [TestMethod]
        public void TerraformInstallerStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: terraformInstaller@0
  displayName: Install Terraform
  inputs:
    terraformVersion: '0.12.12'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Install Terraform
  uses: hashicorp/setup-terraform@v1
  with:
    terraform_version: 0.12.12
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformInstaller2StepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
    - task: ms-devlabs.custom-terraform-tasks.custom-terraform-installer-task.TerraformInstaller@0
      displayName: Install Terraform
      inputs:
        terraformVersion: 0.14.2
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Install Terraform
  uses: hashicorp/setup-terraform@v1
  with:
    terraform_version: 0.14.2
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformInitTaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: terraform@0
  displayName: Terraform Init
  inputs:
    command: 'init'
    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
    backendAzureProviderStorageAccountName: 'mtcdenterraformsandbox'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Init
  run: terraform init
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformNullCommandTaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: terraform@0
  displayName: Terraform Init
  inputs:
    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
    backendAzureProviderStorageAccountName: 'mtcdenterraformsandbox'
    args: -var=environment=demo -out=tfplan.out
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Init
  run: terraform  -var=environment=demo -out=tfplan.out
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformInitV1TaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: TerraformTaskV1@0
  displayName: 'Terraform Init'
  inputs:
    provider: 'azurerm'
    command: 'init'
    backendServiceArm: '$(backendServiceArm)'
    backendAzureRmResourceGroupName: '$(backendAzureRmResourceGroupName)'
    backendAzureRmStorageAccountName: '$(backendAzureRmStorageAccountName)'
    backendAzureRmContainerName: '$(backendAzureRmContainerName)'
    backendAzureRmKey: '$(backendAzureRmKey)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Init
  run: terraform init
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformInitDevLabsTaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
    - task: ms-devlabs.custom-terraform-tasks.custom-terraform-release-task.TerraformTaskV1@0
      displayName: 'Terraform Init'
      inputs:
        command: init
        workingDirectory: tf/env/dev
        backendServiceArm: YAML Template Examples - Dev
        backendAzureRmResourceGroupName: rg-terraformState-dev-eus
        backendAzureRmStorageAccountName: #
        backendAzureRmContainerName: terraform-state
        backendAzureRmKey: terraformStorageExample.tfstate
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Init
  run: terraform init
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformPlanTaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"   
        - task: terraform@0
          displayName: Terraform Plan
          inputs:
            command: 'plan'
            providerAzureConnectedServiceName: 'MTC Denver Sandbox'
            args: -var=environment=demo -out=tfplan.out
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Plan
  run: terraform plan -var=environment=demo -out=tfplan.out
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformPlanV1TaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: TerraformTaskV1@0
  displayName: 'Terraform Plan'
  inputs:
    provider: 'azurerm'
    command: 'plan'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Plan
  run: terraform plan
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformCLIPlanTaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: TerraformCLI@0
  displayName: 'Terraform Plan'
  inputs:
    command: plan
    environmentServiceName: 'My Azure Service Connection'
    # guid for the secure file to use. Can be standard terraform vars file or .env file.
    secureVarsFile: 446e8878-994d-4069-ab56-5b302067a869
    # specify a variable input via pipeline variable
    commandOptions: '-var secret=$(mySecretPipelineVar)'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Plan
  run: terraform plan -var secret=${{ env.mySecretPipelineVar }}
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformApplyTaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"   
        - task: terraform@0
          displayName: Terraform Apply
          inputs:
            command: 'apply'
            providerAzureConnectedServiceName: 'MTC Denver Sandbox'
            args: tfplan.out
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Apply
  run: terraform apply tfplan.out -auto-approve
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformApplyV1TaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: TerraformTaskV1@0
  displayName: 'Terraform Apply'
  inputs:
    provider: 'azurerm'
    command: 'apply'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Apply
  run: terraform apply
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformValidateTaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"   
        - task: terraform@0
          displayName: Terraform Validate
          inputs:
            command: 'validate'
            providerAzureConnectedServiceName: 'MTC Denver Sandbox'
            args: tfplan.out
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Validate
  run: terraform validate tfplan.out
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformValidateV1TaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: TerraformTaskV1@0
  displayName: 'Terraform Validate'
  inputs:
    provider: 'azurerm'
    command: 'validate'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Validate
  run: terraform validate
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformDestroyTaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"   
        - task: terraform@0
          displayName: Terraform Destroy
          inputs:
            command: 'destroy'
            providerAzureConnectedServiceName: 'MTC Denver Sandbox'
            args: tfplan.out
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Destroy
  run: terraform destroy tfplan.out
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformDestroyV1TaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"
- task: TerraformTaskV1@0
  displayName: 'Terraform Destroy'
  inputs:
    provider: 'azurerm'
    command: 'destroy'
";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Terraform Destroy
  run: terraform destroy
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

        [TestMethod]
        public void TerraformCLITaskStepTest()
        {
            //Arrange
            Conversion conversion = new Conversion();
            string yaml = @"   
        - task: terraform@0
          displayName: Execute Terraform CLI Script
          inputs:
            command: 'CLI'
            providerAzureConnectedServiceName: 'MTC Denver Sandbox'
            backendAzureProviderStorageAccountName: 'mtcdenterraformsandbox'
            script: |
              # Validate
              terraform validate

              # Plan
              terraform plan -input=false -out=testplan.tf

              # Get output
              STORAGE_ACCOUNT=`terraform output storage_account`

              # Set storageAccountName variable from terraform output
              echo ""##vso[task.setvariable variable=storageAccountName]$STORAGE_ACCOUNT""
        ";

            //Act
            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

            //Assert
            string expected = @"
- name: Execute Terraform CLI Script
  run: |
    # Validate
    terraform validate

    # Plan
    terraform plan -input=false -out=testplan.tf

    # Get output
    STORAGE_ACCOUNT=`terraform output storage_account`

    # Set storageAccountName variable from terraform output
    echo ""##vso[task.setvariable variable=storageAccountName]$STORAGE_ACCOUNT""
";

            expected = UtilityTests.TrimNewLines(expected);
            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        }

    }
}