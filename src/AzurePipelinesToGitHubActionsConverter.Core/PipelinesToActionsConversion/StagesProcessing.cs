using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using System.Collections.Generic;
using System.Text.Json;
using GitHubActions = GitHubActionsDotNet.Models;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public class StagesProcessing
    {
        private readonly bool _verbose;
        public StagesProcessing(bool verbose)
        {
            _verbose = verbose;
        }

        public Dictionary<string, GitHubActions.Job> ProcessStagesV2(JsonElement stagesJson, JsonElement strategyJson)
        {
            AzurePipelines.Job[] jobs = null;
            List<AzurePipelines.Stage> stages = new List<AzurePipelines.Stage>();
            if (stagesJson.ValueKind != JsonValueKind.Undefined)
            {
                //for each stage
                foreach (JsonElement stageJson in stagesJson.EnumerateArray())
                {
                    AzurePipelines.Stage stage = new AzurePipelines.Stage();
                    JsonElement jsonElement;
                    if (stageJson.TryGetProperty("stage", out jsonElement))
                    {
                        stage.stage = jsonElement.ToString();
                    }
                    if (stageJson.TryGetProperty("displayName", out jsonElement))
                    {
                        stage.displayName = jsonElement.ToString();
                    }
                    if (stageJson.TryGetProperty("condition", out jsonElement))
                    {
                        stage.condition = jsonElement.ToString();
                    }
                    if (stageJson.TryGetProperty("dependsOn", out jsonElement))
                    {
                        GeneralProcessing gp = new GeneralProcessing(_verbose);
                        stage.dependsOn = gp.ProcessDependsOnV2(jsonElement.ToString());
                    }
                    if (stageJson.TryGetProperty("variables", out jsonElement))
                    {
                        VariablesProcessing vp = new VariablesProcessing(_verbose);
                        stage.variables = vp.ProcessParametersAndVariablesV2(null, jsonElement.ToString());
                    }
                    if (stageJson.TryGetProperty("jobs", out jsonElement))
                    {
                        JobProcessing jp = new JobProcessing(_verbose);
                        stage.jobs = jp.ExtractAzurePipelinesJobsV2(jsonElement, strategyJson);
                    }
                    if (stageJson.TryGetProperty("pool", out jsonElement) && stage.jobs != null)
                    {
                        GeneralProcessing gp = new GeneralProcessing(_verbose);
                        stage.pool = gp.ProcessPoolV2(jsonElement.ToString());
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
                                        if (!jobs[jobIndex].variables.ContainsKey(stageVariable.Key))
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
                                jobs[jobIndex].job = ConversionUtility.GenerateCombinedStageJobName(stage.stage, jobName);
                                jobs[jobIndex].stageName = stage.stage;

                                //Process the stage depends on, incorporating the job depends on
                                if (stage.dependsOn != null)
                                {
                                    List<string> stageDependsOn = new List<string>();
                                    foreach (string item in stage.dependsOn)
                                    {
                                        //get every job from each of the depends on stages, and create the combined stage/job name
                                        List<string> stageJobs = GetStageJobs(item, stages);
                                        stageDependsOn.AddRange(stageJobs);
                                    }

                                    //then combine this new stage depends on list with the jobs depends on - being careful not to stomp on anything that already exists.
                                    if (jobs[jobIndex].dependsOn != null)
                                    {
                                        foreach (string item in jobs[jobIndex].dependsOn)
                                        {
                                            stageDependsOn.Add(ConversionUtility.GenerateCombinedStageJobName(jobs[jobIndex].stageName, item));
                                        }
                                        jobs[jobIndex].dependsOn = stageDependsOn.ToArray();
                                    }
                                    else
                                    {
                                        jobs[jobIndex].dependsOn = stageDependsOn.ToArray();
                                    }
                                }
                                else if (jobs[jobIndex].dependsOn != null)
                                {
                                    for (int j = 0; j < jobs[jobIndex].dependsOn.Length; j++)
                                    {
                                        jobs[jobIndex].dependsOn[j] = ConversionUtility.GenerateCombinedStageJobName(jobs[jobIndex].stageName, jobs[jobIndex].dependsOn[j]);
                                    }
                                }
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

        private List<string> GetStageJobs(string stage, List<Stage> stages)
        {
            List<string> jobs = new List<string>();
            foreach (Stage item in stages)
            {
                if (item.stage == stage)
                {
                    //reinitialize the jobs array with the jobs length
                    //jobs = new string[item.jobs.Length];
                    //Add each job name to the list
                    for (int i = 0; i < item.jobs.Length; i++)
                    {
                        Job job = item.jobs[i];
                        jobs.Add(job.job);
                    }
                }
            }
            return jobs;
        }

    }
}
