using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System.Collections.Generic;
using System.Linq;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class AzurePipelinesStepsProcessing
    {
        //This section is very much in Alpha. It has long way to go.
        public GitHubActions.Step ProcessStep(AzurePipelines.Step step)
        {
            if (step.task != null)
            {
                GitHubActions.Step gitHubStep;
                switch (step.task)
                {
                    case "AzureAppServiceManage@0":
                        gitHubStep = CreateAzureManageResourcesStep(step);
                        break;
                    case "AzureResourceGroupDeployment@2":
                        gitHubStep = CreateScriptStep("powershell", step);
                        break;
                    case "AzureRmWebAppDeployment@3":
                        gitHubStep = CreateScriptStep("powershell", step);
                        break;
                    case "CmdLine@2":
                        gitHubStep = CreateScriptStep("cmd", step);
                        break;
                    case "CopyFiles@2":
                        //Use PowerShell to copy files
                        step.script = "Copy " + step.inputs["SourceFolder"] + "/" + step.inputs["Contents"] + " " + step.inputs["TargetFolder"];
                        gitHubStep = CreateScriptStep("powershell", step);
                        break;
                    case "DotNetCoreCLI@2":
                        gitHubStep = CreateDotNetCommandStep(step);
                        break;
                    case "DownloadBuildArtifacts@0":
                        gitHubStep = CreateScriptStep("powershell", step);
                        break;
                    case "PowerShell@2":
                        gitHubStep = CreateScriptStep("powershell", step);
                        break;
                    case "PublishBuildArtifacts@1":
                        gitHubStep = CreatePublishBuildArtifactsStep(step);
                        break;
                    case "UseDotNet@2":
                        gitHubStep = new GitHubActions.Step
                        {
                            name = step.displayName,
                            uses = "actions/setup-dotnet@v1",
                            with = new Dictionary<string, string>
                            {
                                {"dotnet-version", step.inputs["version"] }
                            }
                        };
                        //Pipelines
                        //- task: UseDotNet@2
                        //  displayName: 'Use .NET Core sdk'
                        //  inputs:
                        //    packageType: sdk
                        //    version: 2.2.203
                        //    installationPath: $(Agent.ToolsDirectory)/dotnet

                        //Actions
                        //- uses: actions/setup-dotnet@v1
                        //  with:
                        //    dotnet-version: '2.2.103' # SDK Version to use.
                        break;
                    case "VSTest@2":
                        gitHubStep = CreateScriptStep("powershell", step);
                        break;
                    default:
                        return new GitHubActions.Step
                        {
                            name = "***This step is not currently supported***: " + step.displayName
                      
                        };
                }

                return gitHubStep;
            }
            else if (step.script != null)
            {
                return new GitHubActions.Step
                {
                    name = step.displayName,
                    run = step.script,
                    with = step.inputs
                };
            }
            else if (step.pwsh != null)
            {
                return CreateScriptStep("pwsh", step);
            }
            else if (step.powershell != null)
            {
                return CreateScriptStep("powershell", step);
            }
            else if (step.bash != null)
            {
                return CreateScriptStep("bash", step);
            }
            else
            {
                return null;
            }
        }

        private GitHubActions.Step CreateDotNetCommandStep(AzurePipelines.Step step)
        {

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                run = "dotnet " +
                    step.inputs["command"] + " " +
                    step.inputs["projects"] + " " +
                    step.inputs["arguments"]
            };

            //Remove the new line characters
            gitHubStep.run = gitHubStep.run.Replace("\n", "");

            return gitHubStep;
        }

        private GitHubActions.Step CreateScriptStep(string shellType, AzurePipelines.Step step)
        {
            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                run = step.script,
                shell = shellType//,
                //with = step.inputs
            };

            if (gitHubStep.run == null)
            {
                if (step.powershell != null)
                {
                    gitHubStep.run = step.powershell;
                }
                else if (step.pwsh != null)
                {
                    gitHubStep.run = step.pwsh;
                }
                else if (step.bash != null)
                {
                    gitHubStep.run = step.bash;
                }
                else
                {
                    step.inputs.TryGetValue("script", out string value);
                    gitHubStep.run = value;
                }
            }

            return gitHubStep;
        }

        private GitHubActions.Step CreateAzureLoginStep()
        {
            //https://github.com/Azure/github-actions/tree/master/login
            // action "Azure Login" {
            //  uses = "Azure/github-actions/login@master"
            //  env = {
            //    AZURE_SUBSCRIPTION = "Subscription Name"
            //  }
            //  secrets = ["AZURE_SERVICE_APP_ID", "AZURE_SERVICE_PASSWORD", "AZURE_SERVICE_TENANT"]
            //}
            return null;
        }

        private GitHubActions.Step CreateAzureManageResourcesStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: AzureResourceGroupDeployment@2
            //  displayName: 'Deploy ARM Template to resource group'
            //  inputs:
            //    azureSubscription: 'SamLearnsAzure connection to Azure Portal'
            //    resourceGroupName: $(ResourceGroupName)
            //    location: '[resourceGroup().location]'
            //    csmFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.json'
            //    csmParametersFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.parameters.json'
            //    overrideParameters: '-environment $(AppSettings.Environment) -locationShort $(ArmTemplateResourceGroupLocation)'

            //https://github.com/Azure/github-actions/tree/master/arm
            //action "Manage Azure Resources" {
            //  uses = "Azure/github-actions/arm@master"
            //  env = {
            //    AZURE_RESOURCE_GROUP = "<Resource Group Name"
            //    AZURE_TEMPLATE_LOCATION = "<URL or Relative path in your repository>"
            //    AZURE_TEMPLATE_PARAM_FILE = "<URL or Relative path in your repository>"
            //  }
            //  needs = ["Azure Login"]
            //}}

            step.inputs.TryGetValue("resourceGroupName", out string resourceGroup);
            step.inputs.TryGetValue("csmFile", out string armTemplateFile);
            step.inputs.TryGetValue("csmParametersFile", out string armTemplateParametersFile);

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                uses = "Azure/github-actions/arm@master",
                env = new Dictionary<string, string>
                {
                    { "AZURE_RESOURCE_GROUP", resourceGroup},
                    { "AZURE_TEMPLATE_LOCATION", armTemplateFile},
                    { "AZURE_TEMPLATE_PARAM_FILE", armTemplateParametersFile},
                }
            };

            return null;
        }

        private GitHubActions.Step CreatePublishBuildArtifactsStep(AzurePipelines.Step step)
        {
            //# Publish the artifacts
            //- task: PublishBuildArtifacts@1
            //  displayName: 'Publish Artifact'
            //  inputs:
            //    PathtoPublish: '$(build.artifactstagingdirectory)'";

            //- name: publish build artifacts back to GitHub
            //  uses: actions/upload-artifact@master
            //  with:
            //    name: console exe
            //    path: /home/runner/work/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter.ConsoleApp/bin/Release/netcoreapp3.0

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                uses = "actions/upload-artifact@master",
                with = new Dictionary<string, string>
                {
                    {"path", step.inputs["PathtoPublish"] }
                }
            };
            //In publish task, I we need to delete any usage of build.artifactstagingdirectory variable as it's implied in github actions, and therefore not needed (Adding it adds the path twice)
            gitHubStep.with["path"].Replace("$(build.artifactstagingdirectory)", "");

            return gitHubStep;
        }
    }
}
