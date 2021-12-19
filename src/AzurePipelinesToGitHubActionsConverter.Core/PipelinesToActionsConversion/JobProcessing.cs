using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GitHubActions = GitHubActionsDotNet.Models;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public class JobProcessing
    {
        public List<string> VariableList { get; set; }
        public string MatrixVariableName { get; set; }
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

            if (newJob.steps == null)
            {
                if (job.template != null)
                {
                    //Initialize the array with no items
                    job.steps = new AzurePipelines.Step[0];
                    //Process the steps, adding the default checkout step
                    newJob.steps = sp.AddSupportingSteps(job.steps, true);
                    //TODO: There is currently no conversion path for templates
                    newJob.job_message += "Note: Azure DevOps template does not have an equivalent in GitHub Actions yet";
                }
                else if (job.strategy?.runOnce?.deploy?.steps != null)
                {
                    //Initialize the array with no items
                    job.steps = new AzurePipelines.Step[0];
                    //Process the steps, adding the default checkout step
                    newJob.steps = sp.AddSupportingSteps(job.strategy.runOnce.deploy.steps, false);
                    //TODO: There is currently no conversion path for runOnce strategy
                    newJob.job_message += "Note: Azure DevOps strategy>runOnce does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps";
                }
                else if (job.strategy?.canary?.deploy?.steps != null)
                {
                    //Initialize the array with no items
                    job.steps = new AzurePipelines.Step[0];
                    //Process the steps, adding the default checkout step
                    newJob.steps = sp.AddSupportingSteps(job.strategy.canary.deploy.steps, false);
                    //TODO: There is currently no conversion path for canary strategy
                    newJob.job_message += "Note: Azure DevOps strategy>canary does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps";
                }
                else if (job.strategy?.rolling?.deploy?.steps != null)
                {
                    //Initialize the array with no items
                    job.steps = new AzurePipelines.Step[0];
                    //Process the steps, adding the default checkout step
                    newJob.steps = sp.AddSupportingSteps(job.strategy.rolling.deploy.steps, false);
                    //TODO: There is currently no conversion path for rolling strategy
                    newJob.job_message += "Note: Azure DevOps strategy>rolling does not have an equivalent in GitHub Actions yet, and only the deploy steps are transferred to steps";
                }
            }

            //Process the steps to ensure identical tasks haven't been added twice. (e.g. the checkout task)
            newJob.steps = RemoveDuplicateSteps(newJob.steps);

            //Add this pools in for other strategies  
            if (job.strategy?.runOnce?.deploy?.pool != null)
            {
                newJob.runs_on = generalProcessing.ProcessPool(job.strategy.runOnce.deploy.pool);
            }
            else if (job.strategy?.canary?.deploy?.pool != null)
            {
                newJob.runs_on = generalProcessing.ProcessPool(job.strategy.canary.deploy.pool);
            }
            else if (job.strategy?.rolling?.deploy?.pool != null)
            {
                newJob.runs_on = generalProcessing.ProcessPool(job.strategy.rolling.deploy.pool);
            }
            if (job.continueOnError)
            {
                newJob.continue_on_error = job.continueOnError;
            }

            return newJob;
        }

        private GitHubActions.Step[] RemoveDuplicateSteps(GitHubActions.Step[] steps)
        {
            if (steps != null)
            {
                GitHubActions.Step previousStep = null;
                List<int> indexesToRemove = new List<int>();
                //Look through all of the current items to see if a current step matches a previous step
                for (int i = 0; i < steps.Length; i++)
                {
                    if (previousStep == null)
                    {
                        previousStep = steps[i];
                    }
                    else if (JsonSerialization.JsonCompare(previousStep, steps[i]))
                    {
                        //mark the current step as one to remove
                        indexesToRemove.Add(i);
                    }
                }
                //Then insert all of the steps that don't need to be removed into the new array
                GitHubActions.Step[] newSteps = new GitHubActions.Step[steps.Length - indexesToRemove.Count];
                int index = 0;
                for (int i = 0; i < steps.Length; i++)
                {
                    if (!indexesToRemove.Contains(i))
                    {
                        newSteps[index] = steps[i];
                        index++;
                    }
                }
                return newSteps;
            }
            else
            {
                return steps; //effectively return null
            }
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

        public AzurePipelines.Job[] ExtractAzurePipelinesJobsV2(JsonElement jobsJson, JsonElement strategyJson)
        {
            GeneralProcessing gp = new GeneralProcessing(_verbose);
            StrategyProcessing sp = new StrategyProcessing(_verbose);
            AzurePipelines.Job[] jobs = new AzurePipelines.Job[jobsJson.EnumerateArray().Count()];
            if (jobsJson.ValueKind != JsonValueKind.Undefined)
            {
                int i = 0;
                foreach (JsonElement jobJson in jobsJson.EnumerateArray())
                {
                    JsonElement jsonElement = new JsonElement();
                    AzurePipelines.Job job = new AzurePipelines.Job();
                    if (jobJson.TryGetProperty("job", out jsonElement))
                    {
                        job.job = jsonElement.ToString();
                    }
                    if (jobJson.TryGetProperty("deployment", out jsonElement))
                    {
                        job.deployment = jsonElement.ToString();
                    }
                    if (jobJson.TryGetProperty("displayName", out jsonElement))
                    {
                        job.displayName = jsonElement.ToString();
                    }
                    if (jobJson.TryGetProperty("template", out jsonElement))
                    {
                        job.template = jsonElement.ToString();
                    }
                    //Pool
                    if (jobJson.TryGetProperty("pool", out jsonElement))
                    {
                        job.pool = gp.ProcessPoolV2(jsonElement.ToString());
                    }
                    //Strategy
                    if (jobJson.TryGetProperty("strategy", out jsonElement))
                    {
                        strategyJson = jsonElement;
                    }
                    if (strategyJson.ToString() != null)
                    {
                        job.strategy = sp.ProcessStrategyV2(strategyJson);
                    }

                    if (jobJson.TryGetProperty("dependsOn", out jsonElement))
                    {
                        job.dependsOn = gp.ProcessDependsOnV2(jsonElement.ToString());
                    }
                    if (jobJson.TryGetProperty("condition", out jsonElement))
                    {
                        job.condition = jsonElement.ToString();
                    }
                    if (jobJson.TryGetProperty("environment", out jsonElement))
                    {
                        job.environment = gp.ProcessEnvironmentV2(jsonElement.ToString());
                    }
                    if (jobJson.TryGetProperty("timeoutInMinutes", out jsonElement))
                    {
                        int.TryParse(jsonElement.ToString(), out int timeOut);
                        if (timeOut > 0)
                        {
                            job.timeoutInMinutes = timeOut;
                        }
                    }
                    if (jobJson.TryGetProperty("continueOnError", out jsonElement))
                    {
                        bool.TryParse(jsonElement.ToString(), out bool continueOnError);
                        job.continueOnError = continueOnError;
                    }
                    if (jobJson.TryGetProperty("variables", out jsonElement))
                    {
                        VariablesProcessing vp = new VariablesProcessing(_verbose);
                        job.variables = vp.ProcessParametersAndVariablesV2(null, jsonElement.ToString());
                    }
                    //Currently no conversion path for services
                    //if (jobJson["services"] != null)
                    //{
                    //    job.services = new Dictionary<string, string>();
                    //    ConversionUtility.WriteLine($"Currently no conversion path for services: " + job.services.ToString(), _verbose);
                    //}
                    //Currently no conversion path for parameters
                    if (jobJson.TryGetProperty("parameters", out jsonElement))
                    {
                        job.parameters = new Dictionary<string, string>();
                    }
                    //Currently no conversion path for container
                    //if (jobJson["container"] != null)
                    //{
                    //    job.container = new Containers();
                    //    ConversionUtility.WriteLine($"Currently no conversion path for services: " + job.container.ToString(), _verbose);
                    //}
                    ////Currently no conversion path for cancelTimeoutInMinutes
                    //if (jobJson["cancelTimeoutInMinutes"] != null)
                    //{
                    //    job.cancelTimeoutInMinutes = int.Parse(jobJson["cancelTimeoutInMinutes"].ToString());
                    //    ConversionUtility.WriteLine($"Currently no conversion path for services: " + job.cancelTimeoutInMinutes.ToString(), _verbose);
                    //}
                    if (jobJson.TryGetProperty("steps", out jsonElement))
                    {
                        try
                        {
                            job.steps = YamlSerialization.DeserializeYaml<AzurePipelines.Step[]>(jsonElement.ToString());
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

        public AzurePipelines.Job[] ProcessJobFromPipelineRootV2(string poolYaml, JsonElement strategyJson, string stepsYaml)
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
                    steps = new AzurePipelines.Step[1];
                    steps[0] = new Step
                    {
                        script = $"This step is unknown and caused an exception: " + ex.Message
                    };
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
