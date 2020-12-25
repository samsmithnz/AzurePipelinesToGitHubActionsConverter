using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public class StagesProcessing
    {
        private readonly bool _verbose;
        public StagesProcessing(bool verbose)
        {
            _verbose = verbose;
        }

        public Dictionary<string, GitHubActions.Job> ProcessStagesV2(JToken stagesJson, JToken strategyJson)
        {
            AzurePipelines.Job[] jobs = null;
            List<AzurePipelines.Stage> stages = new List<AzurePipelines.Stage>();
            if (stagesJson != null)
            {
                //for each stage
                foreach (JToken stageJson in stagesJson)
                {
                    AzurePipelines.Stage stage = new AzurePipelines.Stage();
                    if (stageJson["stage"] != null)
                    {
                        stage.stage = stageJson["stage"].ToString();
                    }
                    if (stageJson["displayName"] != null)
                    {
                        stage.displayName = stageJson["displayName"].ToString();
                    }
                    if (stageJson["condition"] != null)
                    {
                        stage.condition = stageJson["condition"].ToString();
                    }
                    if (stageJson["dependsOn"] != null)
                    {
                        GeneralProcessing gp = new GeneralProcessing(_verbose);
                        stage.dependsOn = gp.ProcessDependsOnV2(stageJson["dependsOn"].ToString());
                    }
                    if (stageJson["variables"] != null)
                    {
                        VariablesProcessing vp = new VariablesProcessing(_verbose);
                        stage.variables = vp.ProcessParametersAndVariablesV2(null, stageJson["variables"].ToString());
                    }
                    if (stageJson["jobs"] != null)
                    {
                        JobProcessing jp = new JobProcessing(_verbose);
                        stage.jobs = jp.ExtractAzurePipelinesJobsV2(stageJson["jobs"], strategyJson);
                    }
                    if (stageJson["pool"] != null && stage.jobs != null)
                    {
                        GeneralProcessing gp = new GeneralProcessing(_verbose);
                        stage.pool = gp.ProcessPoolV2(stageJson["pool"].ToString());
                        foreach (Job item in stage.jobs)
                        {
                            //Only update the job pool if it hasn't already been set by the job
                            if (item.pool == null)
                            {
                                item.pool = stage.pool;
                            }
                        }
                    }
                    stages.Add(stage);
                }

                //process the jobs
                if (stages != null)
                {
                    int jobCount = 0;
                    foreach (Stage stage in stages)
                    {
                        if (stage.jobs != null)
                        {
                            jobCount += stage.jobs.Length;
                        }
                    }
                    jobs = new AzurePipelines.Job[jobCount];

                    //Giant nested loop ahead. Loop through stages, looking for all jobs
                    int jobIndex = 0;
                    foreach (Stage stage in stages)
                    {
                        if (stage.jobs != null)
                        {
                            for (int i = 0; i < stage.jobs.Length; i++)
                            {
                                jobs[jobIndex] = stage.jobs[i];
                                if (stage.variables != null)
                                {
                                    if (jobs[jobIndex].variables == null)
                                    {
                                        jobs[jobIndex].variables = new Dictionary<string, string>();
                                    }
                                    foreach (KeyValuePair<string, string> stageVariable in stage.variables)
                                    {
                                        //Add the stage variable if it doesn't already exist
                                        if (jobs[jobIndex].variables.ContainsKey(stageVariable.Key) == false)
                                        {
                                            jobs[jobIndex].variables.Add(stageVariable.Key, stageVariable.Value);
                                        }
                                    }
                                }
                                if (stage.condition != null)
                                {
                                    jobs[jobIndex].condition = stage.condition;
                                }
                                //Get the job name
                                string jobName = ConversionUtility.GenerateJobName(stage.jobs[i], jobIndex);
                                //Rename the job, using the stage name as prefix, so that we keep the job names unique
                                jobs[jobIndex].job = stage.stage + "_Stage_" + jobName;

                                //TODO: Figure out how to do stage depends. I think we need need to keep track of previous stages and the jobs they transform into
                                ////Process the stage depends on
                                //if (stage.dependsOn != null && jobs[jobIndex].dependsOn == null)
                                //{
                                //    jobs[jobIndex].dependsOn = stage.dependsOn;
                                //}
                                jobIndex++;
                            }
                        }
                    }
                }
            }

            //Build the final list of GitHub jobs and return it
            Dictionary<string, GitHubActions.Job> gitHubJobs = null;
            if (jobs != null)
            {
                gitHubJobs = new Dictionary<string, GitHubActions.Job>();
                foreach (AzurePipelines.Job job in jobs)
                {
                    JobProcessing jobProcessing = new JobProcessing(_verbose);
                    gitHubJobs.Add(job.job, jobProcessing.ProcessJob(job, null));
                }
            }
            return gitHubJobs;
        }

    }
}
