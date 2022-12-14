using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using GitHubActionsDotNet.Common;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using GitHubActions = GitHubActionsDotNet.Models;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public class StepsProcessing
    {
        //TODO: Add more task types
        public GitHubActions.Step ProcessStep(AzurePipelines.Step step)
        {
            GitHubActions.Step gitHubStep = null;
            if (step.task != null)
            {
                step = CleanStepInputs(step);
                //TODO: Should we be handling versions seperately? Currently the version is bundled with the step name
                switch (step.task.ToUpper()) //Set to upper case to handle case sensitivity comparisons e.g. NPM hangles Npm, NPM, or npm. 
                {
                    case "ANT@1":
                        gitHubStep = CreateAntStep(step);
                        break;
                    case "ARCHIVEFILES@1":
                    case "ARCHIVEFILES@2":
                        gitHubStep = CreateArchiveFilesStep(step);
                        break;
                    case "AZUREAPPSERVICEMANAGE@0":
                        gitHubStep = CreateAzureAppServiceManageStep(step);
                        break;
                    case "AZURECLI@1":
                    case "AZURECLI@2":
                        gitHubStep = CreateAzureCLIStep(step);
                        break;
                    case "AZUREKEYVAULT@1":
                    case "AZUREKEYVAULT@2":
                        gitHubStep = CreateAzureKeyStep(step);
                        break;
                    case "AZUREPOWERSHELL@2":
                    case "AZUREPOWERSHELL@3":
                    case "AZUREPOWERSHELL@4":
                    case "AZUREPOWERSHELL@5":
                        gitHubStep = CreateAzurePowershellStep(step);
                        break;
                    case "AZURERESOURCEGROUPDEPLOYMENT@2":
                    case "AZURERESOURCEMANAGERTEMPLATEDEPLOYMENT@3":
                        gitHubStep = CreateAzureManageResourcesStep(step);
                        break;
                    case "AZUREFUNCTIONAPP@1":
                    case "AZUREFUNCTIONAPPCONTAINER@1":
                    case "AZURERMWEBAPPDEPLOYMENT@3":
                    case "AZURERMWEBAPPDEPLOYMENT@4":
                    case "AZUREWEBAPPCONTAINER@1":
                    case "AZUREWEBAPP@1":
                        gitHubStep = CreateAzureWebAppDeploymentStep(step);
                        break;
                    case "BASH@3":
                        gitHubStep = CreateBashStep(step);
                        break;
                    case "BATCHSCRIPT@1":
                        gitHubStep = CreateBatchScriptStep(step);
                        break;
                    case "CMDLINE@1":
                    case "CMDLINE@2":
                        gitHubStep = CreateScriptStep("cmd", step);
                        break;
                    case "CACHE@2":
                        gitHubStep = CreateCacheStep(step);
                        break;
                    case "CMAKE@1":
                        gitHubStep = CreateCMakeStep(step);
                        break;
                    case "COPYFILES@1":
                    case "COPYFILES@2":
                        gitHubStep = CreateCopyFilesStep(step);
                        break;
                    case "DOCKER@0":
                    case "DOCKER@1":
                    case "DOCKER@2":
                        gitHubStep = CreateDockerStep(step);
                        break;
                    case "DOTNETCORECLI@1":
                    case "DOTNETCORECLI@2":
                        gitHubStep = CreateDotNetCommandStep(step);
                        break;
                    case "DOWNLOADBUILDARTIFACTS@0":
                        gitHubStep = CreateDownloadBuildArtifactsStep(step);
                        break;
                    case "DOWNLOADPIPELINEARTIFACT@2":
                        gitHubStep = CreateDownloadPipelineArtifactsStep(step);
                        break;
                    case "EXTRACTFILES@1":
                        gitHubStep = CreateExtractFilesStep(step);
                        break;
                    case "GITHUBRELEASE@0":
                        gitHubStep = CreateGitHubReleaseStep(step);
                        break;
                    case "GITVERSION@5":
                    case "GITVERSION/EXECUTE@0":
                        gitHubStep = CreateGitVersionExecuteStep(step);
                        break;
                    case "GITVERSION/SETUP@0":
                        gitHubStep = CreateGitVersionSetupStep(step);
                        break;
                    case "GRADLE@2":
                    case "GRADLE@3":
                        gitHubStep = CreateGradleStep(step);
                        break;
                    case "HUGOTASK@1":
                        gitHubStep = CreateHugoStep(step);
                        break;
                    case "INLINEPOWERSHELL@1":
                        gitHubStep = CreateInnerPowershellStep(step);
                        break;
                    //case "KUBERNETES@1":
                    //    gitHubStep = CreateKubernetesStep(step);
                    //    break;
                    case "MAVEN@3":
                        gitHubStep = CreateMavenStep(step);
                        break;
                    case "MSBUILD@1":
                        gitHubStep = CreateMSBuildStep(step);
                        break;
                    case "NPM@0":
                    case "NPM@1":
                        gitHubStep = CreateNPMStep(step);
                        break;
                    case "NODETOOL@0":
                        gitHubStep = CreateNodeToolStep(step);
                        break;
                    case "NUGETCOMMAND@2":
                        gitHubStep = CreateNuGetCommandStep(step);
                        break;
                    case "NUGETTOOLINSTALLER@0":
                    case "NUGETTOOLINSTALLER@1":
                        gitHubStep = CreateNuGetToolInstallerStep();
                        break;
                    case "OCTOPUSDEPLOY.OCTOPUS-DEPLOY-BUILD-RELEASE-TASKS.OCTOPUS-PACK.OCTOPUSPACK@4":
                        gitHubStep = CreateOctopusPackStep(step);
                        break;
                    case "OCTOPUSDEPLOY.OCTOPUS-DEPLOY-BUILD-RELEASE-TASKS.OCTOPUS-PUSH.OCTOPUSPUSH@4":
                        gitHubStep = CreateOctopusPushStep(step);
                        break;
                    case "POWERSHELL@1":
                    case "POWERSHELL@2":
                        gitHubStep = CreateScriptStep("powershell", step);
                        break;
                    case "PUBLISHPIPELINEARTIFACT@0":
                    case "PUBLISHPIPELINEARTIFACT@1":
                    case "PUBLISHBUILDARTIFACTS@1":
                        gitHubStep = CreatePublishBuildArtifactsStep(step);
                        break;
                    case "PYTHONSCRIPT@0":
                        gitHubStep = CreatePythonStep(step);
                        break;
                    //case "PUBLISHTESTRESULTS@2":
                    //    gitHubStep = CreatePublishTestResultsStep(step);
                    //    break;
                    case "SHELLSCRIPT@2":
                        gitHubStep = CreateBashShellScriptStep(step);
                        break;
                    case "SQLAZUREDACPACDEPLOYMENT@1":
                        gitHubStep = CreateSQLAzureDacPacDeployStep(step);
                        break;
                    case "TERRAFORMINSTALLER@0":
                    case "MS-DEVLABS.CUSTOM-TERRAFORM-TASKS.CUSTOM-TERRAFORM-INSTALLER-TASK.TERRAFORMINSTALLER@0":
                        gitHubStep = CreateTerraformInstallerStep(step);
                        break;
                    case "TERRAFORM@0":
                    case "TERRAFORMTASKV1@0":
                    case "TERRAFORMCLI@0":
                    case "MS-DEVLABS.CUSTOM-TERRAFORM-TASKS.CUSTOM-TERRAFORM-RELEASE-TASK.TERRAFORMTASKV1@0":
                        gitHubStep = CreateTerraformActionStep(step);
                        break;
                    case "USEDOTNET@2":
                        gitHubStep = CreateUseDotNetStep(step);
                        break;
                    case "USENODE@1":
                        gitHubStep = CreateUseNodeStep(step);
                        break;
                    case "USEPYTHONVERSION@0":
                        gitHubStep = CreateUsePythonStep(step);
                        break;
                    case "USERUBYVERSION@0":
                        gitHubStep = CreateUseRubyStep(step);
                        break;
                    case "VSBUILD@1":
                        gitHubStep = CreateMSBuildStep(step);
                        break;
                    case "VSTEST@1":
                    case "VSTEST@2":
                        gitHubStep = CreateFunctionalTestingStep(step);
                        break;
                    case "XAMARINANDROID@1":
                        gitHubStep = CreateXamarinAndroidStep(step);
                        break;
                    case "XAMARINIOS@2":
                        gitHubStep = CreateXamariniOSStep(step);
                        break;
                    default:
                        gitHubStep = CreateScriptStep("", step);
                        string newYaml = YamlSerialization.SerializeYaml<AzurePipelines.Step>(step);
                        string[] newYamlSplit = newYaml.Split(System.Environment.NewLine);
                        StringBuilder yamlBuilder = new StringBuilder();
                        for (int i = 0; i < newYamlSplit.Length; i++)
                        {
                            string line = newYamlSplit[i];
                            if (line.Trim().Length > 0)
                            {
                                yamlBuilder.Append("#");
                                yamlBuilder.Append(line);
                                yamlBuilder.Append(System.Environment.NewLine);
                            }
                        }
                        ////perhaps check for tasks we know are on the radar, but need help, helping to direct users to the repo and encourage contributions
                        //switch (step.task.ToUpper())
                        //{
                        //    case "GULP@1":
                        //        gitHubStep.step_message = "Note: The GULP@1 step does not have a conversion path yet, but it's on our radar. Please consider contributing! https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/219";
                        //        break;
                        //    default:
                        gitHubStep.step_message = $"Error: the step '{step.task}' does not have a conversion path yet";
                        //        break;
                        //}
                        gitHubStep.run = "echo \"" + gitHubStep.step_message + "\"" + System.Environment.NewLine + yamlBuilder.ToString();

                        break;
                }
            }
            else if (step.script != null)
            {
                gitHubStep = new GitHubActions.Step
                {
                    run = step.script,
                    with = step.inputs
                };
            }
            else if (step.pwsh != null)
            {
                gitHubStep = CreateScriptStep("pwsh", step);
            }
            else if (step.powershell != null)
            {
                gitHubStep = CreateScriptStep("powershell", step);
            }
            else if (step.bash != null)
            {
                gitHubStep = CreateScriptStep("bash", step);
            }
            else if (step.publish != null)
            {
                //The shortcut to the build publish step
                //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema#publish
                gitHubStep = CreatePublishBuildArtifactsStep(step);
            }
            else if (step.template != null)
            {
                gitHubStep = CreateTemplateStep(step);
            }
            else if (step.download != null)
            {
                gitHubStep = CreateDownloadStep(step);
            }
            else if (step.checkout != null)
            {
                gitHubStep = CreateCheckoutStep(step);
            }

            if (gitHubStep != null)
            {
                //Add in generic name and conditions
                if (step.displayName != null)
                {
                    gitHubStep.name = step.displayName;
                }
                if (step.condition != null)
                {
                    gitHubStep._if = ConditionsProcessing.TranslateConditions(step.condition);
                }
                //Double check the with. Sometimes we start to add a property, but for various reasons, we don't use it, and have to null out the with so it doesn't display an empty node in the final yaml
                if (gitHubStep.with != null)
                {
                    //Look to see if there is non-null data in the collection
                    bool foundData = false;
                    foreach (KeyValuePair<string, string> item in gitHubStep.with)
                    {
                        //If data was found, break out of the loop, we don't need to look anymore
                        if (item.Value != null)
                        {
                            foundData = true;
                            break;
                        }
                    }
                    //If no data was found, null out the with property
                    if (!foundData)
                    {
                        gitHubStep.with = null;
                    }
                }
                gitHubStep.continue_on_error = step.continueOnError;
                if (step.timeoutInMinutes != 0)
                {
                    gitHubStep.timeout_minutes = step.timeoutInMinutes;
                }
                if (step.env != null)
                {
                    gitHubStep.env = step.env;
                }
            }
            return gitHubStep;
        }

        //Convert all of the input keys to lowercase, to make pattern matching easier later
        private AzurePipelines.Step CleanStepInputs(AzurePipelines.Step step)
        {
            Dictionary<string, string> newInputs = new Dictionary<string, string>();
            if (step.inputs != null)
            {
                foreach (KeyValuePair<string, string> item in step.inputs)
                {
                    newInputs.Add(item.Key.ToLower(), item.Value);
                }
                step.inputs = newInputs;
            }

            return step;
        }

        private GitHubActions.Step CreateDotNetCommandStep(AzurePipelines.Step step)
        {
            string command = "";
            string args = "";
            List<string> projects = new List<string>();
            if (step.inputs != null)
            {
                string runScript = "dotnet ";
                if (step.inputs.ContainsKey("command"))
                {
                    command = GetStepInput(step, "command");
                    if (command == "push")
                    {
                        command = "nuget " + command;
                    }
                    command += " ";
                }
                runScript += command;
                if (step.inputs.ContainsKey("projects"))
                {
                    //There can be multiple projects - if there are, we need to replicate the command on each line for each project
                    string projectsString = GetStepInput(step, "projects");
                    if (projectsString.Contains("\n"))
                    {
                        string[] projectList = projectsString.Split("\n");
                        foreach (string item in projectList)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                projects.Add(item + " ");
                            }
                        }
                    }
                    else
                    {
                        projects.Add(projectsString + " ");
                    }
                }
                if (step.inputs.ContainsKey("packagestopack"))
                {
                    args += GetStepInput(step, "packagesToPack") + " ";
                }
                if (step.inputs.ContainsKey("packagestopush"))
                {
                    args += GetStepInput(step, "packagestopush") + " ";
                }
                if (step.inputs.ContainsKey("publishfeedcredentials"))
                {
                    string publishFeedCredentials = GetStepInput(step, "publishFeedCredentials");
                    if (publishFeedCredentials == "GitHub Packages")
                    {
                        args += "--source \"github\" ";
                    }
                }
                if (step.inputs.ContainsKey("arguments"))
                {
                    args += GetStepInput(step, "arguments") + " ";
                }
                //Build the final build script
                StringBuilder finalRunScript = new();
                if (projects.Count == 1)
                {
                    finalRunScript.Append(runScript);
                    finalRunScript.Append(projects[0]);
                    finalRunScript.Append(args);
                }
                else if (projects.Count > 1)
                {
                    foreach (string project in projects)
                    {
                        //Add each project with it's own command, project, args and new line
                        finalRunScript.Append(runScript);
                        finalRunScript.Append(project);
                        finalRunScript.Append(args);
                        finalRunScript.Append(System.Environment.NewLine);
                    }
                }
                else //there are no projects
                {
                    finalRunScript.Append(runScript);
                    finalRunScript.Append(args);
                }
                GitHubActions.Step gitHubStep = new GitHubActions.Step
                {
                    run = finalRunScript.ToString()
                };

                return gitHubStep;
            }
            else
            {
                GitHubActions.Step gitHubStep = new GitHubActions.Step
                {
                    step_message = "This DotNetCoreCLI task is misconfigured, inputs are required"
                };
                return gitHubStep;
            }
        }

        private GitHubActions.Step CreateDownloadStep(AzurePipelines.Step step)
        {
            //From: 
            //- download: [ current | pipeline resource identifier | none ] # disable automatic download if "none"
            //  artifact: string ## artifact name, optional; downloads all the available artifacts if not specified
            //  patterns: string # patterns representing files to include; optional
            //  displayName: string  # friendly name to display in the UI

            //To:
            //- name: Download serviceapp artifact
            //  uses: actions/download-artifact@v1.0.0
            //  with:
            //    name: serviceapp

            string artifact = step.artifact;
            string patterns = step.patterns;

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "actions/download-artifact@v2",
                with = new Dictionary<string, string>()
            };
            if (artifact != null)
            {
                gitHubStep.with.Add("name", artifact);
            }
            else if (patterns != null)
            {
                gitHubStep.with.Add("name", patterns);
            }

            return gitHubStep;
        }

        private GitHubActions.Step CreateDownloadBuildArtifactsStep(AzurePipelines.Step step)
        {
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

            string downloadType = GetStepInput(step, "downloadType");
            string artifactName = GetStepInput(step, "artifactname");
            string downloadPath = GetStepInput(step, "downloadPath");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "actions/download-artifact@v2",
                with = new Dictionary<string, string>()
            };
            if (downloadType != null && downloadType.ToLower() == "single")
            {
                gitHubStep.with.Add("name", artifactName);
            }
            if (downloadPath != null)
            {
                gitHubStep.with.Add("path", downloadPath);
            }

            return gitHubStep;
        }

        private GitHubActions.Step CreateDownloadPipelineArtifactsStep(AzurePipelines.Step step)
        {

            //From: 
            //- task: DownloadPipelineArtifact@2
            //  inputs:
            //    #source: 'current' # Options: current, specific
            //    #project: # Required when source == Specific
            //    #pipeline: # Required when source == Specific
            //    #preferTriggeringPipeline: false # Optional
            //    #runVersion: 'latest' # Required when source == Specific# Options: latest, latestFromBranch, specific
            //    #runBranch: 'refs/heads/main' # Required when source == Specific && RunVersion == LatestFromBranch
            //    #runId: # Required when source == Specific && RunVersion == Specific
            //    #tags: # Optional
            //    #artifact: # Optional
            //    #patterns: '**' # Optional
            //    #path: '$(Pipeline.Workspace)' 

            //To:
            //- name: Download serviceapp artifact
            //  uses: actions/download-artifact@v1.0.0
            //  with:
            //    name: serviceapp


            string artifactName = GetStepInput(step, "artifact");
            string path = GetStepInput(step, "path");

            //buildtype: current#  
            //artifactname: WebDeploy#  
            //targetpath: ${{ env.Pipeline.Workspace }}

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "actions/download-artifact@v2",
                with = new Dictionary<string, string>()
            };
            if (artifactName != null)
            {
                gitHubStep.with.Add("name", artifactName);
            }
            if (path != null)
            {
                gitHubStep.with.Add("path", path);
            }
            return gitHubStep;
        }

        private GitHubActions.Step CreateBashShellScriptStep(AzurePipelines.Step step)
        {
            //From:
            //- task: ShellScript@2
            //  inputs:
            //    scriptPath:
            //    #args: '' # Optional
            //    #disableAutoCwd: false # Optional
            //    #cwd: '' # Optional
            //    #failOnStandardError: false

            //To:
            //- name: test bash
            //  run: echo 'some text'
            //  shell: bash

            string scriptPath = GetStepInput(step, "scriptpath");
            string args = GetStepInput(step, "args");
            //string failOnStandardError = GetStepInput(step, "failOnStandardError");

            string inlineScript = scriptPath;
            if (args != null)
            {
                inlineScript += " " + args;
            }

            GitHubActions.Step gitHubStep = CreateScriptStep("bash", step);
            gitHubStep.run = inlineScript;

            return gitHubStep;
        }

        private GitHubActions.Step CreateBashStep(AzurePipelines.Step step)
        {
            //From:
            //- task: Bash@3
            //  inputs:
            //    #targetType: 'filePath' # Optional. Options: filePath, inline
            //    #filePath: # Required when targetType == FilePath
            //    #arguments: # Optional
            //    #script: '# echo Hello world' # Required when targetType == inline
            //    #workingDirectory: # Optional
            //    #failOnStderr: false # Optional
            //    #noProfile: true # Optional
            //    #noRc: true # Optional

            //To:
            //- name: test bash
            //  run: echo 'some text'
            //  shell: bash

            GitHubActions.Step gitHubStep = CreateScriptStep("bash", step);

            return gitHubStep;
        }

        private GitHubActions.Step CreateBatchScriptStep(AzurePipelines.Step step)
        {
            //From: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/utility/batch-script?view=azure-devops
            //- task: BatchScript@1
            //  inputs:
            //    filename: 
            //    #arguments: # Optional
            //    #modifyEnvironment: False # Optional
            //    #workingFolder: # Optional
            //    #failOnStandardError: false # Optional

            //To:
            //- name: test cmd
            //  run: dir /w
            //  shell: cmd

            string filename = GetStepInput(step, "filename");
            string arguments = GetStepInput(step, "arguments");

            string inlineScript = filename;
            if (arguments != null)
            {
                inlineScript += " " + arguments;
            }

            GitHubActions.Step gitHubStep = CreateScriptStep("cmd", step);
            gitHubStep.run = inlineScript;

            return gitHubStep;
        }

        private GitHubActions.Step CreateCacheStep(AzurePipelines.Step step)
        {
            //From: https://github.com/actions/cache
            //- task: Cache@2
            //  displayName: Cache multiple paths
            //  inputs:
            //    key: 'npm | "$(Agent.OS)" | package-lock.json'
            //    restoreKeys: |
            //      npm | "$(Agent.OS)"
            //      npm
            //    path: $(NPM_CACHE_FOLDER)

            //https://github.com/actions/cache
            //To:
            //- name: Cache multiple paths
            //  uses: actions/cache@v3
            //  with:
            //    key: 'npm | "${{ runner.os }}" | package-lock.json'
            //    restore-keys: |
            //      npm | "${{ runner.os }}"
            //      npm
            //    path: ${{ env.NPM_CACHE_FOLDER }}

            string key = GetStepInput(step, "key");
            string restoreKeys = GetStepInput(step, "restoreKeys");
            string path = GetStepInput(step, "path");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "actions/cache@v3",
                with = new Dictionary<string, string>
                {
                    { "key", key },
                    { "restore-keys", restoreKeys },
                    { "path", path }
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateCMakeStep(AzurePipelines.Step step)
        {
            //From: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/build/cmake?view=azure-devops
            //- task: CMake@1
            //  inputs:
            //    #workingDirectory: 'build' # Optional
            //    #cmakeArgs: # Optional

            //https://github.com/marketplace/actions/build-cmake
            //To:
            //- name: Build & Test
            //     uses: ashutoshvarma/action-cmake-build@master
            //     with:
            //       build-dir: ${{ runner.workspace }}/build
            //       build-options: Misc Options to pass to CMake while building project using cmake --build
            //       # will set the CC & CXX for cmake
            //       cc: gcc
            //       cxx: g++
            //       build-type: Release
            //       # Extra options pass to cmake while configuring project
            //       configure-options: -DCMAKE_C_FLAGS=-w32 -DPNG_INCLUDE=OFF
            //       run-test: true
            //       ctest-options: -R mytest
            //       # install the build using cmake --install
            //       install-build: true
            //       # run build using '-j [parallel]' to use multiple threads to build
            //       parallel: 14

            string workingDirectory = GetStepInput(step, "workingDirectory");
            string cmakeArgs = GetStepInput(step, "cmakeArgs");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "ashutoshvarma/action-cmake-build@master",
                with = new Dictionary<string, string>()
            };
            if (workingDirectory != null)
            {
                gitHubStep.with.Add("build-dir", workingDirectory);
            }
            if (cmakeArgs != null)
            {
                gitHubStep.with.Add("build-options", cmakeArgs);
            }

            return gitHubStep;
        }

        private GitHubActions.Step CreateCopyFilesStep(AzurePipelines.Step step)
        {
            //Use PowerShell to copy files
            step.script = "Copy '" + GetStepInput(step, "sourcefolder") + "/" + GetStepInput(step, "contents") + "' '" + GetStepInput(step, "targetfolder") + "'";

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);
            return gitHubStep;
        }

        //Very very simple. Needs more branches and logic
        private GitHubActions.Step CreateDockerStep(AzurePipelines.Step step)
        {
            //From: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/build/docker?view=azure-devops
            //- task: Docker@2
            //  displayName: Build
            //  inputs:
            //    command: build
            //    repository: contosoRepository
            //    dockerfile: MyDockerFile
            //    containerRegistry: dockerRegistryServiceConnection
            //    tags: tag1
            //    arguments: --secret id=mysecret,src=mysecret.txt

            //- task: Docker@1
            //  displayName: Push
            //  inputs:
            //    azureSubscriptionEndpoint: '$(Azure.ServiceConnectionId)'
            //    azureContainerRegistry: '$(ACR.FullName)'
            //    imageName: '$(ACR.ImageName)'
            //    command: push


            //To: https://github.com/marketplace/actions/docker-build-push
            //- name: Build the Docker image
            //  run: docker build . --file MyDockerFile --tag my-image-name:$(date +%s)

            //Docker@0 inputs
            //docker build -f /home/vsts/work/1/s/ContainerPOC/ContainerPOC.Web/Dockerfile -t samsappdeveucontainerregistry.azurecr.io/containerpoc:9015 /home/vsts/work/1/s/ContainerPOC

            //Docker@1 inputs
            //string azureSubscriptionEndpoint = GetStepInput(step, "azureSubscriptionEndpoint");
            //string azureContainerRegistry = GetStepInput(step, "azureContainerRegistry");

            //Docker@2 inputs
            string command = GetStepInput(step, "command");
            if (string.IsNullOrEmpty(command))
            {
                command = GetStepInput(step, "action");
            }
            string containerRegistry = GetStepInput(step, "containerRegistry");
            string azureContainerRegistry = GetStepInput(step, "azureContainerRegistry");
            if (string.IsNullOrEmpty(containerRegistry) && !string.IsNullOrEmpty(azureContainerRegistry))
            {
                JsonElement containerJson = JsonSerialization.DeserializeStringToJsonElement(azureContainerRegistry);
                if (containerJson.ValueKind == JsonValueKind.String)
                {
                    containerRegistry = containerJson.ToString();
                }
                else
                {
                    JsonElement jsonElement;
                    if (containerJson.TryGetProperty("loginServer", out jsonElement))
                    {
                        containerRegistry = jsonElement.ToString();
                    }
                }
            }
            string repository = GetStepInput(step, "repository");
            string tags = GetStepInput(step, "tags");
            string dockerFile = GetStepInput(step, "dockerfile");
            string context = GetStepInput(step, "context");
            string buildContext = GetStepInput(step, "buildContext");
            if (!string.IsNullOrEmpty(buildContext))
            {
                context = buildContext;
            }
            string arguments = GetStepInput(step, "arguments");
            string imageName = GetStepInput(step, "imageName");

            //We use a list as docker can have multiple lines we need to build
            List<string> dockerLines = new List<string>();
            string stepMessage = "";
            // build command is assumed - if this is blank, set command to "build"
            if (string.IsNullOrEmpty(command))
            {
                command = "build";
            }
            else if (command == "Push an image")
            {
                command = "push";
            }
            switch (command)
            {
                case "build":
                    dockerLines.Add("docker build");
                    break;
                case "push":
                    dockerLines.Add("docker push");
                    break;
                case "buildAndPush":
                    dockerLines.Add("docker build");
                    dockerLines.Add("docker push");
                    break;
                case "login":
                    dockerLines.Add("docker login");
                    break;
                case "logout":
                    dockerLines.Add("docker logout");
                    break;
            }

            //docker file is assumed to be DockerFile - if this is blank, set dockerfile to "DockerFile"
            if (string.IsNullOrEmpty(dockerFile))
            {
                switch (command)
                {
                    case "build":
                    case "push":
                    case "buildAndPush":
                        dockerFile = "Dockerfile";
                        break;
                }
            }

            if (dockerFile != null)
            {
                dockerLines = AppendStringToListItems(dockerLines, " --file " + dockerFile);
            }
            if (containerRegistry != null)
            {
                switch (command)
                {
                    case "push":
                    case "buildAndPush":
                    case "login":
                    case "logout":
                        dockerLines = AppendStringToListItems(dockerLines, " " + containerRegistry.Replace("\n", " ").Trim());
                        break;
                }
            }
            if (repository != null)
            {
                dockerLines = AppendStringToListItems(dockerLines, " " + repository);
            }
            if (imageName != null)
            {
                dockerLines = AppendStringToListItems(dockerLines, " " + imageName);
            }

            if (tags != null)
            {
                string[] splitTags = tags.Split("\n");
                StringBuilder newTags = new();
                foreach (string item in splitTags)
                {
                    if (item.Trim().Length > 0)
                    {
                        newTags.Append(item.Trim());
                        newTags.Append(",");
                    }
                }
                dockerLines = AppendStringToListItems(dockerLines, " --tags " + newTags.ToString()); //tags.Replace("\n", ",").Trim();
                for (int i = 0; i < dockerLines.Count; i++)
                {
                    if (dockerLines[i][dockerLines[i].Length - 1] == ',')
                    {
                        dockerLines[i] = dockerLines[i].Substring(0, dockerLines[i].Length - 1);
                    }
                }
            }
            if (arguments != null)
            {
                dockerLines = AppendStringToListItems(dockerLines, " " + arguments);
            }
            if (context != null)
            {
                dockerLines = AppendStringToListItems(dockerLines, " " + context);
            }

            StringBuilder script = new();
            if (dockerLines.Count > 1)
            {
                script.Append(System.Environment.NewLine);
            }
            foreach (string item in dockerLines)
            {
                script.Append(item);
                script.Append(System.Environment.NewLine);
            }
            step.script = script.ToString();
            GitHubActions.Step gitHubStep = CreateScriptStep("", step);
            if (!string.IsNullOrEmpty(stepMessage))
            {
                gitHubStep.step_message = stepMessage;
            }
            return gitHubStep;
        }

        private List<string> AppendStringToListItems(List<string> lines, string suffix)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] += suffix;
            }
            return lines;
        }

        private GitHubActions.Step CreateDockerLoginStep(string loginServer)
        {
            //- uses: azure/docker-login@v1
            //  with:
            //    login-server: contoso.azurecr.io
            //    username: ${{ secrets.REGISTRY_USERNAME }}
            //    password: ${{ secrets.REGISTRY_PASSWORD }}
            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = "Docker Login",
                uses = "azure/docker-login@v1",
                with = new Dictionary<string, string>
                {
                    {"login-server", loginServer },
                    {"username", "${{ secrets.REGISTRY_USERNAME }}" },
                    {"password", "${{ secrets.REGISTRY_PASSWORD }}" }
                }
            };

            //Add note that 'REGISTRY_USERNAME' and 'REGISTRY_PASSWORD' secrets are required
            gitHubStep.step_message = @"Note: login-server needs to be manually set, and the 'REGISTRY_USERNAME' and 'REGISTRY_PASSWORD' secrets are required to be added into GitHub Secrets. See these docs for details: https://github.com/Azure/docker-login";

            return gitHubStep;
        }

        private GitHubActions.Step CreateScriptStep(string shellType, AzurePipelines.Step step)
        {
            string targetType = GetStepInput(step, "targetType");
            string filePath = GetStepInput(step, "filePath");
            string arguments = GetStepInput(step, "arguments");

            if (targetType == "FilePath")
            {
                step.script = filePath + " " + arguments;
            }

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                run = step.script,
                shell = shellType
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
            if (gitHubStep.shell == "")
            {
                gitHubStep.shell = null;
            }

            return gitHubStep;
        }

        public GitHubActions.Step CreateCheckoutStep(AzurePipelines.Step step = null)
        {
            //https://github.com/actions/checkout
            //- uses: actions/checkout@v2
            //  with:
            //    # Repository name with owner. For example, actions/checkout
            //    # Default: ${{ github.repository }}
            //    repository: ''

            //    # The branch, tag or SHA to checkout. When checking out the repository that triggered a workflow, this defaults to the reference or SHA for that event.
            //    # Otherwise, uses the default branch.
            //    ref: ''

            //    # Personal access token (PAT) used to fetch the repository. The PAT is configured with the local git config, which enables your scripts to run authenticated git commands. The post-job step removes the PAT.
            //    # We recommend using a service account with the least permissions necessary. Also when generating a new PAT, select the least scopes necessary.
            //    # Default: ${{ github.token }}
            //    token: ''

            //    # SSH key used to fetch the repository. The SSH key is configured with the local git config, which enables your scripts to run authenticated git commands. The post-job step removes the SSH key.
            //    ssh-key: ''

            //    # Known hosts in addition to the user and global host key database. The public SSH keys for a host may be obtained using the utility `ssh-keyscan`. For example, `ssh-keyscan github.com`. The public key for github.com is always implicitly added.
            //    ssh-known-hosts: ''

            //    # Whether to perform strict host key checking. When true, adds the options `StrictHostKeyChecking=yes` and `CheckHostIP=no` to the SSH command line. Use the input `ssh-known-hosts` to configure additional hosts.
            //    # Default: true
            //    ssh-strict: ''

            //    # Whether to configure the token or SSH key with the local git config
            //    # Default: true
            //    persist-credentials: ''

            //    # Relative path under $GITHUB_WORKSPACE to place the repository
            //    path: ''

            //    # Whether to execute `git clean -ffdx && git reset --hard HEAD` before fetching
            //    # Default: true
            //    clean: ''

            //    # Number of commits to fetch. 0 indicates all history for all branches and tags.
            //    # Default: 1
            //    fetch-depth: ''

            //    # Whether to download Git-LFS files
            //    # Default: false
            //    lfs: ''

            //    # Whether to checkout submodules: `true` to checkout submodules or `recursive` to
            //    # recursively checkout submodules.
            //    #
            //    # When the `ssh-key` input is not provided, SSH URLs beginning with
            //    # `git@github.com:` are converted to HTTPS.
            //    #
            //    # Default: false
            //    submodules: ''


            //Add the check out step to get the code
            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "actions/checkout@v2",
                with = new Dictionary<string, string>()
            };

            if (step != null)
            {
                string fetchDepth = step.fetchDepth;
                string persistCredentials = step.persistCredentials;
                string lfs = step.lfs;
                string clean = step.clean;

                //    # Repository name with owner. For example, actions/checkout
                //    # Default: ${{ github.repository }}
                //    repository: ''

                //    # The branch, tag or SHA to checkout. When checking out the repository that triggered a workflow, this defaults to the reference or SHA for that event.
                //    # Otherwise, uses the default branch.
                //    ref: ''
                //    token: ${{ github.token }}

                //    # SSH key used to fetch the repository. The SSH key is configured with the local git config, which enables your scripts to run authenticated git commands. The post-job step removes the SSH key.
                //    ssh-key: ''

                //    # Known hosts in addition to the user and global host key database. The public SSH keys for a host may be obtained using the utility `ssh-keyscan`. For example, `ssh-keyscan github.com`. The public key for github.com is always implicitly added.
                //    ssh-known-hosts: ''
                //    ssh-strict: true
                //    persist-credentials: true
                //    path: '/mycode'
                //    clean: true
                //    fetch-depth: 1
                //    lfs: false
                //    submodules: false


                if (step.checkout != null && step.checkout != "self")
                {
                    gitHubStep.with.Add("repository", step.checkout);
                }
                if (fetchDepth != null)
                {
                    gitHubStep.with.Add("fetch-depth", fetchDepth);
                }
                if (persistCredentials != null)
                {
                    gitHubStep.with.Add("persist-credentials", persistCredentials);
                }
                if (lfs != null)
                {
                    gitHubStep.with.Add("lfs", lfs);
                }
                if (clean != null)
                {
                    gitHubStep.with.Add("clean", clean);
                }
            }
            if (gitHubStep.with.Count == 0)
            {
                gitHubStep.with = null;
            }
            return gitHubStep;
        }

        public GitHubActions.Step CreateAzureLoginStep()
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

            //Add note that 'AZURE_SP' secret is required
            gitHubStep.step_message = @"Note: the 'AZURE_SP' secret is required to be added into GitHub Secrets. See this blog post for details: https://samlearnsazure.blog/2019/12/13/github-actions/";

            return gitHubStep;
        }

        private GitHubActions.Step CreateUseDotNetStep(AzurePipelines.Step step)
        {
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

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "actions/setup-dotnet@v1",
                with = new Dictionary<string, string>
                {
                    {"dotnet-version", GetStepInput(step, "version")}
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateUseNodeStep(AzurePipelines.Step step)
        {
            //Pipelines
            //- task: UseNode@1
            //  displayName: 'Use Node.js 8.10.0'
            //  inputs:
            //    version: '8.10.0'

            //Actions
            //- uses: actions/setup-node@v2
            //  with:
            //    node-version: '14'

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "actions/setup-node@v2",
                with = new Dictionary<string, string>
                {
                    {"node-version", GetStepInput(step, "version")}
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateAzureManageResourcesStep(AzurePipelines.Step step)
        {

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


            //or:
            //- task: AzureResourceManagerTemplateDeployment@3
            //  inputs:
            //    deploymentScope: 'Resource Group'
            //    azureResourceManagerConnection: 'copy-connection'
            //    subscriptionId: '00000000-0000-0000-0000-000000000000'
            //    action: 'Create Or Update Resource Group'
            //    resourceGroupName: 'demogroup'
            //    location: 'West US'
            //    templateLocation: 'URL of the file'
            //    csmFileLink: '$(AzureFileCopy.StorageContainerUri)templates/mainTemplate.json$(AzureFileCopy.StorageContainerSasToken)'
            //    csmParametersFileLink: '$(AzureFileCopy.StorageContainerUri)templates/mainTemplate.parameters.json$(AzureFileCopy.StorageContainerSasToken)'
            //    deploymentMode: 'Incremental'
            //    deploymentName: 'deploy1'

            // https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/deploy-cli
            //- name: Swap web service staging slot to production
            //  uses: Azure/cli@v1.0.0
            //  with:
            //    inlineScript: az deployment group create --resource-group <resource-group-name> --template-file <path-to-template>

            string resourceGroup = GetStepInput(step, "resourceGroupName");
            string armTemplateFile = GetStepInput(step, "csmFile");
            string armTemplateParametersFile = GetStepInput(step, "csmParametersFile");
            if (string.IsNullOrEmpty(armTemplateFile))
            {
                armTemplateFile = GetStepInput(step, "csmFileLink");
            }
            if (string.IsNullOrEmpty(armTemplateFile))
            {
                armTemplateParametersFile = GetStepInput(step, "csmParametersFileLink");
            }
            string overrideParameters = GetStepInput(step, "overrideParameters");

            string script = "az deployment group create --resource-group " + resourceGroup +
                " --template-file " + armTemplateFile;

            //Add parameters
            if (!string.IsNullOrEmpty(armTemplateParametersFile) || !string.IsNullOrEmpty(overrideParameters))
            {
                string parameters = " --parameters ";
                if (!string.IsNullOrEmpty(armTemplateParametersFile))
                {
                    parameters += " " + armTemplateParametersFile;
                }
                if (!string.IsNullOrEmpty(overrideParameters))
                {
                    parameters += " " + overrideParameters;
                }
                script += parameters;
            }

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "Azure/cli@v1.0.0",
                with = new Dictionary<string, string>
                {
                    { "inlineScript", script}
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateAzureWebAppDeploymentStep(AzurePipelines.Step step)
        {
            string webappName = GetStepInput(step, "webappname");
            string appName = GetStepInput(step, "appName");
            string package = GetStepInput(step, "package");
            string packageForLinux = GetStepInput(step, "packageForLinux");
            string slotName = GetStepInput(step, "slotname");
            string imageName = GetStepInput(step, "imageName");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "Azure/webapps-deploy@v2",
                with = new Dictionary<string, string>()
            };

            if (webappName != null)
            {
                gitHubStep.with.Add("app-name", webappName);
            }
            else if (appName != null)
            {
                gitHubStep.with.Add("app-name", appName);
            }
            if (package != null)
            {
                gitHubStep.with.Add("package", package);
            }
            else if (packageForLinux != null)
            {
                gitHubStep.with.Add("package", packageForLinux);
            }
            if (slotName != null)
            {
                gitHubStep.with.Add("slot-name", slotName);
            }
            if (imageName != null)
            {
                gitHubStep.with.Add("images", imageName);
            }

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

        private GitHubActions.Step CreateNuGetCommandStep(AzurePipelines.Step step)
        {
            string command = GetStepInput(step, "command");
            if (string.IsNullOrEmpty(command))
            {
                command = "restore";
            }
            string restoresolution = GetStepInput(step, "restoresolution");

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);
            gitHubStep.run = "nuget " + command + " " + restoresolution;

            //coming from:
            //# NuGet
            //# Restore, pack, or push NuGet packages, or run a NuGet command. Supports NuGet.org and authenticated feeds like Azure Artifacts and MyGet. Uses NuGet.exe and works with .NET Framework apps. For .NET Core and .NET Standard apps, use the .NET Core task.
            //- task: NuGetCommand@2
            //  inputs:
            //    #command: 'restore' # Options: restore, pack, push, custom
            //    #restoreSolution: '**/*.sln' # Required when command == Restore
            //    #feedsToUse: 'select' # Options: select, config
            //    #vstsFeed: # Required when feedsToUse == Select
            //    #includeNuGetOrg: true # Required when feedsToUse == Select
            //    #nugetConfigPath: # Required when feedsToUse == Config
            //    #externalFeedCredentials: # Optional
            //    #noCache: false 
            //    #disableParallelProcessing: false 
            //    restoreDirectory: 
            //    #verbosityRestore: 'Detailed' # Options: quiet, normal, detailed
            //    #packagesToPush: '$(Build.ArtifactStagingDirectory)/**/*.nupkg;!$(Build.ArtifactStagingDirectory)/**/*.symbols.nupkg' # Required when command == Push
            //    #nuGetFeedType: 'internal' # Required when command == Push# Options: internal, external
            //    #publishVstsFeed: # Required when command == Push && NuGetFeedType == Internal
            //    #publishPackageMetadata: true # Optional
            //    #allowPackageConflicts: # Optional
            //    #publishFeedCredentials: # Required when command == Push && NuGetFeedType == External
            //    #verbosityPush: 'Detailed' # Options: quiet, normal, detailed
            //    #packagesToPack: '**/*.csproj' # Required when command == Pack
            //    #configuration: '$(BuildConfiguration)' # Optional
            //    #packDestination: '$(Build.ArtifactStagingDirectory)' # Optional
            //    #versioningScheme: 'off' # Options: off, byPrereleaseNumber, byEnvVar, byBuildNumber
            //    #includeReferencedProjects: false # Optional
            //    #versionEnvVar: # Required when versioningScheme == ByEnvVar
            //    #majorVersion: '1' # Required when versioningScheme == ByPrereleaseNumber
            //    #minorVersion: '0' # Required when versioningScheme == ByPrereleaseNumber
            //    #patchVersion: '0' # Required when versioningScheme == ByPrereleaseNumber
            //    #packTimezone: 'utc' # Required when versioningScheme == ByPrereleaseNumber# Options: utc, local
            //    #includeSymbols: false # Optional
            //    #toolPackage: # Optional
            //    #buildProperties: # Optional
            //    #basePath: # Optional, specify path to nuspec files
            //    #verbosityPack: 'Detailed' # Options: quiet, normal, detailed
            //    #arguments: # Required when command == Custom

            //Going to:
            //- name: Nuget Push
            //  run: nuget push *.nupkg

            return gitHubStep;
        }

        //https://github.com/warrenbuckley/Setup-Nuget
        private GitHubActions.Step CreateNuGetToolInstallerStep()
        {
            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "nuget/setup-nuget@v1"
            };

            //coming from:
            //# NuGet tool installer
            //# Acquires a specific version of NuGet from the internet or the tools cache and adds it to the PATH. Use this task to change the version of NuGet used in the NuGet tasks.
            //- task: NuGetToolInstaller@0
            //  inputs:
            //    #versionSpec: '4.3.0' 
            //    #checkLatest: false # Optional

            //Going to:
            //- name: Setup Nuget.exe
            //  uses: nuget/setup-nuget@v1

            return gitHubStep;
        }

        private GitHubActions.Step CreateOctopusPackStep(AzurePipelines.Step step)
        {
            //Coming from: 
            //- task: octopusdeploy.octopus-deploy-build-release-tasks.octopus-pack.OctopusPack@4
            //  displayName: 'Package OctopusSamples.OctoPetShop.Database'
            //  inputs:
            //    PackageId: OctopusSamples.OctoPetShop.Database
            //    PackageFormat: Zip
            //    PackageVersion: '$(Build.BuildNumber)'
            //    SourcePath: '$(build.artifactstagingdirectory)\output\OctoPetShop.Database\'
            //    OutputPath: '$(Build.SourcesDirectory)\output'

            //Going to:
            //- name: Package OctoPetShopDatabase
            //  run: |
            //    octo pack --id="OctoPetShop.Database" --format="Zip" --version="$PACKAGE_VERSION" --basePath="$GITHUB_WORKSPACE/artifacts/OctopusSamples.OctoPetShop.Database" --outFolder="$GITHUB_WORKSPACE/artifacts"

            string packageId = GetStepInput(step, "packageid");
            string packageFormat = GetStepInput(step, "packageformat");
            string packageVersion = GetStepInput(step, "packageversion");
            string sourcePath = GetStepInput(step, "sourcepath");
            string outputPath = GetStepInput(step, "outputpath");
            if (!string.IsNullOrEmpty(packageId))
            {
                packageId = "--id=" + packageId + " ";
            }
            if (!string.IsNullOrEmpty(packageFormat))
            {
                packageFormat = "--format=" + packageFormat + " ";
            }
            if (!string.IsNullOrEmpty(packageVersion))
            {
                packageVersion = "--version=" + packageVersion + " ";
            }
            if (!string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = "--basePath=" + sourcePath + " ";
            }
            if (!string.IsNullOrEmpty(outputPath))
            {
                outputPath = "--outFolder=" + outputPath + " ";
            }

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);
            gitHubStep.run = "octo pack " + packageId + packageFormat + packageVersion + sourcePath + outputPath;

            return gitHubStep;
        }

        private GitHubActions.Step CreateOctopusPushStep(AzurePipelines.Step step)
        {
            //Coming from: 
            //- task: octopusdeploy.octopus-deploy-build-release-tasks.octopus-push.OctopusPush@4
            //  displayName: 'Push Packages to Octopus'
            //  inputs:
            //    OctoConnectedServiceName: OctopusWebinars
            //    Space: 'Spaces-222'
            //    Package: '$(Build.SourcesDirectory)/output/*.zip'

            //Going to:
            //- name: Push OctoPetShop Database
            //  run: |
            //    octo push --package="$GITHUB_WORKSPACE/artifacts/OctoPetShop.Database.$PACKAGE_VERSION.zip" --server="${{ secrets.OCTOPUSSERVERURL }}" --apiKey="${{ secrets.OCTOPUSSERVERAPIKEY }}" --space="${{ secrets.OCTOPUSSERVER_SPACE }}"

            string package = GetStepInput(step, "package");
            string space = GetStepInput(step, "space");
            if (!string.IsNullOrEmpty(package))
            {
                package = "--package=" + package + " ";
            }
            if (!string.IsNullOrEmpty(space))
            {
                space = "--space=" + space + " ";
            }
            string secrets = @"--server=""${{ secrets.OCTOPUSSERVERURL }}"" --apiKey=""${{ secrets.OCTOPUSSERVERAPIKEY }}"" ";

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);
            gitHubStep.run = "octo push " + package + space + secrets;
            gitHubStep.step_message = "Note: requires secrets OCTOPUSSERVERURL and OCTOPUSSERVERAPIKEY to be configured";

            return gitHubStep;
        }

        private GitHubActions.Step CreateOctopusInstallerStep()
        {
            // - name: Install Octopus CLI
            //   uses: OctopusDeploy/install-octocli@v1
            //   with:
            //     version: 7.4.2
            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "OctopusDeploy/install-octocli@v1",
                with = new Dictionary<string, string>
                {
                    { "version", "7.4.2"}
                },
            };

            return gitHubStep;
        }

        //https://github.com/Azure/sql-action
        private GitHubActions.Step CreateSQLAzureDacPacDeployStep(AzurePipelines.Step step)
        {
            string serverName = GetStepInput(step, "servername");
            string dacPacFile = GetStepInput(step, "dacpacfile");
            string arguments = GetStepInput(step, "additionalarguments");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "azure/sql-action@v1",
                with = new Dictionary<string, string>
                {
                    { "server-name", serverName},
                    { "connection-string", "${{ secrets.AZURE_SQL_CONNECTION_STRING }}"},
                    { "dacpac-package", dacPacFile},
                    { "arguments", arguments }
                },
                step_message = "Note: Connection string needs to be specified - this is different than Pipelines where the server, database, user, and password were specified separately. It's recommended you use secrets for the connection string."
            };

            //coming from:
            //- task: SqlAzureDacpacDeployment@1
            //  displayName: 'Azure SQL dacpac publish'
            //  inputs:
            //    azureSubscription: 'my connection to Azure Portal'
            //    ServerName: '$(databaseServerName).database.windows.net'
            //    DatabaseName: '$(databaseName)'
            //    SqlUsername: '$(databaseLoginName)'
            //    SqlPassword: '$(databaseLoginPassword)'
            //    DacpacFile: '$(build.artifactstagingdirectory)/drop/MyDatabase.dacpac'
            //    additionalArguments: '/p:BlockOnPossibleDataLoss=true'  

            //Going to:
            //- uses: azure/sql-action@v1
            //  with:
            //    server-name: REPLACE_THIS_WITH_YOUR_SQL_SERVER_NAME
            //    connection-string: ${{ secrets.AZURE_SQL_CONNECTION_STRING }}
            //    dacpac-package: './yourdacpacfile.dacpac'

            return gitHubStep;
        }

        private GitHubActions.Step CreateTerraformInstallerStep(AzurePipelines.Step step)
        {

            //coming from (two known variations)
            //- task: terraformInstaller@0
            //  displayName: Install Terraform
            //  inputs:
            //    terraformVersion: '0.12.12'

            //- task: ms-devlabs.custom-terraform-tasks.custom-terraform-installer-task.TerraformInstaller@0
            //  displayName: Install Terraform
            //  inputs:
            //    terraformVersion: 0.12.12

            //Going to:
            //- uses: hashicorp/setup-terraform@v1
            //  with:
            //    terraform_version: 0.12.12

            string terraformVersion = GetStepInput(step, "terraformversion");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "hashicorp/setup-terraform@v1",
                with = new Dictionary<string, string>
                {
                    { "terraform_version", terraformVersion}
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateTerraformActionStep(AzurePipelines.Step step)
        {
            //Note: requires the hashicorp/setup-terraform@v1 task to be run before

            //coming from a 3 known varations:
            //- task: TerraformTaskV1@0
            //  displayName: 'Terraform Init'
            //  inputs:
            //    provider: 'azurerm'
            //    command: 'init'
            //    backendServiceArm: '$(backendServiceArm)'
            //    backendAzureRmResourceGroupName: '$(backendAzureRmResourceGroupName)'
            //    backendAzureRmStorageAccountName: '$(backendAzureRmStorageAccountName)'
            //    backendAzureRmContainerName: '$(backendAzureRmContainerName)'
            //    backendAzureRmKey: '$(backendAzureRmKey)'

            //terraform@0
            //- task: terraform@0
            //  displayName: Terraform Init
            //  inputs:
            //    command: 'init'
            //    providerAzureConnectedServiceName: 'MTC Denver Sandbox'
            //    backendAzureProviderStorageAccountName: 'mtcdenterraformsandbox'

            //- task: ms-devlabs.custom-terraform-tasks.custom-terraform-release-task.TerraformTaskV1@0
            //  displayName: 'Terraform : init'
            //  inputs:
            //    command: init
            //    workingDirectory: tf/env/dev
            //    backendServiceArm: YAML Template Examples - Dev
            //    backendAzureRmResourceGroupName: rg-terraformState-dev-eus
            //    backendAzureRmStorageAccountName: #
            //    backendAzureRmContainerName: terraform-state
            //    backendAzureRmKey: terraformStorageExample.tfstate

            //TerraformCLI@0

            //going to:
            //- name: 'Terraform : azure init'
            //  uses: hashicorp/terraform-github-actions@master
            //  with:
            //    tf_actions_version: 0.12.13
            //    tf_actions_subcommand: "init"

            string command = GetStepInput(step, "command");
            if (command == null)
            {
                command = "";
            }
            string args = GetStepInput(step, "args");
            if (string.IsNullOrEmpty(args))
            {
                args = GetStepInput(step, "commandOptions");
            }
            string script = GetStepInput(step, "script");

            //string backendServiceArm = GetStepInput(step, "backendServiceArm");
            //string backendAzureRmResourceGroupName = GetStepInput(step, "backendAzureRmResourceGroupName");
            //string backendAzureRmStorageAccountName = GetStepInput(step, "backendAzureRmStorageAccountName");
            //string backendAzureRmContainerName = GetStepInput(step, "backendAzureRmContainerName");
            //string backendAzureRmKey = GetStepInput(step, "backendAzureRmKey");

            //initialize the step (copying over some 
            GitHubActions.Step gitHubStep = CreateScriptStep("", step);

            //build the run command
            if (command == "CLI")
            {
                gitHubStep.run = script;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("terraform ");
                sb.Append(command);
                if (args != null)
                {
                    sb.Append(" ");
                    sb.Append(args);
                    if (command.ToLower() == "apply")
                    {
                        //If we don't add an auto-approve, the apply command is stuck waiting for a confirmation
                        sb.Append(" -auto-approve");
                    }
                }
                gitHubStep.run = sb.ToString();
            }

            return gitHubStep;
        }

        //"Note that this task is in still in development by HashiCorp, and is not supported in production usage. More details: https://github.com/hashicorp/setup-terraform"

        private GitHubActions.Step CreateMSBuildStep(AzurePipelines.Step step)
        {
            //coming from:
            //# Visual Studio build
            //# Build with MSBuild and set the Visual Studio version property
            //- task: VSBuild@1
            //  inputs:
            //    solution: 'MySolution.sln' 
            //    vsVersion: 'latest' # Optional. Options: latest, 16.0, 15.0, 14.0, 12.0, 11.0
            //    msbuildArgs: # Optional
            //    platform: # Optional
            //    configuration: # Optional
            //    clean: false # Optional
            //    maximumCpuCount: false # Optional
            //    restoreNugetPackages: false # Optional
            //    msbuildArchitecture: 'x86' # Optional. Options: x86, x64
            //    logProjectEvents: true # Optional
            //    createLogFile: false # Optional
            //    logFileVerbosity: 'normal' # Optional. Options: quiet, minimal, normal, detailed, diagnostic

            //# Build with MSBuild
            //- task: MSBuild@1
            //  inputs:
            //    #solution: '**/*.sln' 
            //    #msbuildLocationMethod: 'version' # Optional. Options: version, location
            //    #msbuildVersion: 'latest' # Optional. Options: latest, 16.0, 15.0, 14.0, 12.0, 4.0
            //    #msbuildArchitecture: 'x86' # Optional. Options: x86, x64
            //    #msbuildLocation: # Optional
            //    #platform: # Optional
            //    #configuration: # Optional
            //    #msbuildArguments: # Optional
            //    #clean: false # Optional
            //    #maximumCpuCount: false # Optional
            //    #restoreNugetPackages: false # Optional
            //    #logProjectEvents: false # Optional
            //    #createLogFile: false # Optional
            //    #logFileVerbosity: 'normal' # Optional. Options: quiet, minimal, normal, detailed, diagnostic


            //Going to:
            //- run: msbuild MySolution.sln /p:configuration=release

            string solution = GetStepInput(step, "solution");
            string platform = GetStepInput(step, "platform");
            string configuration = GetStepInput(step, "configuration");
            string msbuildArgs = GetStepInput(step, "msbuildArgs");
            string msbuildArguments = GetStepInput(step, "msbuildArguments");
            //string msbuildArchitecture = GetStepInput(step, "msbuildArchitecture");
            string run = "msbuild '" + solution + "'";
            if (configuration != null)
            {
                run += " /p:configuration='" + configuration + "'";
            }
            if (platform != null)
            {
                run += " /p:platform='" + platform + "'";
            }
            if (msbuildArgs != null) //VSBuild@1
            {
                run += " " + msbuildArgs;
            }
            else if (msbuildArguments != null) //MSBUILD@1
            {
                run += " " + msbuildArguments;
            }
            step.script = run;

            //To script
            GitHubActions.Step gitHubStep = CreateScriptStep("", step);
            gitHubStep.run = run;

            return gitHubStep;
        }

        public GitHubActions.Step CreateMSBuildSetupStep()
        {
            //To:
            //- name: Setup MSBuild.exe
            //  uses: microsoft/setup-msbuild@v1.0.2

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "microsoft/setup-msbuild@v1.0.2"
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateFunctionalTestingStep(AzurePipelines.Step step)
        {
            //From:
            //- task: VSTest@2
            //  displayName: 'Run functional smoke tests on website and web service'
            //  inputs:
            //    searchFolder: '$(build.artifactstagingdirectory)'
            //    testAssemblyVer2: **\MyProject.FunctionalTests\MyProject.FunctionalTests.dll
            //    uiTests: true
            //    runSettingsFile: '$(build.artifactstagingdirectory)/drop/FunctionalTests/MyProject.FunctionalTests/test.runsettings'
            //    overrideTestrunParameters: |
            //     -ServiceUrl "https://$(WebServiceName)-staging.azurewebsites.net/" 
            //     -WebsiteUrl "https://$(WebsiteName)-staging.azurewebsites.net/" 
            //     -TestEnvironment "$(AppSettings.Environment)" 

            //To:
            //- name: Functional Tests
            //  run: |
            //    $vsTestConsoleExe = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\Common7\\IDE\\Extensions\\TestPlatform\\vstest.console.exe"
            //    $targetTestDll = "functionaltests\FeatureFlags.FunctionalTests.dll"
            //    $testRunSettings = "/Settings:`"functionaltests\test.runsettings`" "
            //    $parameters = " -- TestEnvironment=""Beta123"" ServiceUrl=""https://featureflags-data-eu-service-staging.azurewebsites.net/"" WebsiteUrl=""https://featureflags-data-eu-web-staging.azurewebsites.net/"" "
            //    #Note that the `" is an escape character to quote strings, and the `& is needed to start the command
            //    $command = "`& `"$vsTestConsoleExe`" `"$targetTestDll`" $testRunSettings $parameters " 
            //    Write-Host "$command"
            //    Invoke-Expression $command

            //Defined in the github windows runner.
            //TODO: fix this hardcoded VS path
            string vsTestConsoleLocation = @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\Extensions\TestPlatform\";

            string run = "";
            run += "$vsTestConsoleExe = \"" + vsTestConsoleLocation + "vstest.console.exe\"\n";
            run += "$targetTestDll = \"" + GetStepInput(step, "testassemblyver2") + "\"\n";
            run += "$testRunSettings = \"/Settings:`\"" + GetStepInput(step, "runsettingsfile") + "`\" \"\n";

            string parametersInput = GetStepInput(step, "overridetestrunparameters");
            if (parametersInput != null)
            {
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
                        string[] items = item.Replace("\n", "").Split(" ");
                        if (items.Length == 2)
                        {
                            //build the new format [var name]=[var value]
                            parameters.Append(items[0]);
                            parameters.Append("=");
                            parameters.Append(items[1]);
                            parameters.Append(" ");
                        }
                        else
                        {
                            for (int i = 0; i < items.Length - 1; i++)
                            {
                                //if it's an even number (and hence the var name):
                                if (i % 2 == 0)
                                {
                                    //Sometimes the first item has an extra -, remove this.
                                    if (items[i].ToString().StartsWith("-"))
                                    {
                                        items[i] = items[i].TrimStart('-');
                                    }
                                    //build the new format [var name]=[var value]
                                    parameters.Append(items[i]);
                                    parameters.Append("=");
                                }
                                else //It's an odd number (and hence the var value)
                                {
                                    //build the new format [var name]=[var value]
                                    parameters.Append(items[i]);
                                    parameters.Append(" ");
                                }
                            }
                        }
                    }
                }
                run += "$parameters = \" -- " + parameters.ToString() + "\"\n";
                //run += "$parameters = \"poop\"\n";
            }

            run += "#Note that the `\" is an escape character to quote strings, and the `& is needed to start the command\n";
            run += "$command = \"`& `\"$vsTestConsoleExe`\" `\"$targetTestDll`\" $testRunSettings $parameters \"\n";
            run += "Write-Host \"$command\"\n";
            run += "Invoke-Expression $command";

            //To PowerShell script
            step.script = run;
            GitHubActions.Step gitHubStep = CreateScriptStep("powershell", step);

            return gitHubStep;
        }

        public GitHubActions.Step CreateSetupJavaStep(string javaVersion)
        {
            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = "Setup JDK " + javaVersion,
                uses = "actions/setup-java@v1",
                with = new Dictionary<string, string>
                {
                    { "java-version", javaVersion}
                }
            };

            return gitHubStep;
        }

        public GitHubActions.Step CreateSetupGradleStep()
        {
            //Going to: 
            //- name: Grant execute permission for gradlew
            //  run: chmod +x gradlew

            AzurePipelines.Step step = new Step
            {
                name = "Grant execute permission for gradlew",
                script = "chmod +x gradlew"
            };
            GitHubActions.Step gitHubStep = CreateScriptStep("", step);

            return gitHubStep;
        }

        public GitHubActions.Step CreateExtractFilesStep(AzurePipelines.Step step)
        {
            //From: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/utility/extract-files            //# GitHub Release
            //# Extract files
            //# Extract a variety of archive and compression files such as .7z, .rar, .tar.gz, and .zip
            //- task: ExtractFiles@1
            //  inputs:
            //    #archiveFilePatterns: '**/*.zip' 
            //    destinationFolder: 
            //    #cleanDestinationFolder: true 
            //    #overwriteExistingFiles: false

            //To: 
            //- uses: montudor/action-zip@v0.1.1
            //  with:
            //    args: unzip -qq dir.zip -d dir

            string archiveFilePatterns = GetStepInput(step, "archiveFilePatterns");
            string destinationFolder = GetStepInput(step, "destinationFolder");
            //string cleanDestinationFolder = GetStepInput(step, "cleanDestinationFolder");
            //string overwriteExistingFiles = GetStepInput(step, "overwriteExistingFiles");

            string unzipCommand = "unzip -qq " + archiveFilePatterns + " -d " + destinationFolder;

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "montudor/action-zip@v0.1.0",
                with = new Dictionary<string, string>
                {
                    { "args", unzipCommand}
                },
                //TODO: Should there be a branch that creates a different path if it's a Windows runner?
                step_message = "Note: This is a third party action and currently only supports Linux: https://github.com/marketplace/actions/create-zip-file"
            };

            return gitHubStep;
        }

        public GitHubActions.Step CreateGitHubReleaseStep(AzurePipelines.Step step)
        {
            //From: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/utility/github-release?view=azure-devops#examples
            //# GitHub Release
            //# Create, edit, or delete a GitHub release
            //- task: GitHubRelease@0
            //  inputs:
            //    gitHubConnection: 
            //    #repositoryName: '$(Build.Repository.Name)' 
            //    #action: 'create' # Options: create, edit, delete
            //    #target: '$(Build.SourceVersion)' # Required when action == Create || Action == Edit
            //    #tagSource: 'auto' # Required when action == Create# Options: auto, manual
            //    #tagPattern: # Optional
            //    #tag: # Required when action == Edit || Action == Delete || TagSource == Manual
            //    #title: # Optional
            //    #releaseNotesSource: 'file' # Optional. Options: file, input
            //    #releaseNotesFile: # Optional
            //    #releaseNotes: # Optional
            //    #assets: '$(Build.ArtifactStagingDirectory)/*' # Optional
            //    #assetUploadMode: 'delete' # Optional. Options: delete, replace
            //    #isDraft: false # Optional
            //    #isPreRelease: false # Optional
            //    #addChangeLog: true # Optional
            //    #compareWith: 'lastFullRelease' # Required when addChangeLog . Options: lastFullRelease, lastRelease, lastReleaseByTag
            //    #releaseTag: # Required when compareWith == LastReleaseByTag

            //To: https://github.com/actions/create-release
            //- name: Create Release
            //  id: create_release
            //  uses: actions/create-release@v1
            //  env:
            //    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }} # This token is provided by Actions, you do not need to create your own token
            //  with:
            //    tag_name: ${{ github.ref }}
            //    release_name: Release ${{ github.ref }}
            //    body: |
            //      Changes in this Release
            //      - First Change
            //      - Second Change
            //    draft: false
            //    prerelease: false

            string tag = GetStepInput(step, "tag");
            string title = GetStepInput(step, "title");
            string releaseNotes = GetStepInput(step, "releaseNotes");
            string isDraft = GetStepInput(step, "isDraft");
            string isPreRelease = GetStepInput(step, "isPreRelease");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.name,
                uses = "actions/create-release@v1",
                env = new Dictionary<string, string>
                {
                    {"GITHUB_TOKEN", "${{ secrets.GITHUB_TOKEN }}"}
                },
                with = new Dictionary<string, string>
                {
                    { "tag_name", tag}
                }
            };

            if (title != null)
            {
                gitHubStep.with.Add("release_name", title);
            }
            if (releaseNotes != null)
            {
                gitHubStep.with.Add("body", releaseNotes);
            }
            if (isDraft != null)
            {
                gitHubStep.with.Add("draft", isDraft);
            }
            if (isPreRelease != null)
            {
                gitHubStep.with.Add("prerelease", isPreRelease);
            }

            return gitHubStep;
        }


        public GitHubActions.Step CreateGitVersionExecuteStep(AzurePipelines.Step step)
        {
            //From: https://github.com/GitTools/actions/blob/main/docs/examples/azure/gitversion/execute/usage-examples.md
            //- task: GitVersion@5
            //  name: gitVersion
            //  displayName: 'Evaluate Next Version'
            //  inputs:
            //    runtime: 'core'
            //    configFilePath: 'GitVersion.yml'

            //To: https://github.com/GitTools/actions/blob/main/docs/examples/github/gitversion/execute/usage-examples.md
            //- name: Determine Version
            //  uses: gittools/actions/gitversion/execute@v0.9.11

            string targetPath = GetStepInput(step, "targetPath");
            string useConfigFile = GetStepInput(step, "useConfigFile");
            string configFilePath = GetStepInput(step, "configFilePath");
            string updateAssemblyInfo = GetStepInput(step, "updateAssemblyInfo");
            string updateAssemblyInfoFilename = GetStepInput(step, "updateAssemblyInfoFilename");
            string additionalArguments = GetStepInput(step, "additionalArguments");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.name,
                uses = "gittools/actions/gitversion/execute@v0.9.11",
                with = new Dictionary<string, string>()
            };

            if (targetPath != null)
            {
                gitHubStep.with.Add("targetPath", targetPath);
            }
            if (useConfigFile != null)
            {
                gitHubStep.with.Add("useConfigFile", useConfigFile);
            }
            if (configFilePath != null)
            {
                gitHubStep.with.Add("configFilePath", configFilePath);
            }
            if (updateAssemblyInfo != null)
            {
                gitHubStep.with.Add("updateAssemblyInfo", updateAssemblyInfo);
            }
            if (updateAssemblyInfoFilename != null)
            {
                gitHubStep.with.Add("updateAssemblyInfoFilename", updateAssemblyInfoFilename);
            }
            if (additionalArguments != null)
            {
                gitHubStep.with.Add("additionalArguments", additionalArguments);
            }

            return gitHubStep;
        }

        public GitHubActions.Step CreateGitVersionSetupStep(AzurePipelines.Step step)
        {
            //From: https://github.com/GitTools/actions/blob/main/docs/examples/azure/gitversion/setup/usage-examples.md
            //- task: gitversion/setup@0
            //  displayName: 'Install GitVersion'
            //  inputs:
            //    versionSpec: '5.x'

            //To: https://github.com/GitTools/actions/blob/main/docs/examples/github/gitversion/setup/usage-examples.md
            //- name: Install GitVersion
            //  uses: gittools/actions/gitversion/setup@v0.9.11
            //  with:
            //    versionSpec: '5.x'

            string versionSpec = GetStepInput(step, "versionSpec");
            string includePrerelease = GetStepInput(step, "includePrerelease");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = step.name,
                uses = "gittools/actions/gitversion/setup@v0.9.11",
                with = new Dictionary<string, string>
                {
                   { "versionSpec", versionSpec }
                }
            };
            if (includePrerelease != null)
            {
                gitHubStep.with.Add("includePrerelease", includePrerelease);
            }

            return gitHubStep;
        }


        public GitHubActions.Step CreateGradleStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: Gradle@2
            // inputs:
            //   workingDirectory: ''
            //   gradleWrapperFile: 'gradlew'
            //   gradleOptions: '-Xmx3072m'
            //   publishJUnitResults: false
            //   testResultsFiles: '**/TEST-*.xml'
            //   tasks: 'assembleDebug'

            //Going to:
            //- name: Build with Gradle
            //  run: ./gradlew build

            step.script = "./gradlew build";
            GitHubActions.Step gitHubStep = CreateScriptStep("", step);

            return gitHubStep;
        }

        public GitHubActions.Step CreateHugoStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: HugoTask@1
            //  displayName: 'Generate Hugo site'
            //  inputs:
            //    hugoVersion: latest
            //    extendedVersion: true
            //    destination: '$(Build.ArtifactStagingDirectory)'

            //Going to: https://github.com/peaceiris/actions-hugo
            //- name: Generate Hugo site
            //  uses: peaceiris/actions-hugo@v2
            //  with:
            //    hugo-version: latest
            //    extended: true

            string hugoVersion = GetStepInput(step, "hugoVersion");
            string extendedVersion = GetStepInput(step, "extendedVersion");
            //string destination = GetStepInput(step, "destination");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "peaceiris/actions-hugo@v2",
                with = new Dictionary<string, string>
                {
                    { "hugo-version", hugoVersion}
                },
                step_message = "Note: This is a third party action: https://github.com/peaceiris/actions-hugo"
            };
            if (extendedVersion != null)
            {
                gitHubStep.with.Add("extended", extendedVersion);
            }

            return gitHubStep;
        }

        private GitHubActions.Step CreateInnerPowershellStep(AzurePipelines.Step step)
        {

            //From: 
            //- task: InlinePowershell@1                
            //  displayName: 'old Powershell task'
            //  inputs:
            //    Script: |
            //      Write-Host 'Hello World'

            //To: 
            //- name: old Powershell task
            //  run: |
            //    Write-Host 'Hello World'
            //  shell: powershell


            string script = GetStepInput(step, "script");
            step.script = script;

            GitHubActions.Step gitHubStep = CreateScriptStep("powershell", step);

            return gitHubStep;
        }

        //TODO: Finish this Kubernetes Step
        //public GitHubActions.Step CreateKubernetesStep(AzurePipelines.Step step)
        //{
        //    //coming from: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/deploy/kubernetes?view=azure-devops

        //    //Azure Resource Manager service connection
        //    //- task: Kubernetes@1
        //    //  displayName: kubectl apply
        //    //  inputs:
        //    //    connectionType: Azure Resource Manager
        //    //    azureSubscriptionEndpoint: Contoso
        //    //    azureResourceGroup: contoso.azurecr.io
        //    //    kubernetesCluster: Contoso
        //    //    useClusterAdmin: false

        //    //Kubernetes Service Connection
        //    //- task: Kubernetes@1
        //    //  displayName: kubectl apply
        //    //  inputs:
        //    //    connectionType: Kubernetes Service Connection
        //    //    kubernetesServiceEndpoint: Contoso

        //    //This YAML example demonstrates the apply command:
        //    //- task: Kubernetes@1
        //    //  displayName: kubectl apply using arguments
        //    //  inputs:
        //    //    connectionType: Azure Resource Manager
        //    //    azureSubscriptionEndpoint: $(azureSubscriptionEndpoint)
        //    //    azureResourceGroup: $(azureResourceGroup)
        //    //    kubernetesCluster: $(kubernetesCluster)
        //    //    command: apply
        //    //    arguments: -f mhc-aks.yaml

        //    //This YAML example demonstrates the use of a configuration file with the apply command:
        //    //- task: Kubernetes@1
        //    //  displayName: kubectl apply using configFile
        //    //  inputs:
        //    //    connectionType: Azure Resource Manager
        //    //    azureSubscriptionEndpoint: $(azureSubscriptionEndpoint)
        //    //    azureResourceGroup: $(azureResourceGroup)
        //    //    kubernetesCluster: $(kubernetesCluster)
        //    //    command: apply
        //    //    useConfigurationFile: true
        //    //    configuration: mhc-aks.yaml

        //    //This YAML example demonstrates the setting up of ImagePullSecrets:
        //    //- task: Kubernetes@1
        //    //  displayName: kubectl apply for secretType dockerRegistry
        //    //  inputs:
        //    //    azureSubscriptionEndpoint: $(azureSubscriptionEndpoint)
        //    //    azureResourceGroup: $(azureResourceGroup)
        //    //    kubernetesCluster: $(kubernetesCluster)
        //    //    command: apply
        //    //    arguments: -f mhc-aks.yaml
        //    //    secretType: dockerRegistry
        //    //    containerRegistryType: Azure Container Registry
        //    //    azureSubscriptionEndpointForSecrets: $(azureSubscriptionEndpoint)
        //    //    azureContainerRegistry: $(azureContainerRegistry)
        //    //    secretName: mysecretkey2
        //    //    forceUpdate: true

        //    //This YAML example creates generic secrets from literal values specified for the secretArguments input:
        //    //- task: Kubernetes@1
        //    //  displayName: secretType generic with literal values
        //    //  inputs:
        //    //    azureSubscriptionEndpoint: $(azureSubscriptionEndpoint)
        //    //    azureResourceGroup: $(azureResourceGroup)
        //    //    kubernetesCluster: $(kubernetesCluster)
        //    //    command: apply
        //    //    arguments: -f mhc-aks.yaml
        //    //    secretType: generic
        //    //    secretArguments: --from-literal=contoso=5678
        //    //    secretName: mysecretkey

        //    //This YAML example creates a ConfigMap by pointing to a ConfigMap file:
        //    //- task: Kubernetes@1
        //    //  displayName: kubectl apply
        //    //  inputs:
        //    //    configMapName: myconfig
        //    //    useConfigMapFile: true
        //    //    configMapFile: src/configmap

        //    //Going to:


        //    string arguments = GetStepInput(step, "arguments");
        //    string command = GetStepInput(step, "command");
        //    string configMapFile = GetStepInput(step, "configMapFile");
        //    string configMapName = GetStepInput(step, "configMapName");
        //    string configuration = GetStepInput(step, "configuration");
        //    string connectionType = GetStepInput(step, "connectionType");
        //    string containerRegistryType = GetStepInput(step, "containerRegistryType");
        //    string azureContainerRegistry = GetStepInput(step, "azureContainerRegistry");
        //    string azureSubscriptionEndpoint = GetStepInput(step, "azureSubscriptionEndpoint");
        //    string azureSubscriptionEndpointForSecrets = GetStepInput(step, "azureSubscriptionEndpointForSecrets");
        //    string azureResourceGroup = GetStepInput(step, "azureResourceGroup");
        //    string forceUpdate = GetStepInput(step, "forceUpdate");
        //    string kubernetesCluster = GetStepInput(step, "kubernetesCluster");
        //    string kubernetesServiceEndpoint = GetStepInput(step, "kubernetesServiceEndpoint");
        //    string secretArguments = GetStepInput(step, "secretArguments");
        //    string secretName = GetStepInput(step, "secretName");
        //    string secretType = GetStepInput(step, "secretType");
        //    string useClusterAdmin = GetStepInput(step, "useClusterAdmin");
        //    string useConfigMapFile = GetStepInput(step, "useConfigMapFile");
        //    string useConfigurationFile = GetStepInput(step, "useConfigurationFile");


        //    step.script = "";
        //    GitHubActions.Step gitHubStep = CreateScriptStep("", step);

        //    return gitHubStep;
        //}


        private GitHubActions.Step CreateAntStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: Ant@1
            //  inputs:
            //    workingDirectory: ''
            //    buildFile: 'build.xml'
            //    javaHomeOption: 'JDKVersion'
            //    jdkVersionOption: '1.8'
            //    jdkArchitectureOption: 'x64'
            //    publishJUnitResults: true
            //    testResultsFiles: '**/TEST-*.xml'  

            //Going to:
            //- name: Build with Ant
            //  run: ant -noinput -buildfile build.xml

            string buildFile = GetStepInput(step, "buildFile");

            string antCommand = "ant -noinput -buildfile " + buildFile;
            step.script = antCommand;

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);

            return gitHubStep;
        }

        private GitHubActions.Step CreateMavenStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: Maven@3
            //  inputs:
            //    mavenPomFile: 'Maven/pom.xml'
            //    mavenOptions: '-Xmx3072m'
            //    javaHomeOption: 'JDKVersion'
            //    jdkVersionOption: '1.8'
            //    jdkArchitectureOption: 'x64'
            //    publishJUnitResults: true
            //    testResultsFiles: '**/surefire-reports/TEST-*.xml'
            //    goals: 'package' 

            //Going to:
            //- name: Build with Maven
            //  run: mvn -B package --file pom.xml

            string pomFile = GetStepInput(step, "mavenPomFile");

            string pomCommand = "mvn -B package --file " + pomFile;
            step.script = pomCommand;

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);

            return gitHubStep;
        }

        private GitHubActions.Step CreateNPMStep(AzurePipelines.Step step)
        {
            //https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/package/npm?view=azure-devops
            //coming from:
            //# npm
            //# Install and publish npm packages, or run an npm command. Supports npmjs.com and authenticated registries like Azure Artifacts.
            //- task: Npm@1
            //  inputs:
            //    #command: 'install' # Options: install, publish, custom
            //    #workingDir: # Optional
            //    #verbose: # Optional
            //    #customCommand: # Required when command == Custom
            //    #customRegistry: 'useNpmrc' # Optional. Options: useNpmrc, useFeed
            //    #customFeed: # Required when customRegistry == UseFeed
            //    #customEndpoint: # Optional
            //    #publishRegistry: 'useExternalRegistry' # Optional. Options: useExternalRegistry, useFeed
            //    #publishFeed: # Required when publishRegistry == UseFeed
            //    #publishPackageMetadata: true # Optional
            //    #publishEndpoint: # Required when publishRegistry == UseExternalRegistry

            //Example:
            //- task: Npm@1
            //  displayName: 'npm install'
            //  inputs:
            //    command: install
            //    workingDir: src/angular7

            //- task: Npm@1
            //  displayName: 'Build Angular'
            //  inputs:
            //    command: custom
            //    customCommand: run build -- --prod
            //    workingDir: src/angular7


            //Going to:

            //run: npm publish --access public

            //


            string command = GetStepInput(step, "command");
            string workingDir = GetStepInput(step, "workingDir");
            string customCommand = GetStepInput(step, "customCommand");

            if (command == "custom")
            {
                step.script = "npm " + customCommand;
            }
            else
            {
                step.script = "npm " + command;
            }
            if (!string.IsNullOrEmpty(workingDir))
            {
                step.script += " " + workingDir;
            }

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);

            return gitHubStep;
        }

        private GitHubActions.Step CreateNodeToolStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: NodeTool@0
            //  inputs:
            //    versionSpec: '10.x'
            //  displayName: 'Install Node.js'

            //Going to:
            //- name: Use Node.js 10.x
            //  uses: actions/setup-node@v1
            //  with:
            //    node-version: 10.x

            string version = GetStepInput(step, "versionSpec");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = "Use Node.js " + version,
                uses = "actions/setup-node@v1",
                with = new Dictionary<string, string>
                {
                    { "node-version", version}
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateUsePythonStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: UsePythonVersion@0
            //  inputs:
            //    versionSpec: '3.7'
            //    addToPath: true
            //    architecture: 'x64'

            //Going to:
            //- name: Setup Python 3.7
            //  uses: actions/setup-python@v1
            //  with:
            //    python-version: '3.7'

            string version = GetStepInput(step, "versionSpec");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = "Setup Python " + version,
                uses = "actions/setup-python@v1",
                with = new Dictionary<string, string>
                {
                    { "python-version", version}
                }
            };


            return gitHubStep;
        }

        private GitHubActions.Step CreateUseRubyStep(AzurePipelines.Step step)
        {
            //coming from:
            //# Use Ruby version: Use the specified version of Ruby from the tool cache, optionally adding it to the PATH
            //- task: UseRubyVersion@0
            //  inputs:
            //    #versionSpec: '>= 2.4' 
            //    #addToPath: true # Optional

            //Going to:
            //- uses: actions/setup-ruby@v1
            //  with:
            //    ruby-version: 2.6.x


            string version = GetStepInput(step, "versionSpec");

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                name = "Setup Ruby " + version,
                uses = "actions/setup-ruby@v1",
                with = new Dictionary<string, string>
                {
                    { "ruby-version", version}
                }
            };


            return gitHubStep;
        }

        private GitHubActions.Step CreatePythonStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: PythonScript@0
            //  inputs:
            //    scriptSource: 'filePath'
            //    scriptPath: 'Python/Hello.py'

            //Going to:
            //- run: python Python/Hello.py

            string scriptPath = GetStepInput(step, "scriptPath");

            string pythonCommand = "python " + scriptPath;
            step.script = pythonCommand;

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);

            return gitHubStep;
        }

        //private GitHubActions.Step CreatePublishTestResultsStep(AzurePipelines.Step step)
        //{
        //    //coming from:
        //    //# Publish Test Results
        //    //- task: PublishTestResults@2
        //    //  inputs:
        //    //    #testResultsFormat: 'JUnit' # Options: JUnit, NUnit, VSTest, xUnit, cTest
        //    //    #testResultsFiles: '**/TEST-*.xml' 
        //    //    #searchFolder: '$(System.DefaultWorkingDirectory)' # Optional
        //    //    #mergeTestResults: false # Optional
        //    //    #failTaskOnFailedTests: false # Optional
        //    //    #testRunTitle: # Optional
        //    //    #buildPlatform: # Optional
        //    //    #buildConfiguration: # Optional
        //    //    #publishRunAttachments: true # Optional

        //    //TODO: Monitor this when a testing tab is finally added to GitHub
        //    //Going to:
        //    //- run: echo ""This task equivalent does not yet exist in GitHub Actions""

        //    //string scriptPath = GetStepInput(step, "scriptPath");

        //    string command = @"echo ""This task equivalent does not yet exist in GitHub Actions""";
        //    step.script = command;

        //    GitHubActions.Step gitHubStep = CreateScriptStep("", step);

        //    gitHubStep.step_message = "PublishTestResults@2 is a Azure DevOps specific task. There is no equivalent in GitHub Actions until there is a testing summary tab. See: https://github.community/t/publishing-test-results/16215";

        //    return gitHubStep;
        //}

        private GitHubActions.Step CreateArchiveFilesStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: ArchiveFiles@2
            //  displayName: 'Archive files'
            //  inputs:
            //    rootFolderOrFile: '$(System.DefaultWorkingDirectory)/publish_output'
            //    includeRootFolder: false
            //    archiveType: zip
            //    archiveFile: $(Build.ArtifactStagingDirectory)/$(Build.BuildId).zip
            //    replaceExistingArchive: true

            //Going to: //https://github.com/marketplace/actions/create-zip-file
            //- uses: montudor/action-zip@v0.1.0
            //  with:
            //    args: zip -qq -r ./dir.zip ./dir

            string rootFolderOrFile = GetStepInput(step, "rootFolderOrFile");
            string archiveFile = GetStepInput(step, "archiveFile");

            string zipCommand = "zip -qq -r " + archiveFile + " " + rootFolderOrFile;

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "montudor/action-zip@v0.1.0",
                with = new Dictionary<string, string>
                {
                    { "args", zipCommand}
                },
                step_message = "Note: This is a third party action and currently only supports Linux: https://github.com/marketplace/actions/create-zip-file"
            };

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
            if (string.IsNullOrEmpty(targetSlot))
            {
                targetSlot = "production";
            }
            //TODO: Add other properties for az webapp deployment

            string inlineScript = "az webapp deployment slot swap --resource-group " + resourceGroup +
                " --name " + webAppName +
                " --slot " + sourceSlot +
                " --target-slot " + targetSlot + "";

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "Azure/cli@v1.0.0",
                with = new Dictionary<string, string>
                {
                    { "inlineScript", inlineScript}
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateAzureCLIStep(AzurePipelines.Step step)
        {
            //From: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/deploy/azure-cli?view=azure-devops
            //- task: AzureCLI@2
            //  displayName: Azure CLI
            //  inputs:
            //    azureSubscription: <Name of the Azure Resource Manager service connection>
            //    scriptType: ps
            //    scriptLocation: inlineScript
            //    inlineScript: |
            //      az --version
            //      az account show

            //To: https://github.com/Azure/CLI
            //- name: Azure CLI script
            //  uses: azure/CLI@v1
            //  with:
            //    azcliversion: 2.0.72
            //    inlineScript: |
            //      az account show
            //      az storage -h

            //string azureSubscription = GetStepInput(step, "azuresubscription");
            //string scriptType = GetStepInput(step, "scripttype");
            string arguments = GetStepInput(step, "arguments");
            string scriptPath = GetStepInput(step, "scriptPath");
            string inlineScript = GetStepInput(step, "inlinescript");
            if (scriptPath != null)
            {
                inlineScript = scriptPath;
                if (arguments != null)
                {
                    inlineScript += " " + arguments;
                }
            }

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "azure/cli@v1.0.0",
                with = new Dictionary<string, string>
                {
                    { "inlineScript", inlineScript },
                    { "azcliversion", "latest" }
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateAzureKeyStep(AzurePipelines.Step step)
        {
            //From: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/deploy/azure-key-vault?view=azure-devops
            //- task: AzureKeyVault@1
            //  inputs:
            //    azureSubscription: 
            //    keyVaultName: 
            //    secretsFilter: '*'
            //    runAsPreJob: false # Azure DevOps Services only

            //To: https://github.com/Azure/CLI
            //- name: Azure CLI script
            //  uses: azure/CLI@v1
            //  with:
            //    azcliversion: 2.0.72
            //    inlineScript: az keyvault secret list --subscription 'mysubscription' --vault-name 'keyvaultname'

            string azureSubscription = GetStepInput(step, "azureSubscription");
            string connectedServiceName = GetStepInput(step, "connectedServiceName");
            string keyVaultName = GetStepInput(step, "keyVaultName");
            string inlineScript = "az keyvault secret list --subscription " + azureSubscription + connectedServiceName;
            inlineScript += " --vault-name " + keyVaultName;

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "azure/cli@v1.0.0",
                with = new Dictionary<string, string>
                {
                    { "inlineScript", inlineScript },
                    { "azcliversion", "latest" }
                }
            };

            return gitHubStep;
        }

        private GitHubActions.Step CreateAzurePowershellStep(AzurePipelines.Step step)
        {

            //From: https://docs.microsoft.com/en-us/azure/devops/pipelines/tasks/deploy/azure-powershell?view=azure-devops
            //# Run a PowerShell script within an Azure environment
            //- task: AzurePowerShell@4
            //  inputs:
            //    #azureSubscription: Required. Name of Azure Resource Manager service connection
            //    #scriptType: 'FilePath' # Optional. Options: filePath, inlineScript
            //    #scriptPath: # Optional
            //    #inline: '# You can write your Azure PowerShell scripts inline here. # You can also pass predefined and custom variables to this script using arguments' # Optional
            //    #scriptArguments: # Optional
            //    #errorActionPreference: 'stop' # Optional. Options: stop, continue, silentlyContinue
            //    #failOnStandardError: false # Optional
            //    #azurePowerShellVersion: 'OtherVersion' # Required. Options: latestVersion, otherVersion
            //    #preferredAzurePowerShellVersion: # Required when azurePowerShellVersion == OtherVersion

            //To: https://github.com/Azure/PowerShell
            //- name: Run Azure PowerShell script
            //  uses: azure/powershell@v1
            //  with:
            //    inlineScript: |
            //      Get-AzVM -ResourceGroupName "ResourceGroup11"
            //    azPSVersion: '3.1.0'

            //string azureSubscription = GetStepInput(step, "azuresubscription");
            //string scriptType = GetStepInput(step, "scripttype");
            string scriptArguments = GetStepInput(step, "scriptArguments");
            string scriptPath = GetStepInput(step, "scriptPath");
            string inlineScript = GetStepInput(step, "inlinescript");
            string azurePowerShellVersion = GetStepInput(step, "azurePowerShellVersion");
            string preferredAzurePowerShellVersion = GetStepInput(step, "preferredAzurePowerShellVersion");
            string targetAzurePs = GetStepInput(step, "TargetAzurePs");
            string customTargetAzurePs = GetStepInput(step, "CustomTargetAzurePs");
            string errorActionPreference = GetStepInput(step, "errorActionPreference");
            string failOnStandardError = GetStepInput(step, "failOnStandardError");
            //This task still doesn't support the scriptpath, so we assign it to the inlinescript
            if (scriptPath != null)
            {
                inlineScript = scriptPath;
                if (scriptArguments != null)
                {
                    inlineScript += " " + scriptArguments;
                }
            }
            string azPSVersion = "latest";
            if (azurePowerShellVersion != null && azurePowerShellVersion.ToLower() == "otherversion")
            {
                //There are a couple aliases for power shell version in this task
                if (preferredAzurePowerShellVersion != null)
                {
                    azPSVersion = preferredAzurePowerShellVersion;
                }
                else if (targetAzurePs != null)
                {
                    azPSVersion = targetAzurePs;
                }
                else if (customTargetAzurePs != null)
                {
                    azPSVersion = customTargetAzurePs;
                }
            }

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "azure/powershell@v1",
                with = new Dictionary<string, string>
                {
                    { "inlineScript", inlineScript},
                    { "azPSVersion", azPSVersion}
                }
            };
            if (errorActionPreference != null)
            {
                gitHubStep.with.Add("errorActionPreference", errorActionPreference);
            }
            if (failOnStandardError != null)
            {
                gitHubStep.with.Add("failOnStandardError", failOnStandardError);
            }

            return gitHubStep;
        }

        private GitHubActions.Step CreateXamarinAndroidStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: XamarinAndroid@1
            //  inputs:
            //    projectFile: '**/*droid*.csproj'
            //    outputDirectory: '$(outputDirectory)'
            //    configuration: '$(buildConfiguration)'

            //Going to: https://levelup.gitconnected.com/using-github-actions-with-ios-and-android-xamarin-apps-693a93b48a61
            //- name: Android
            //  run: |
            //    cd Blank
            //    nuget restore
            //    cd Blank.Android
            //    msbuild '**/*droid*.csproj' /verbosity:normal /t:Rebuild /p:Configuration='$(buildConfiguration)'

            string projectFile = GetStepInput(step, "projectFile");
            string configuration = GetStepInput(step, "configuration");

            string script = "" +
            "cd Blank\n" +
            "nuget restore\n" +
            "cd Blank.Android\n" +
            "msbuild " + projectFile + " /verbosity:normal /t:Rebuild /p:Configuration=" + configuration;
            step.script = script;

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);

            return gitHubStep;
        }

        private GitHubActions.Step CreateXamariniOSStep(AzurePipelines.Step step)
        {
            //coming from:
            //- task: XamariniOS@2
            //  inputs:
            //    solutionFile: '**/*.sln'
            //    configuration: 'Release'
            //    buildForSimulator: true
            //    packageApp: false

            //Going to: https://levelup.gitconnected.com/using-github-actions-with-ios-and-android-xamarin-apps-693a93b48a61
            //- name: iOS
            //  run: |
            //    cd Blank
            //    nuget restore
            //    msbuild Blank.iOS/Blank.iOS.csproj /verbosity:normal /t:Rebuild /p:Platform=iPhoneSimulator /p:Configuration=Debug

            string projectFile = GetStepInput(step, "projectFile");
            string configuration = GetStepInput(step, "configuration");

            string script = "" +
            "cd Blank\n" +
            "nuget restore\n" +
            "cd Blank.Android\n" +
            "msbuild " + projectFile + " /verbosity:normal /t:Rebuild /p:Platform=iPhoneSimulator /p:Configuration=" + configuration;
            step.script = script;

            GitHubActions.Step gitHubStep = CreateScriptStep("", step);

            return gitHubStep;
        }

        private GitHubActions.Step CreatePublishBuildArtifactsStep(AzurePipelines.Step step)
        {
            //There are 3 Azure DevOps variations
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

            //- publish: $(Build.ArtifactStagingDirectory)/$(Build.BuildId).zip
            //  artifact: drop

            //- name: publish build artifacts back to GitHub
            //  uses: actions/upload-artifact@v2
            //  with:
            //    name: console exe
            //    path: /home/runner/work/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter/AzurePipelinesToGitHubActionsConverter.ConsoleApp/bin/Release/netcoreapp3.0

            string name = "";
            if (step.inputs != null && step.inputs.ContainsKey("artifactname"))
            {
                name = GetStepInput(step, "artifactname");
            }
            string path = "";
            if (step.task?.ToUpper() == "PUBLISHBUILDARTIFACTS@1")
            {
                path = GetStepInput(step, "pathtopublish");
            }
            else if (step.task?.ToUpper() == "PUBLISHPIPELINEARTIFACT@0")
            {
                path = GetStepInput(step, "targetpath");
            }
            else if (step.task?.ToUpper() == "PUBLISHPIPELINEARTIFACT@1")
            {
                path = GetStepInput(step, "targetpath");
            }
            else if (step.publish != null)
            {
                name = step.artifact;
                path = step.publish;
            }

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                uses = "actions/upload-artifact@v2",
                with = new Dictionary<string, string>
                {
                    {"path", path}
                }
            };
            if (!string.IsNullOrEmpty(name))
            {
                gitHubStep.with.Add("name", name);
            }

            return gitHubStep;
        }

        private GitHubActions.Step CreateTemplateStep(AzurePipelines.Step step)
        {

            //There is no conversion for this: https://github.community/t5/GitHub-Actions/Call-an-action-from-another-action/td-p/45034
            //- template: templates/npm-build-steps.yaml
            //  parameters:
            //    extensionName: $(ExtensionName)



            //string name = "";
            //if (step.inputs != null && step.inputs.ContainsKey("artifactname") )
            //{
            //    name = GetStepInput(step, "artifactname");
            //}
            //string path = "";
            //if (step.task?.ToUpper() == "PUBLISHBUILDARTIFACTS@1")
            //{
            //    path = GetStepInput(step, "pathtopublish");
            //}
            //else if (step.task?.ToUpper() == "PUBLISHPIPELINEARTIFACT@0")
            //{
            //    path = GetStepInput(step, "targetpath");
            //}
            //else if (step.publish != null)
            //{
            //    name = step.artifact;
            //    path = step.publish;
            //}

            GitHubActions.Step gitHubStep = new GitHubActions.Step
            {
                run = "#" + step.template,
                step_message = "There is no conversion path for templates in GitHub Actions"
            };


            StringBuilder stepParameters = new();
            if (step.parameters != null)
            {
                foreach (KeyValuePair<string, string> item in step.parameters)
                {
                    stepParameters.Append(item.Key);
                    stepParameters.Append(": ");
                    stepParameters.Append(item.Value);
                    stepParameters.Append(System.Environment.NewLine);
                }
            }
            if (stepParameters.Length > 0)
            {
                gitHubStep.run += System.Environment.NewLine + stepParameters.ToString();
            }

            return gitHubStep;
        }

        //Safely extract the step input, if it exists
        private string GetStepInput(AzurePipelines.Step step, string name)
        {
            string input = null;
            if (step.inputs != null && name != null)
            {
                //Extract the input
                foreach (KeyValuePair<string, string> item in step.inputs)
                {
                    //Make the name lowercase to help prevent conflicts later
                    if (item.Key.ToLower() == name.ToLower())
                    {
                        input = item.Value;
                        break;
                    }
                }
            }
            return input;
        }

        //Some pipelines need supporting steps as part of the processing. 
        //For example, if we are deploying to Azure, we need to add an Azure Login step
        public GitHubActions.Step[] AddSupportingSteps(AzurePipelines.Step[] steps, bool addCheckoutStep = true)
        {
            GitHubActions.Step[] newSteps = null;
            if (steps != null)
            {
                //Start by scanning all of the steps, to see if we need to insert additional tasks
                int stepAdjustment = 0;
                bool addJavaSetupStep = false;
                bool addGradleSetupStep = false;
                bool addDockerLoginStep = false;
                bool addAzureLoginStep = false;
                bool addMSSetupStep = false;
                bool addOctopusSetupStep = false;
                bool addGitHubVersionSetupStep = false;
                string javaVersion = null;

                //If the code needs a Checkout step, add it first
                if (addCheckoutStep)
                {
                    stepAdjustment++; // we are inserting a step and need to start moving steps 1 place into the array
                }

                //Loop through the steps to see if we need other tasks inserted in for specific circumstances
                foreach (AzurePipelines.Step step in steps)
                {
                    if (step.task != null)
                    {
                        switch (step.task.ToUpper()) //Set to upper case to handle case sensitivity comparisons e.g. NPM hangles Npm, NPM, or npm. 
                        {
                            //If we have an Java based step, we will need to add a Java setup step
                            case "ANT@1":
                            case "MAVEN@3":
                                if (!addJavaSetupStep)
                                {
                                    addJavaSetupStep = true;
                                    stepAdjustment++;
                                    javaVersion = GetStepInput(step, "jdkVersionOption");
                                }
                                break;

                            //Needs a the Java step and an additional Gradle step
                            case "GRADLE@2":

                                if (!addJavaSetupStep)
                                {
                                    addJavaSetupStep = true;
                                    stepAdjustment++;
                                    //Create the java step, as it doesn't exist
                                    javaVersion = "1.8";
                                }
                                if (!addGradleSetupStep)
                                {
                                    addGradleSetupStep = true;
                                    stepAdjustment++;
                                }
                                break;


                            case "VSBUILD@1":
                                if (!addMSSetupStep)
                                {
                                    addMSSetupStep = true;
                                    stepAdjustment++;
                                }
                                break;

                            case "TERRAFORM@0":
                            case "TERRAFORMTASKV1@0":
                            case "TERRAFORMTASKV2@2":
                            case "TERRAFORMTASKV2@3":
                            case "TERRAFORMCLI@0":
                            case "MS-DEVLABS.CUSTOM-TERRAFORM-TASKS.CUSTOM-TERRAFORM-RELEASE-TASK.TERRAFORMTASKV1@0":

                                //What cloud we are deploying too - as Terraform supports multiple clouds. 
                                //Currently only supports Azure #NeedHelpForOtherClouds
                                string provider = GetStepInput(step, "provider");
                                string providerAzureConnectedServiceName = GetStepInput(step, "providerAzureConnectedServiceName");
                                string backendAzureRmResourceGroupName = GetStepInput(step, "backendAzureRmResourceGroupName");

                                if (provider == "azurerm" || !string.IsNullOrEmpty(providerAzureConnectedServiceName) || !string.IsNullOrEmpty(backendAzureRmResourceGroupName) && !addAzureLoginStep)
                                {
                                    addAzureLoginStep = true;
                                    stepAdjustment++;
                                }
                                break;

                            case "DOCKER@0":
                            case "DOCKER@1":
                            case "DOCKER@2":
                                if (!addDockerLoginStep)
                                {
                                    addDockerLoginStep = true;
                                    stepAdjustment++;
                                }
                                break;

                            case "GITVERSION@5":
                            case "GITVERSION/EXECUTE@0":
                                if (!addGitHubVersionSetupStep)
                                {
                                    addGitHubVersionSetupStep = true;
                                    stepAdjustment++;
                                }
                                break;

                            case "OCTOPUSDEPLOY.OCTOPUS-DEPLOY-BUILD-RELEASE-TASKS.OCTOPUS-PACK.OCTOPUSPACK@4":
                            case "OCTOPUSDEPLOY.OCTOPUS-DEPLOY-BUILD-RELEASE-TASKS.OCTOPUS-PUSH.OCTOPUSPUSH@4":
                                if (!addOctopusSetupStep)
                                {
                                    addOctopusSetupStep = true;
                                    stepAdjustment++;
                                }
                                break;

                            default:
                                //If we have an Azure step, we will need to add a Azure login step
                                if (step.task.ToUpper().StartsWith("AZURE"))
                                {
                                    //case "AZURECLI@2":
                                    //case "AZUREPOWERSHELL@4":
                                    //case "AZUREAPPSERVICEMANAGE@0":
                                    //case "AZURERESOURCEGROUPDEPLOYMENT@2":
                                    //case "AZURERESOURCEMANAGERTEMPLATEDEPLOYMENT@3":
                                    //case "AZUREFUNCTIONAPP@1":
                                    //case "AZUREFUNCTIONAPPCONTAINER@1":
                                    //case "AZURERMWEBAPPDEPLOYMENT@3":
                                    //case "AZURERMWEBAPPDEPLOYMENT@4":
                                    //case "AZUREWEBAPPCONTAINER@1":
                                    //case "AZUREWEBAPP@1":
                                    if (!addAzureLoginStep)
                                    {
                                        addAzureLoginStep = true;
                                        stepAdjustment++;
                                    }
                                    break;
                                }
                                break;
                        }
                    }
                }

                //Re-size the newSteps array with adjustments as needed
                newSteps = new GitHubActions.Step[steps.Length + stepAdjustment];

                int adjustmentsUsed = 0;

                //Add the steps array
                if (addCheckoutStep)
                {
                    //Add the check out step to get the code
                    newSteps[adjustmentsUsed] = CreateCheckoutStep();
                    adjustmentsUsed++;
                }
                if (addJavaSetupStep && javaVersion != null)
                {
                    //Add the JavaSetup step to the code
                    newSteps[adjustmentsUsed] = CreateSetupJavaStep(javaVersion);
                    adjustmentsUsed++;
                }
                if (addGradleSetupStep)
                {
                    //Add the Gradle setup step to the code
                    newSteps[adjustmentsUsed] = CreateSetupGradleStep();
                    adjustmentsUsed++;
                }
                if (addAzureLoginStep)
                {
                    //Add the Azure login step to the code
                    newSteps[adjustmentsUsed] = CreateAzureLoginStep();
                    adjustmentsUsed++;
                }
                if (addDockerLoginStep)
                {
                    //Add the Azure login step to the code
                    newSteps[adjustmentsUsed] = CreateDockerLoginStep(""); //TODO: Fix login server
                    adjustmentsUsed++;
                }
                if (addMSSetupStep)
                {
                    //Add the Azure login step to the code
                    newSteps[adjustmentsUsed] = CreateMSBuildSetupStep();
                    adjustmentsUsed++;
                }
                if (addOctopusSetupStep)
                {
                    //Add the Azure login step to the code
                    newSteps[adjustmentsUsed] = CreateOctopusInstallerStep();
                    adjustmentsUsed++;
                }
                if (addGitHubVersionSetupStep)
                {
                    //Add the GitVersion setup step to the code
                    AzurePipelines.Step step = new AzurePipelines.Step
                    {
                        name = "Install GitVersion",
                        inputs = new Dictionary<string, string>
                        {
                            { "versionSpec", "5.x" }
                        }
                    };
                    newSteps[adjustmentsUsed] = CreateGitVersionSetupStep(step);
                    //adjustmentsUsed++; //don't need to update the adjustment on the last item
                }

                //Translate the other steps
                for (int i = stepAdjustment; i < steps.Length + stepAdjustment; i++)
                {
                    newSteps[i] = ProcessStep(steps[i - stepAdjustment]);
                }
            }

            return newSteps;
        }
    }
}
