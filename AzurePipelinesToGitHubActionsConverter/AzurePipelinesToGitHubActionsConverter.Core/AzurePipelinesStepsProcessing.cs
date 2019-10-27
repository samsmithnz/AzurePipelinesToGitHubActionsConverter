using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System.Collections.Generic;
using System.Linq;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class AzurePipelinesStepsProcessing
    {
        public List<string> VariableList;

        //This section is very much in Alpha. It has long way to go.
        public GitHubActions.Step ProcessStep(AzurePipelines.Step step)
        {

            if (step.task != null)
            {
                string taskName = null;
                GitHubActions.Step gitHubStep = null;
                switch (step.task)
                {
                    case "CmdLine@2":
                        gitHubStep = CreateScript("cmd", step);
                        break;
                    case "CopyFiles@2":
                        //Use PowerShell to copy files
                        step.script = "Copy " + step.inputs["SourceFolder"] + "/" + step.inputs["Contents"] + " " + step.inputs["TargetFolder"];
                        gitHubStep = CreateScript("powershell", step);
                        break;
                    case "DotNetCoreCLI@2":
                        gitHubStep = CreateDotNetCommand(step);
                        break;
                    case "PowerShell@2":
                        gitHubStep = CreateScript("powershell", step);
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
                    case "PublishBuildArtifacts@1":
                        gitHubStep = new GitHubActions.Step
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

                        break;
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

                    default:
                        taskName = "***unknown task***" + step.task;
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
                return CreateScript("pwsh", step);
            }
            else if (step.powershell != null)
            {
                return CreateScript("pwsh", step);
            }
            else if (step.bash != null)
            {
                return CreateScript("bash", step);
            }
            else
            {
                return new GitHubActions.Step
                {
                    name = "***This step is not currently supported***: " + step.displayName
                };
            }
        }

        private GitHubActions.Step CreateDotNetCommand(AzurePipelines.Step step)
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

        private GitHubActions.Step CreateScript(string shellType, AzurePipelines.Step step)
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
                step.inputs.TryGetValue("script", out string value);
                gitHubStep.run = value;
            }

            return gitHubStep;
        }
    }
}
