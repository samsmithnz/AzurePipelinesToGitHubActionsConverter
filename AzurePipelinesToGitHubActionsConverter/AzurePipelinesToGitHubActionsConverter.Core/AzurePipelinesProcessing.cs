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

        public GitHubActionsRoot ProcessPipeline(AzurePipelinesRoot<T> azurePipeline, string[] simpleTrigger, Trigger complexTrigger)
        {
            GitHubActionsRoot gitHubActions = new GitHubActionsRoot();
            //Name
            if (azurePipeline.name != null)
            {
                gitHubActions.name = azurePipeline.name;
            }

            //Trigger
            if (azurePipeline.trigger != null)
            {
                if (complexTrigger != null)
                {
                    //gitHubActions.on = 
                    ProcessComplexTrigger(complexTrigger);
                }
                else if (simpleTrigger != null)
                {
                    gitHubActions.on = ProcessSimpleTrigger(simpleTrigger);
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

        private string[] ProcessSimpleTrigger(string[] trigger)
        {
            return new string[] { "push" };
        }

        private Trigger ProcessComplexTrigger(Trigger trigger)
        {
            //TODO: Add more processing on different triggers
            return new Trigger
            {

            };
        }

        private string ProcessPool(Pool pool)
        {
            string newPool = null;
            if (pool != null)
            {
                newPool = pool.vmImage;
            }
            return newPool;
        }

        private Dictionary<string, string> ProcessVariables(Dictionary<string, string> variables)
        {
            VariableList = new List<string>();
            //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
            foreach (string item in variables.Keys)
            {
                VariableList.Add(item);
            }

            return variables;
        }

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
                    newSteps[i] = new GitHubActions.Step
                    {
                        name = steps[i - adjustment].displayName,
                        run = steps[i - adjustment].script
                    };
                }
            }

            return newSteps;
        }
    }
}
