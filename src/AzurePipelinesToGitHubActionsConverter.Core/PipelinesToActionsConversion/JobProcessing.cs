using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
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
            VariablesProcessing vp = new VariablesProcessing(_verbose);
            StepsProcessing sp = new StepsProcessing();
            GitHubActions.Job newJob = new GitHubActions.Job
            {
                name = job.displayName,
                needs = job.dependsOn,
                _if = ConditionsProcessing.TranslateConditions(job.condition),
                runs_on = generalProcessing.ProcessPool(job.pool),
                strategy = generalProcessing.ProcessStrategy(job.strategy),
                container = generalProcessing.ProcessContainer(resources),
                env = vp.ProcessSimpleVariables(job.variables),
                timeout_minutes = job.timeoutInMinutes,
                environment = ProcessJobEnvironment(job.environment),
                steps = sp.AddSupportingSteps(job.steps)
            };
            MatrixVariableName = generalProcessing.MatrixVariableName;
            VariableList = vp.VariableList;

            if (newJob.steps == null & job.template != null)
            {
                //Initialize the array with no items
                job.steps = new AzurePipelines.Step[0];
                //Process the steps, adding the default checkout step
                newJob.steps = sp.AddSupportingSteps(job.steps, true);
                //TODO: There is currently no conversion path for templates
                newJob.job_message += "Note: Azure DevOps template does not have an equivalent in GitHub Actions yet";
            }
            else if (newJob.steps == null && job.strategy?.runOnce?.deploy?.steps != null)
            {
                //Initialize the array with no items
                job.steps = new AzurePipelines.Step[0];
                //Process the steps, adding the default checkout step
                newJob.steps = sp.AddSupportingSteps(job.strategy.runOnce.deploy.steps, false);
                //TODO: There is currently no conversion path for runOnce strategy
                newJob.job_message += "Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps";
            }
            else if (newJob.steps == null && job.strategy?.canary?.deploy?.steps != null)
            {
                //Initialize the array with no items
                job.steps = new AzurePipelines.Step[0];
                //Process the steps, adding the default checkout step
                newJob.steps = sp.AddSupportingSteps(job.strategy.canary.deploy.steps, false);
                //TODO: There is currently no conversion path for runOnce strategy
                newJob.job_message += "Note: Azure DevOps strategy>canary does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps";
            }
            else if (newJob.steps == null && job.strategy?.rolling?.deploy?.steps != null)
            {
                //Initialize the array with no items
                job.steps = new AzurePipelines.Step[0];
                //Process the steps, adding the default checkout step
                newJob.steps = sp.AddSupportingSteps(job.strategy.rolling.deploy.steps, false);
                //TODO: There is currently no conversion path for runOnce strategy
                newJob.job_message += "Note: Azure DevOps strategy>rolling does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps";
            }


            //TODO: Add this pools in for other strategies  
            //if (newJob.runs_on == null && job.strategy?.runOnce?.deploy?.pool != null)
            //{
            //    newJob.runs_on = generalProcessing.ProcessPool(job.strategy?.runOnce?.deploy?.pool);
            //}
            //else if (newJob.runs_on == null && job.strategy?.canary?.deploy?.pool != null)
            //{
            //    newJob.runs_on = generalProcessing.ProcessPool(job.strategy?.canary?.deploy?.pool);
            //}
            //else if (newJob.runs_on == null && job.strategy?.rolling?.deploy?.pool != null)
            //{
            //    newJob.runs_on = generalProcessing.ProcessPool(job.strategy?.rolling?.deploy?.pool);
            //}
            if (job.continueOnError == true)
            {
                newJob.continue_on_error = job.continueOnError;
            }

            return newJob;
        }


        public Dictionary<string, GitHubActions.Job> ProcessJobsV2(AzurePipelines.Job[] jobs, Resources resources)
        {
            if (jobs != null)
            {
                Dictionary<string, GitHubActions.Job> gitHubJobs = new Dictionary<string, GitHubActions.Job>();
                int i = 0;
                foreach (AzurePipelines.Job job in jobs)
                {
                    JobProcessing jobProcessing = new JobProcessing(_verbose);
                    string jobName = job.job;
                    if (jobName == null && job.deployment != null)
                    {
                        jobName = job.deployment;
                    }
                    else if (jobName == null && job.template != null)
                    {
                        jobName = "job_" + (i + 1).ToString() + "_template";
                    }
                    gitHubJobs.Add(jobName, jobProcessing.ProcessJob(job, resources));
                    MatrixVariableName = jobProcessing.MatrixVariableName;
                    i++;
                }
                return gitHubJobs;
            }
            else
            {
                return null;
            }
        }

        public AzurePipelines.Job[] ExtractAzurePipelinesJobsV2(JToken jobsJson, JToken strategyJson)
        {
            GeneralProcessing gp = new GeneralProcessing(_verbose);
            StrategyProcessing sp = new StrategyProcessing(_verbose);
            AzurePipelines.Job[] jobs = new AzurePipelines.Job[jobsJson.Count()];
            if (jobsJson != null)
            {
                int i = 0;
                foreach (JToken jobJson in jobsJson)
                {
                    AzurePipelines.Job job = new AzurePipelines.Job
                    {
                        job = jobJson["job"]?.ToString(),
                        deployment = jobJson["deployment"]?.ToString(),
                        displayName = jobJson["displayName"]?.ToString(),
                        template = jobJson["template"]?.ToString()
                    };
                    if (jobJson["pool"] != null)
                    {
                        job.pool = gp.ProcessPoolV2(jobJson["pool"].ToString());
                    }
                    //Strategy
                    if (jobJson["strategy"] != null)
                    {
                        strategyJson = jobJson["strategy"];
                    }
                    if (strategyJson != null)
                    {
                        job.strategy = sp.ProcessStrategyV2(strategyJson);
                    }

                    if (jobJson["dependsOn"] != null)
                    {
                        job.dependsOn = gp.ProcessDependsOnV2(jobJson["dependsOn"].ToString());
                    }
                    if (jobJson["condition"] != null)
                    {
                        job.condition = jobJson["condition"].ToString();
                    }
                    if (jobJson["environment"] != null)
                    {
                        job.environment = gp.ProcessEnvironmentV2(jobJson["environment"].ToString());
                    }
                    if (jobJson["timeoutInMinutes"] != null)
                    {
                        int.TryParse(jobJson["timeoutInMinutes"].ToString(), out int timeOut);
                        if (timeOut > 0)
                        {
                            job.timeoutInMinutes = timeOut;
                        }
                    }
                    if (jobJson["continueOnError"] != null)
                    {
                        bool.TryParse(jobJson["continueOnError"].ToString(), out bool continueOnError);
                        job.continueOnError = continueOnError;
                    }
                    if (jobJson["variables"] != null)
                    {
                        VariablesProcessing vp = new VariablesProcessing(_verbose);
                        job.variables = vp.ProcessParametersAndVariablesV2(null, jobJson["variables"].ToString());
                    }
                    //Currently no conversion path for services
                    if (jobJson["services"] != null)
                    {
                        job.services = new Dictionary<string, string>();
                    }
                    //Currently no conversion path for parameters
                    if (jobJson["parameters"] != null)
                    {
                        job.parameters = new Dictionary<string, string>();
                    }
                    //Currently no conversion path for container
                    if (jobJson["container"] != null)
                    {
                        job.container = new Containers();
                    }
                    //Currently no conversion path for cancelTimeoutInMinutes
                    if (jobJson["cancelTimeoutInMinutes"] != null)
                    {
                        job.cancelTimeoutInMinutes = int.Parse(jobJson["cancelTimeoutInMinutes"].ToString());
                    }
                    if (jobJson["steps"] != null)
                    {
                        try
                        {
                            job.steps = YamlSerialization.DeserializeYaml<AzurePipelines.Step[]>(jobJson["steps"].ToString());
                        }
                        catch (Exception ex)
                        {
                            ConversionUtility.WriteLine($"DeserializeYaml<AzurePipelines.Step[]>(jobJson[\"steps\"].ToString() swallowed an exception: " + ex.Message, _verbose);
                        }
                    }
                    jobs[i] = job;
                    i++;
                }
            }

            return jobs;
        }

        public AzurePipelines.Job[] ProcessJobFromPipelineRootV2(string poolYaml, JToken strategyJson, string stepsYaml)
        {
            //Pool
            Pool pool = null;
            if (poolYaml != null)
            {
                GeneralProcessing gp = new GeneralProcessing(_verbose);
                pool = gp.ProcessPoolV2(poolYaml);
            }

            //Strategy
            StrategyProcessing sp = new StrategyProcessing(_verbose);
            AzurePipelines.Strategy strategy = sp.ProcessStrategyV2(strategyJson);

            //Steps
            AzurePipelines.Step[] steps = null;
            if (stepsYaml != null)
            {
                try
                {
                    steps = YamlSerialization.DeserializeYaml<AzurePipelines.Step[]>(stepsYaml);
                }
                catch (Exception ex)
                {
                    ConversionUtility.WriteLine($"DeserializeYaml<AzurePipelines.Step[]>(stepsYaml) swallowed an exception: " + ex.Message, _verbose);
                }
            }

            AzurePipelines.Job job = new AzurePipelines.Job
            {
                pool = pool,
                strategy = strategy,
                steps = steps
            };
            //Don't add the build name unless there is content
            if (job.pool != null || job.strategy != null || steps != null)
            {
                AzurePipelines.Job[] jobs = new AzurePipelines.Job[1];
                job.job = "build";
                jobs[0] = job;
                return jobs;
            }
            else
            {
                return null;
            }
        }

        private GitHubActions.Environment ProcessJobEnvironment(AzurePipelines.Environment environment)
        {
            if (environment != null)
            {
                GitHubActions.Environment newEnvironment = new GitHubActions.Environment
                {
                    name = environment.name
                };
                return newEnvironment;
            }
            else
            {
                return null;
            }
        }

    }
}
