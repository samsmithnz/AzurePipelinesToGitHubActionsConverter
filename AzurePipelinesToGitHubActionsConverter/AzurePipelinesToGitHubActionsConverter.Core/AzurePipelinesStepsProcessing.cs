using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class AzurePipelinesStepsProcessing
    {
        //This section is very much in Alpha. It has long way to go.
        //TODO: Add more task types
        //TODO: Add logic to handle different versions of tasks
        public GitHubActions.Step ProcessStep(AzurePipelines.Step step)
        {
            if (step.task != null)
            {
                step = CleanStepInputs(step);

                GitHubActions.Step gitHubStep;
                switch (step.task)
                {
                    case "AzureAppServiceManage@0":
                        gitHubStep = CreateAzureAppServiceManageStep(step);
                        break;
                    case "AzureResourceGroupDeployment@2":
                        gitHubStep = CreateAzureManageResourcesStep(step);
                        break;
                    case "AzureRmWebAppDeployment@3":
                        gitHubStep = CreateAzureWebAppDeploymentStep(step);
                        break;
                    case "CmdLine@2":
                        gitHubStep = CreateScriptStep("cmd", step);
                        break;
                    case "CopyFiles@2":
                        gitHubStep = CreateCopyFilesStep(step);
                        break;
                    case "DotNetCoreCLI@2":
                        gitHubStep = CreateDotNetCommandStep(step);
                        break;
                    case "DownloadBuildArtifacts@0":
                        gitHubStep = CreateDownloadBuildArtifacts(step);
                        break;
                    case "PowerShell@2":
                        gitHubStep = CreateScriptStep("powershell", step);
                        break;
                    case "PublishPipelineArtifact@0":
                    case "PublishBuildArtifacts@1":
                        gitHubStep = CreatePublishBuildArtifactsStep(step);
                        break;
                    case "UseDotNet@2":
                        gitHubStep = CreateUseDotNetStep(step);
                        break;
                    case "VSTest@2":
                        gitHubStep = CreateSeleniumTestingStep(step);
                        break;

                    default:
                        gitHubStep = CreateScriptStep("powershell", step);
                        if (step.displayName != null)
                        {
                            gitHubStep.name = step.displayName;
                        }
                        string newYaml = Global.WriteYAMLFile<AzurePipelines.Step>(step);
                        string[] newYamlSplit = newYaml.Split(Environment.NewLine);
                        StringBuilder yamlBuilder = new StringBuilder();
                        for (int i = 0; i < newYamlSplit.Length; i++)
                        {
                            string line = newYamlSplit[i];
                            if (line.Trim().Length > 0)
                            {
                                yamlBuilder.Append("#");
                                yamlBuilder.Append(line);
                                //if (i < newYamlSplit.Length - 1)
                                //{
                                //    yamlBuilder.Append(Environment.NewLine);
                                //}
                            }
                        }
                        gitHubStep.run = yamlBuilder.ToString();
                        gitHubStep.step_message = "NOTE: This step does not have a conversion path yet: " + step.task;
                        break;
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

        //Convert all of the input keys to lowercase, to make pattern matching easier later
        private AzurePipelines.Step CleanStepInputs(AzurePipelines.Step step)
        {
            Dictionary<string, string> newInputs = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> item in step.inputs)
            {
                newInputs.Add(item.Key.ToLower(), item.Value);
            }
            step.inputs = newInputs;

            return step;
        }

        private GitHubActions.Step CreateDotNetCommandStep(AzurePipelines.Step step)
        {

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                run = "dotnet "
            };
            if (step.inputs.ContainsKey("command") == true)
            {
                gitHubStep.run += GetStepInput(step, "command") + " ";
            }
            if (step.inputs.ContainsKey("projects") == true)
            {
                gitHubStep.run += GetStepInput(step, "projects") + " ";
            }
            if (step.inputs.ContainsKey("arguments") == true)
            {
                gitHubStep.run += GetStepInput(step, "arguments") + " ";
            }

            //Remove the new line characters
            gitHubStep.run = gitHubStep.run.Replace("\n", "");

            return gitHubStep;
        }

        private GitHubActions.Step CreateDownloadBuildArtifacts(AzurePipelines.Step step)
        {
            string artifactName = GetStepInput(step, "artifactname");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                uses = "actions/download-artifact@v1.0.0",
                with = new Dictionary<string, string>
                {
                    { "name", artifactName }
                }
            };

            //From: 
            //- task: DownloadBuildArtifacts@0
            //  displayName: 'Download the build artifacts'
            //  inputs:
            //    buildType: 'current'
            //    downloadType: 'single'
            //    artifactName: 'drop'
            //    downloadPath: '$(build.artifactstagingdirectory)'

            //To:
            //- name: Download serviceapp artifact
            //  uses: actions/download-artifact@v1.0.0
            //  with:
            //    name: serviceapp
            return gitHubStep;
        }

        private GitHubActions.Step CreateCopyFilesStep(AzurePipelines.Step step)
        {
            //Use PowerShell to copy files
            step.script = "Copy " + GetStepInput(step, "sourcefolder") + "/" + GetStepInput(step, "contents") + " " + GetStepInput(step, "targetfolder");

            GitHubActions.Step gitHubStep = CreateScriptStep("powershell", step);
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
                    if (step.inputs != null)
                    {
                        string runValue = GetStepInput(step, "script");
                        gitHubStep.run = runValue;
                    }
                }
            }

            return gitHubStep;
        }

        private GitHubActions.Step CreateAzureLoginStep()
        {
            //Goal:
            //- name: Log into Azure
            //  uses: azure/login@v1
            //  with:
            //    creds: ${{ secrets.AZURE_SP }}
            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = "Azure Login",
                uses = "azure/login@v1",
                with = new Dictionary<string, string>
                {
                    {"creds","${{ secrets.AZURE_SP }}" }
                }
            };

            //Add note that "AZURE_SP" secret is required
            gitHubStep.step_message = @"Note that ""AZURE_SP"" secret is required to be setup and added into GitHub Secrets: https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets";

            return gitHubStep;
        }

        private GitHubActions.Step CreateUseDotNetStep(AzurePipelines.Step step)
        {
            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                uses = "actions/setup-dotnet@v1",
                with = new Dictionary<string, string>
                {
                    {"dotnet-version", GetStepInput(step, "version")}
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
            return gitHubStep;
        }

        private GitHubActions.Step CreateAzureManageResourcesStep(AzurePipelines.Step step)
        {
            string resourceGroup = GetStepInput(step, "resourcegroupname");
            string armTemplateFile = GetStepInput(step, "csmfile");
            string armTemplateParametersFile = GetStepInput(step, "csmparametersfile");

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

            //coming from:
            //- task: AzureResourceGroupDeployment@2
            //  displayName: 'Deploy ARM Template to resource group'
            //  inputs:
            //    azureSubscription: 'connection to Azure Portal'
            //    resourceGroupName: $(ResourceGroupName)
            //    location: '[resourceGroup().location]'
            //    csmFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.json'
            //    csmParametersFile: '$(build.artifactstagingdirectory)/drop/ARMTemplates/azuredeploy.parameters.json'
            //    overrideParameters: '-environment $(AppSettings.Environment) -locationShort $(ArmTemplateResourceGroupLocation)'

            //Going to:
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

            return gitHubStep;
        }

        private GitHubActions.Step CreateAzureWebAppDeploymentStep(AzurePipelines.Step step)
        {
            string webappName = GetStepInput(step, "webappname");
            string package = GetStepInput(step, "package");
            string slotName = GetStepInput(step, "slotname");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                uses = "Azure/webapps-deploy@v1",
                with = new Dictionary<string, string>
                {
                    { "app-name", webappName},
                    { "package", package},
                    { "slot-name", slotName},
                }
            };

            //coming from:
            //- task: AzureRmWebAppDeployment@3
            //  displayName: 'Azure App Service Deploy: web service'
            //  inputs:
            //    azureSubscription: 'connection to Azure Portal'
            //    WebAppName: $(WebServiceName)
            //    DeployToSlotFlag: true
            //    ResourceGroupName: $(ResourceGroupName)
            //    SlotName: 'staging'
            //    Package: '$(build.artifactstagingdirectory)/drop/MyProject.Service.zip'
            //    TakeAppOfflineFlag: true
            //    JSONFiles: '**/appsettings.json'

            //Going to:
            //- name: Deploy web service to Azure WebApp
            //  uses: Azure/webapps-deploy@v1
            //  with:
            //    app-name: myproject-service
            //    package: serviceapp
            //    slot-name: staging   

            return gitHubStep;
        }

        private GitHubActions.Step CreateSeleniumTestingStep(AzurePipelines.Step step)
        {
            //From:
            //- task: VSTest@2
            //displayName: 'Run functional smoke tests on website and web service'
            //inputs:
            //  searchFolder: '$(build.artifactstagingdirectory)'
            //  testAssemblyVer2: |
            //    **\MyProject.FunctionalTests\MyProject.FunctionalTests.dll
            //  uiTests: true
            //  runSettingsFile: '$(build.artifactstagingdirectory)/drop/FunctionalTests/MyProject.FunctionalTests/test.runsettings'
            //  overrideTestrunParameters: |
            //   -ServiceUrl "https://$(WebServiceName)-staging.azurewebsites.net/" 
            //   -WebsiteUrl "https://$(WebsiteName)-staging.azurewebsites.net/" 
            //   -TestEnvironment "$(AppSettings.Environment)" 

            //Defined in the github windows runner
            string vsTestConsoleLocation = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\TestPlatform\";

            string run = " |\n";
            run += "    $vsTestConsoleExe = \"" + vsTestConsoleLocation + "vstest.console.exe\"\n";
            run += "    $targetTestDll = \"" + GetStepInput(step, "testassemblyver2") + "\"\n";
            run += "    $testRunSettings = \" / Settings:`\"" + GetStepInput(step, "runsettingsfile") + "`\" \"\n";

            string parametersInput = GetStepInput(step, "overridetestrunparameters");
            //Split it two ways, there are 3 combinations, parameters are on each new line, parameters are all on one line, a combination of both multi and single lines.
            //1. Multiline
            string[] multiLineParameters = parametersInput.Split("\n-");
            StringBuilder parameters = new StringBuilder();
            foreach (string multiLineItem in multiLineParameters)
            {
                //2. Single line 
                string[] singleLineParameters = multiLineItem.Split(" -");
                foreach (string item in singleLineParameters)
                {
                    string[] items = item.Split(" ");
                    if (items.Length >= 2)
                    {
                        //Sometimes the first item has an extra -, remove this.
                        if (items[0].ToString().StartsWith("-") == true)
                        {
                            items[0] = items[0].TrimStart('-');
                        }
                        //build the new format [var name]=[var value]
                        parameters.Append(items[0]);
                        parameters.Append("=");
                        parameters.Append(items[1]);
                        parameters.Append("  ");
                    }
                }
            }
            run += "    $parameters = \"-- " + parameters.ToString() + "\"";

            //run += "    $parameters = \"\""; //\"-- TestEnvironment = \"Beta123\"  ServiceUrl = \"https://myproject-service-staging.azurewebsites.net/\" WebsiteUrl=\"https://myproject-web-staging.azurewebsites.net/\" \"" + "\n";
            run += "    #Note that the `\" is an escape character sequence to quote strings, and `& is needed to start the command\n";
            run += "    $command = \"`& `\"$vsTestConsoleExe`\" `\"$targetTestDll`\" $testRunSettings $parameters \"\n";
            run += "    Write - Host \"$command\"\n";
            run += "    Invoke - Expression $command";

            //To PowerShell script
            GitHubActions.Step gitHubStep = CreateScriptStep("powershell", step);
            gitHubStep.run = run;
            //gitHubStep.run = @" |
            //    $vsTestConsoleExe = ""C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\Extensions\\TestPlatform\\vstest.console.exe""
            //    $targetTestDll = """ + GetStepInput(step, "testassemblyver2") + @"""
            //    $testRunSettings = "" / Settings:`""" + GetStepInput(step, "runsettingsfile") + @"`"" ""
            //    $parameters = ""-- TestEnvironment = ""Beta123""  ServiceUrl = ""https://myproject-service-staging.azurewebsites.net/"" WebsiteUrl=""https://myproject-web-staging.azurewebsites.net/"" ""
            //    #Note that the `"" is an escape character to quote strings, and the `& is needed to start the command
            //    $command = ""`& `""$vsTestConsoleExe`"" `""$targetTestDll`"" $testRunSettings $parameters ""
            //    Write - Host ""$command""
            //    Invoke - Expression $command
            //    ";

            return gitHubStep;
        }
        private GitHubActions.Step CreateAzureAppServiceManageStep(AzurePipelines.Step step)
        {
            //https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/deploy/azure-app-service-manage?view=azure-devops
            //coming from:
            //- task: AzureAppServiceManage@0
            //displayName: 'Swap Slots: web service'
            //inputs:
            //  azureSubscription: 'connection to Azure Portal'
            //  WebAppName: $(WebServiceName)
            //  ResourceGroupName: $(ResourceGroupName)
            //  SourceSlot: 'staging'

            //Going to:
            //- name: Swap web service staging slot to production
            //  uses: Azure/cli@v1.0.0
            //  with:
            //    inlineScript: az webapp deployment slot swap --resource-group MyProjectRG --name featureflags-data-eu-service --slot staging --target-slot production


            string resourceGroup = GetStepInput(step, "resourcegroupname");
            string webAppName = GetStepInput(step, "webappname");
            string sourceSlot = GetStepInput(step, "sourceslot");
            string targetSlot = GetStepInput(step, "targetslot");
            if (string.IsNullOrEmpty(targetSlot) == true)
            {
                targetSlot = "production";
            }
            //TODO: Add other properties for az webapp deployment

            string script = "az webapp deployment slot swap --resource-group " + resourceGroup +
                " --name " + webAppName +
                " --slot " + sourceSlot +
                " --target-slot " + targetSlot + "";

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                uses = "Azure/cli@v1.0.0",
                with = new Dictionary<string, string>
                {
                    { "inlineScript", script}
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreatePublishBuildArtifactsStep(AzurePipelines.Step step)
        {
            //# Publish the artifacts
            //- task: PublishBuildArtifacts@1
            //  displayName: 'Publish Artifact'
            //  inputs:
            //    artifactName: drop
            //    PathtoPublish: '$(build.artifactstagingdirectory)'";

            //# Publishing pipeline artifacts is almost identical
            //- task: PublishPipelineArtifact@0
            //  displayName: Store artifact
            //  inputs:
            //    artifactName: 'MyProject'
            //    targetPath: 'MyProject/bin/release/netcoreapp2.2/publish/'

            //- name: publish build artifacts back to GitHub
            //  uses: actions/upload-artifact@master
            //  with:
            //    name: console exe
            //    path: /home/runner/work/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter.ConsoleApp/bin/Release/netcoreapp3.0

            string name = "";
            if (step.inputs.ContainsKey("artifactName") == true)
            {
                name = GetStepInput(step, "artifactname");
            }
            string path = "";
            if (step.task == "PublishBuildArtifacts@1")
            {
                path = GetStepInput(step, "pathtopublish");
            }
            else if (step.task == "PublishPipelineArtifact@0")
            {
                path = GetStepInput(step, "targetpath");
            }

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.displayName,
                uses = "actions/upload-artifact@master",
                with = new Dictionary<string, string>
                {
                    {"path", path}
                }
            };
            if (string.IsNullOrEmpty(name) == false)
            {
                gitHubStep.with.Add("name", name);
            }

            //In publish task, I we need to delete any usage of build.artifactstagingdirectory variable as it's implied in github actions, and therefore not needed (Adding it adds the path twice)
            if (gitHubStep.with.ContainsKey("path") == true)
            {
                gitHubStep.with["path"].Replace("$(build.artifactstagingdirectory)", "");
            }
            return gitHubStep;
        }

        //Safely extract the step input, if it exists
        private string GetStepInput(AzurePipelines.Step step, string name)
        {
            string input = "";
            if (step.inputs.ContainsKey(name) == true)
            {
                input = step.inputs[name];
            }
            return input;
        }
    }
}
