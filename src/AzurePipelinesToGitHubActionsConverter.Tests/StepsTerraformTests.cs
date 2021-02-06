using AzurePipelinesToGitHubActionsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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

        //        [TestMethod]
        //        public void TerraformPlanTaskStepTest()
        //        {
        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            string yaml = @"   
        //- task: terraform@0
        //  displayName: Terraform Plan
        //  inputs:
        //    command: 'plan'
        //    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
        //    args: -var=environment=demo -out=tfplan.out
        //        ";

        //            //Act
        //            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

        //            //Assert
        //            string expected = @"
        //- name: Terraform Plan
        //  id: plan
        //  run: terraform plan -no-color
        //  continue-on-error: true
        //";

        //            expected = UtilityTests.TrimNewLines(expected);
        //            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //        }

        //        [TestMethod]
        //        public void TerraformApplyTaskStepTest()
        //        {
        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            string yaml = @"   
        //- task: terraform@0
        //  displayName: Terraform Apply
        //  inputs:
        //    command: 'apply'
        //    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
        //    args: tfplan.out
        //        ";

        //            //Act
        //            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

        //            //Assert
        //            string expected = @"

        //";

        //            expected = UtilityTests.TrimNewLines(expected);
        //            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //        }

        //        [TestMethod]
        //        public void TerraformValidateTaskStepTest()
        //        {
        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            string yaml = @"   
        //- task: terraform@0
        //  displayName: Terraform Validate
        //  inputs:
        //    command: 'validate'
        //    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
        //    args: tfplan.out
        //        ";

        //            //Act
        //            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

        //            //Assert
        //            string expected = @"
        //- name: Terraform Validate
        //  id: validate
        //  run: terraform validate -no-color
        //";

        //            expected = UtilityTests.TrimNewLines(expected);
        //            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //        }

        //        [TestMethod]
        //        public void TerraformDestroyTaskStepTest()
        //        {
        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            string yaml = @"   
        //- task: terraform@0
        //  displayName: Terraform Destroy
        //  inputs:
        //    command: 'destroy'
        //    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
        //    args: tfplan.out
        //        ";

        //            //Act
        //            ConversionResponse gitHubOutput = conversion.ConvertAzurePipelineTaskToGitHubActionTask(yaml);

        //            //Assert
        //            string expected = @"

        //";

        //            expected = UtilityTests.TrimNewLines(expected);
        //            Assert.AreEqual(expected, gitHubOutput.actionsYaml);
        //        }

        //        [TestMethod]
        //        public void TerraformCLITaskStepTest()
        //        {
        //            //Arrange
        //            Conversion conversion = new Conversion();
        //            string yaml = @"   
        //- task: terraform@0
        //  displayName: Execute Terraform CLI Script
        //  inputs:
        //    command: 'CLI'
        //    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
        //    backendAzureProviderStorageAccountName: 'mtcdenterraformsandbox'
        //    script: |
        //      # Validate
        //      terraform validate

        //      # Plan
        //      terraform plan -input=false -out=testplan.tf

        //      # Get output
        //      STORAGE_ACCOUNT=`terraform output storage_account`

        //      # Set storageAccountName variable from terraform output
        //      echo ""##vso[task.setvariable variable=storageAccountName]$STORAGE_ACCOUNT""
        //        ";

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