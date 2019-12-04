using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class AzurePipelinesProcessing<T>
    {
        public List<string> VariableList;
        public string MatrixVariableName;

        public GitHubActionsRoot ProcessPipeline(AzurePipelinesRoot<T> azurePipeline, string[] simpleTrigger, AzurePipelines.Trigger complexTrigger)
        {
            VariableList = new List<string>();
            GitHubActionsRoot gitHubActions = new GitHubActionsRoot();
            //Name
            if (azurePipeline.name != null)
            {
                gitHubActions.name = azurePipeline.name;
            }

            //Resources
            if (azurePipeline.resources != null)
            {
                gitHubActions.messages.Add( "TODO: Resource conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/8");
            }

            //Container
            if (azurePipeline.container != null)
            {
                gitHubActions.messages.Add( "TODO: Container conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39");
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

            //Stages (Note: stages are not yet enabled in actions, we are merging them into one giant list of jobs)
            if (azurePipeline.stages != null)
            {
                //Count the number of jobs and initialize the jobs array with that number
                int jobCounter = 0;
                foreach (Stage item in azurePipeline.stages)
                {
                    jobCounter += item.jobs.Length;
                }
                azurePipeline.jobs = new AzurePipelines.Job[jobCounter];

                //We are going to take each stage and assign it a set of jobs
                int currentIndex = 0;
                foreach (Stage item in azurePipeline.stages)
                {
                    int j = 0;
                    for (int i = currentIndex; i < currentIndex + item.jobs.Length; i++)
                    {
                        //Rename the job, using the stage name as prefix, so that we keep the job names unique
                        item.jobs[j].job = item.stage + "_" + item.jobs[j].job;
                        azurePipeline.jobs[i] = item.jobs[j];
                        azurePipeline.jobs[i].condition = item.condition;
                        j++;
                    }
                    currentIndex += item.jobs.Length;
                }
            }

            //Jobs (when no stages are defined)
            if (azurePipeline.jobs != null)
            {
                //If there is a parent strategy, and no child strategy, load in the parent
                //This is not perfect...
                if (azurePipeline.strategy != null)
                {
                    foreach (AzurePipelines.Job item in azurePipeline.jobs)
                    {
                        if (item.strategy == null)
                        {
                            item.strategy = azurePipeline.strategy;
                        }
                    }
                }
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
                            strategy = ProcessStrategy(azurePipeline.strategy),
                            steps = ProcessSteps(azurePipeline.steps)
                        }
                    }
                };
            }

            //Variables
            if (azurePipeline.variables != null)
            {
                gitHubActions.env = ProcessVariables(azurePipeline.variables);
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
        private string ProcessPool(Pool pool)
        {
            string newPool = null;
            if (pool != null)
            {
                newPool = pool.vmImage;
            }
            return newPool;
        }

        //process the strategy matrix
        private GitHubActions.Strategy ProcessStrategy(AzurePipelines.Strategy strategy)
        {
            //Azure DevOps
            //strategy:
            //  matrix:
            //    linux:
            //      imageName: ubuntu - 16.04
            //    mac:
            //      imageName: macos-10.13
            //    windows:
            //      imageName: vs2017-win2016
            //jobs:
            //- job: Build
            //  pool: 
            //    vmImage: $(imageName)

            //GitHub Actions
            //runs-on: ${{ matrix.imageName }}
            //strategy:
            //  matrix:
            //    imageName: [ubuntu-16.04, macos-10.13, vs2017-win2016]

            if (strategy != null)
            {
                string[] matrix = new string[strategy.matrix.Count];
                KeyValuePair<string, Dictionary<string, string>> matrixVariable = strategy.matrix.First();
                MatrixVariableName = matrixVariable.Value.Keys.First();
                //VariableList.Add("$(" + _matrixVariableName + ")");
                int i = 0;
                foreach (KeyValuePair<string, Dictionary<string, string>> entry in strategy.matrix)
                {
                    matrix[i] = strategy.matrix[entry.Key][MatrixVariableName];
                    i++;
                }
                return new GitHubActions.Strategy()
                {
                    matrix = new Dictionary<string, string[]>
                    {
                        { MatrixVariableName, matrix }
                    },
                    max_parallel = strategy.maxParallel
                };

            }
            else
            {
                return null;
            }
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
                runs_on = ProcessPool(job.pool),
                strategy = ProcessStrategy(job.strategy),
                env = ProcessVariables(job.variables),
                timeout_minutes = job.timeoutInMinutes,
                steps = ProcessSteps(job.steps)
            };
        }

        //process the steps
        private GitHubActions.Step[] ProcessSteps(AzurePipelines.Step[] steps, bool addCheckoutStep = true)
        {
            AzurePipelinesStepsProcessing stepsProcessing = new AzurePipelinesStepsProcessing();

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
                    newSteps[i] = stepsProcessing.ProcessStep(steps[i - adjustment]);
                }
            }

            return newSteps;
        }
    }
}
