using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public class JobProcessing
    {
        public List<string> VariableList;
        public string MatrixVariableName;
        private readonly bool _verbose;
        public JobProcessing(bool verbose)
        {
            _verbose = verbose;
        }

        public GitHubActions.Job ProcessJob(AzurePipelines.Job job, AzurePipelines.Resources resources)
        {
            GeneralProcessing generalProcessing = new GeneralProcessing(_verbose);
            GitHubActions.Job newJob = new GitHubActions.Job
            {
                name = job.displayName,
                needs = job.dependsOn,
                _if = generalProcessing.ProcessCondition(job.condition),
                runs_on = generalProcessing.ProcessPool(job.pool),
                strategy = generalProcessing.ProcessStrategy(job.strategy),
                container = generalProcessing.ProcessContainer(resources),
                env = generalProcessing.ProcessSimpleVariables(job.variables),
                timeout_minutes = job.timeoutInMinutes,
                steps = generalProcessing.ProcessSteps(job.steps)
            };
            MatrixVariableName = generalProcessing.MatrixVariableName;
            VariableList = generalProcessing.VariableList;


            if (newJob.steps == null & job.template != null)
            {
                //Initialize the array with no items
                job.steps = new AzurePipelines.Step[0];
                //Process the steps, adding the default checkout step
                newJob.steps = generalProcessing.ProcessSteps(job.steps, true);
                //TODO: Find a way to allow GitHub jobs to reference another job as a template
                newJob.job_message += "Note: Azure DevOps template does not have an equivalent in GitHub Actions yet";
            }
            else if (newJob.steps == null && job.strategy?.runOnce?.deploy?.steps != null)
            {
                //Initialize the array with no items
                job.steps = new AzurePipelines.Step[0];
                //Process the steps, adding the default checkout step
                newJob.steps = generalProcessing.ProcessSteps(job.strategy?.runOnce?.deploy?.steps, false);
                //TODO: Find a way to allow GitHub jobs to reference another job as a template
                newJob.job_message += "Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yet";
            }
            if (job.environment != null)
            {
                newJob.job_message += "Note: Azure DevOps job environment does not have an equivalent in GitHub Actions yet";
            }
            //if (newJob._if != null)
            //{
            //    if (newJob.job_message != null)
            //    {
            //        newJob.job_message += System.Environment.NewLine;
            //    }
            //}
            if (job.continueOnError)
            {
                newJob.continue_on_error = job.continueOnError;
            }

            return newJob;
        }

    }
}
