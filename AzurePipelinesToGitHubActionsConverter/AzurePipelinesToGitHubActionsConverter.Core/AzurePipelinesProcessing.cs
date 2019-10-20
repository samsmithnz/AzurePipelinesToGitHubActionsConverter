using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class AzurePipelinesProcessing<T>
    {
        public List<string> VariableList;

        public GitHubActionsRoot ProcessPipeline(AzurePipelinesRoot<T> azurePipeline, string[] simpleTrigger, AzurePipelines.Trigger complexTrigger)
        {
            VariableList = new List<string>();
            GitHubActionsRoot gitHubActions = new GitHubActionsRoot();
            //Name
            if (azurePipeline.name != null)
            {
                gitHubActions.name = azurePipeline.name;
            }

            //Triggers for pushs 
            if (azurePipeline.trigger != null)
            {
                if (complexTrigger != null)
                {
                    gitHubActions.on = ProcessComplexTrigger(complexTrigger);
                }
                else if (simpleTrigger != null)
                {
                    gitHubActions.on = ProcessSimpleTrigger(simpleTrigger);
                }
            }

            //Triggers for pull requests
            if (azurePipeline.pr != null)
            {
                if (azurePipeline.pr != null)
                {
                    GitHubActions.Trigger pr = ProcessPullRequest(azurePipeline.pr);
                    if (gitHubActions.on == null)
                    {
                        gitHubActions.on = pr;
                    }
                    else
                    {
                        gitHubActions.on.pull_request = pr.pull_request;
                    }

                }
            }

            //Variables
            if (azurePipeline.variables != null)
            {
                gitHubActions.env = ProcessVariables(azurePipeline.variables);
            }

            //Stages
            if (azurePipeline.stages != null)
            {
                //TODO: Stages are not yet supported in GitHub actions (I think?)
                //We are just going to take the first stage and use it's jobs
                azurePipeline.jobs = azurePipeline.stages[0].jobs;
            }

            //Jobs (when no stages are defined)
            if (azurePipeline.jobs != null)
            {
                gitHubActions.jobs = ProcessJobs(azurePipeline.jobs);
            }

            //Pool + Steps (When there are no jobs defined)
            if (azurePipeline.pool != null ||
                    (azurePipeline.steps != null && azurePipeline.steps.Length > 0))
            {
                //Steps only have one job, so we just create it here
                gitHubActions.jobs = new Dictionary<string, GitHubActions.Job>
                    {
                        {
                            "build",
                            new GitHubActions.Job
                            {
                                runs_on = ProcessPool(azurePipeline.pool),
                                steps = ProcessSteps(azurePipeline.steps)
                            }
                        }
                    };
            }

            return gitHubActions;
        }

        //Process a simple trigger, e.g. "Trigger: [master, develop]"
        private GitHubActions.Trigger ProcessSimpleTrigger(string[] trigger)
        {
            AzurePipelines.Trigger newTrigger = new AzurePipelines.Trigger
            {
                branches = new IncludeExclude
                {
                    include = trigger
                }
            };
            return ProcessComplexTrigger(newTrigger);
        }

        //Process a complex trigger, using the Trigger object
        private GitHubActions.Trigger ProcessComplexTrigger(AzurePipelines.Trigger trigger)
        {
            //Note: as of 18-Oct, you receive an error if you try to post both a "branches" and a "ignore-branches", or a "paths and a ignore-paths". You can only have one or the other...
            TriggerDetail push = new TriggerDetail();
            //process branches
            if (trigger.branches != null)
            {
                if (trigger.branches.include != null)
                {
                    push.branches = trigger.branches.include;
                }
                else if (trigger.branches.exclude != null)
                {
                    push.branches_ignore = trigger.branches.exclude;
                }
            }
            //process paths
            if (trigger.paths != null)
            {
                if (trigger.paths.include != null)
                {
                    push.paths = trigger.paths.include;
                }
                if (trigger.paths.exclude != null)
                {
                    push.paths_ignore = trigger.paths.exclude;
                }
            }
            //process tags
            if (trigger.tags != null)
            {
                if (trigger.tags.include != null)
                {
                    push.tags = trigger.tags.include;
                }
                if (trigger.tags.exclude != null)
                {
                    push.tags_ignore = trigger.tags.exclude;
                }
            }

            return new GitHubActions.Trigger
            {
                push = push
            };

        }

        //process the pull request
        private GitHubActions.Trigger ProcessPullRequest(AzurePipelines.Trigger pr)
        {
            TriggerDetail pullRequest = new TriggerDetail();
            //process branches
            if (pr.branches != null)
            {
                if (pr.branches.include != null)
                {
                    pullRequest.branches = pr.branches.include;
                }
                else if (pr.branches.exclude != null)
                {
                    pullRequest.branches_ignore = pr.branches.exclude;
                }
            }
            //process paths
            if (pr.paths != null)
            {
                if (pr.paths.include != null)
                {
                    pullRequest.paths = pr.paths.include;
                }
                if (pr.paths.exclude != null)
                {
                    pullRequest.paths_ignore = pr.paths.exclude;
                }
            }
            //process tags
            if (pr.tags != null)
            {
                if (pr.tags.include != null)
                {
                    pullRequest.tags = pr.tags.include;
                }
                if (pr.tags.exclude != null)
                {
                    pullRequest.tags_ignore = pr.tags.exclude;
                }
            }

            return new GitHubActions.Trigger
            {
                pull_request = pullRequest
            };
        }

        //process the build pool/agent
        //TODO: Resolve bug where I can't use a variable for runs-on
        private string ProcessPool(Pool pool)
        {
            string newPool = null;
            if (pool != null)
            {
                newPool = pool.vmImage;
            }
            return newPool;
        }

        //process all variables
        private Dictionary<string, string> ProcessVariables(Dictionary<string, string> variables)
        {
            if (variables != null)
            {
                //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
                foreach (string item in variables.Keys)
                {
                    VariableList.Add(item);
                }
            }

            return variables;
        }

        //process the jobs
        private Dictionary<string, GitHubActions.Job> ProcessJobs(AzurePipelines.Job[] jobs)
        {
            //A dictonary is perfect here, as the job_id (a string), must be unique in the action
            Dictionary<string, GitHubActions.Job> newJobs = null;
            if (jobs != null)
            {
                newJobs = new Dictionary<string, GitHubActions.Job>();
                for (int i = 0; i < jobs.Length; i++)
                {
                    newJobs.Add(jobs[i].job, ProcessIndividualJob(jobs[i]));
                }
            }
            return newJobs;
        }

        private GitHubActions.Job ProcessIndividualJob(AzurePipelines.Job job)
        {
            return new GitHubActions.Job
            {
                name = job.displayName,
                needs = job.dependsOn,
                _if = job.condition,
                env = ProcessVariables(job.variables),
                runs_on = ProcessPool(job.pool),
                timeout_minutes = job.timeoutInMinutes,
                steps = ProcessSteps(job.steps)
            };
        }

        //process the steps
        private GitHubActions.Step[] ProcessSteps(AzurePipelines.Step[] steps, bool addCheckoutStep = true)
        {
            //TODO: Add support to actually translate Azure Pipeline steps to GitHub Action steps. This has the potential to be the biggest task here due to the possible number of permutations.
            GitHubActions.Step[] newSteps = null;
            if (steps != null)
            {
                int adjustment = 0;
                if (addCheckoutStep == true)
                {
                    adjustment = 1; // we are inserting a step and need to start moving steps 1 place into the array
                }
                newSteps = new GitHubActions.Step[steps.Length + adjustment]; //Add 1 for the check out step

                //Add the check out step to get the code
                if (addCheckoutStep == true)
                {
                    newSteps[0] = new GitHubActions.Step
                    {
                        uses = "actions/checkout@v1"
                    };
                }

                //Translate the other steps
                for (int i = adjustment; i < steps.Length + adjustment; i++)
                {
                    newSteps[i] = ProcessStep(steps[i - adjustment]);
                }
            }

            return newSteps;
        }

        //This section is very much in Alpha. It has long way to go.
        //TODO: Refactor this into a separate class
        private GitHubActions.Step ProcessStep(AzurePipelines.Step step)
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

                if (gitHubStep == null)
                {
                    return new GitHubActions.Step
                    {
                        name = step.displayName,
                        uses = taskName,
                        with = step.inputs
                    };
                }
                else
                {
                    return gitHubStep;
                }
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
