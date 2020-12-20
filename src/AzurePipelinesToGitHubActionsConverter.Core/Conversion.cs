﻿using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class Conversion
    {
        private string _matrixVariableName;
        private readonly bool _verbose;

        public Conversion(bool verbose = true)
        {
            _verbose = verbose;
        }

        /// <summary>
        /// Convert an entire Azure DevOps Pipeline to a GitHub Actions 
        /// </summary>
        /// <param name="yaml">Yaml to convert</param>
        /// <returns>Converion object, with original yaml, processed yaml, comments on the conversion, and which conversion method was used</returns>
        public ConversionResponse ConvertAzurePipelineToGitHubAction(string yaml)
        {
            return ConvertAzurePipelineToGitHubActionV2(yaml);
        }

        /// <summary>
        /// V2 plan:
        /// 1. get the yaml
        /// 2. converting the yaml into a json document
        /// 3. parse the json document into azure pipelines sub-objects
        /// 4. put it together into one azure pipelines object
        /// 5. convert the azure pipelines object to github action
        /// </summary>
        /// <param name="yaml"></param>
        /// <returns></returns>

        private ConversionResponse ConvertAzurePipelineToGitHubActionV2(string yaml)
        {
            string gitHubYaml = "";
            List<string> variableList = new List<string>();
            List<string> stepComments = new List<string>();

            //convert the yaml into json, it's easier to parse
            JObject json = null;
            if (yaml != null)
            {
                //Clean up the YAML to remove conditional insert statements
                string processedYaml = ConversionUtility.CleanYamlBeforeDeserializationV2(yaml);
                json = JsonSerialization.DeserializeStringToObject(processedYaml);
            }

            //Build up the GitHub object piece by piece
            GitHubActionsRoot gitHubActions = new GitHubActionsRoot();
            GeneralProcessing gp = new GeneralProcessing(_verbose);

            if (json != null)
            {
                //Name
                if (json["name"] != null)
                {
                    string nameYaml = json["name"].ToString();
                    gitHubActions.name = gp.ProcessNameV2(nameYaml);
                }

                //Trigger/PR/Schedules
                TriggerProcessing tp = new TriggerProcessing(_verbose);
                if (json["trigger"] != null)
                {
                    string triggerYaml = json["trigger"].ToString();
                    triggerYaml = ConversionUtility.ProcessNoneJsonElement(triggerYaml);
                    gitHubActions.on = tp.ProcessTriggerV2(triggerYaml);
                }
                if (json["pr"] != null)
                {
                    string prYaml = json["pr"].ToString();
                    prYaml = ConversionUtility.ProcessNoneJsonElement(prYaml);
                    GitHubActions.Trigger prTrigger = tp.ProcessPullRequestV2(prYaml);
                    if (gitHubActions.on == null)
                    {
                        gitHubActions.on = prTrigger;
                    }
                    else
                    {
                        gitHubActions.on.pull_request = prTrigger.pull_request;
                    }
                }
                if (json["schedules"] != null)
                {
                    string schedulesYaml = json["schedules"].ToString();
                    GitHubActions.Trigger schedules = tp.ProcessSchedulesV2(schedulesYaml);
                    if (gitHubActions.on == null)
                    {
                        gitHubActions.on = schedules;
                    }
                    else
                    {
                        gitHubActions.on.schedule = schedules.schedule;
                    }
                }

                //Parameters & Variables
                string parametersYaml = json["parameters"]?.ToString();
                string variablesYaml = json["variables"]?.ToString();
                VariablesProcessing vp = new VariablesProcessing(_verbose);
                gitHubActions.env = vp.ProcessParametersAndVariablesV2(parametersYaml, variablesYaml);

                //Resources
                string resourcesYaml = json["resources"]?.ToString();
                //Resource Pipelines
                if (resourcesYaml?.IndexOf("\"pipelines\"") >= 0)
                {
                    gitHubActions.messages.Add("TODO: Resource pipelines conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/8");
                }
                //Resource Repositories
                if (resourcesYaml?.IndexOf("\"repositories\"") >= 0)
                {
                    gitHubActions.messages.Add("TODO: Resource repositories conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/8");
                }
                //Resource Container
                if (resourcesYaml?.IndexOf("\"containers\"") >= 0)
                {
                    gitHubActions.messages.Add("TODO: Container conversion not yet done, we need help!: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39");
                }
                //Strategy
                string strategyYaml = json["strategy"]?.ToString();


                //If we have stages, convert them into jobs first:
                if (json["stages"] != null)
                {
                    StagesProcessing sp = new StagesProcessing(_verbose);
                    gitHubActions.jobs = sp.ProcessStagesV2(json["stages"], strategyYaml);
                }
                //If we don't have stages, but have jobs:
                else if (json["stages"] == null && json["jobs"] != null)
                {
                    JobProcessing jp = new JobProcessing(_verbose);
                    gitHubActions.jobs = jp.ProcessJobsV2(jp.ExtractAzurePipelinesJobsV2(json["jobs"], strategyYaml), gp.ExtractResourcesV2(resourcesYaml));
                    _matrixVariableName = jp.MatrixVariableName;
                }
                //Otherwise, if we don't have stages or jobs, we just have steps, and need to load them into a new job
                else if (json["stages"] == null && json["jobs"] == null)
                {
                    //Pool
                    string poolYaml = json["pool"]?.ToString();
                    //pool/demands
                    if (poolYaml?.IndexOf("\"demands\":") >= 0)
                    {
                        gitHubActions.messages.Add("Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet");
                    }

                    //Steps
                    string stepsYaml = json["steps"]?.ToString();
                    JobProcessing jp = new JobProcessing(_verbose);
                    AzurePipelines.Job[] pipelineJobs = jp.ProcessJobFromPipelineRootV2(poolYaml, strategyYaml, stepsYaml);
                    Resources resources = gp.ExtractResourcesV2(resourcesYaml);
                    gitHubActions.jobs = jp.ProcessJobsV2(pipelineJobs, resources);
                    _matrixVariableName = jp.MatrixVariableName;
                }

                if (gitHubActions.jobs != null && gitHubActions.jobs.Count == 0)
                {
                    gitHubActions.messages.Add("Note that although having no jobs is valid YAML, it is not a valid GitHub Action.");
                }

                //Load in all variables. Duplicates are ok, they are processed the same
                variableList.AddRange(vp.SearchForVariables(yaml));
                variableList.AddRange(vp.SearchForVariablesV2(gitHubActions));

                //Create the GitHub YAML and apply some adjustments
                gitHubYaml = GitHubActionsSerialization.Serialize(gitHubActions, variableList, _matrixVariableName);


                //Load failed task comments for processing
                //Add any header messages
                foreach (string message in gitHubActions.messages)
                {
                    stepComments.Add(ConversionUtility.ConvertMessageToYamlComment(message));
                }
                if (gitHubActions.jobs != null)
                {
                    //Add each individual step comments
                    foreach (KeyValuePair<string, GitHubActions.Job> job in gitHubActions.jobs)
                    {
                        if (job.Value.steps != null)
                        {
                            if (job.Value.job_message != null)
                            {
                                stepComments.Add(ConversionUtility.ConvertMessageToYamlComment(job.Value.job_message));
                            }
                            foreach (GitHubActions.Step step in job.Value.steps)
                            {
                                if (step != null && string.IsNullOrEmpty(step.step_message) == false)
                                {
                                    stepComments.Add(ConversionUtility.ConvertMessageToYamlComment(step.step_message));
                                }
                            }
                        }
                    }
                }

            }

            //Append all of the comments to the top of the file
            foreach (string item in stepComments)
            {
                gitHubYaml = item + System.Environment.NewLine + gitHubYaml;
            }

            //Return the final conversion result, with the original (pipeline) yaml, processed (actions) yaml, and any comments
            return new ConversionResponse
            {
                pipelinesYaml = yaml,
                actionsYaml = gitHubYaml,
                comments = stepComments
            };
        }

        /// <summary>
        /// Convert a single Azure DevOps Pipeline task to a GitHub Actions task
        /// </summary>
        /// <param name="input">Yaml to convert</param>
        /// <returns>Converion object, with original yaml, processed yaml, and comments on the conversion</returns>
        public ConversionResponse ConvertAzurePipelineTaskToGitHubActionTask(string input)
        {
            string yaml = "";
            string processedInput = ConversionUtility.StepsPreProcessing(input);
            GitHubActions.Step gitHubActionStep = new GitHubActions.Step();

            //Process the YAML for the individual job
            AzurePipelines.Job azurePipelinesJob = YamlSerialization.DeserializeYaml<AzurePipelines.Job>(processedInput);
            if (azurePipelinesJob != null && azurePipelinesJob.steps != null && azurePipelinesJob.steps.Length > 0)
            {
                //As we needed to create an entire (but minimal) pipelines job, we need to now extract the step for processing
                StepsProcessing stepsProcessing = new StepsProcessing();
                gitHubActionStep = stepsProcessing.ProcessStep(azurePipelinesJob.steps[0]);

                //Find all variables in this text block, we need this for a bit later
                VariablesProcessing vp = new VariablesProcessing(_verbose);
                List<string> variableList = vp.SearchForVariables(processedInput);

                //Create the GitHub YAML and apply some adjustments
                if (gitHubActionStep != null)
                {
                    //add the step into a github job so it renders correctly
                    GitHubActions.Job gitHubJob = new GitHubActions.Job
                    {
                        steps = new GitHubActions.Step[1] //create an array of size 1
                    };
                    //Load the step into the single item array
                    gitHubJob.steps[0] = gitHubActionStep;

                    //Finally, we can serialize the job back to yaml
                    yaml = GitHubActionsSerialization.SerializeJob(gitHubJob, variableList);
                }
            }

            //Load failed tasks and comments for processing
            List<string> allComments = new List<string>();
            if (gitHubActionStep != null)
            {
                allComments.Add(gitHubActionStep.step_message);
            }

            //Return the final conversion result, with the original (pipeline) yaml, processed (actions) yaml, and any comments
            return new ConversionResponse
            {
                pipelinesYaml = input,
                actionsYaml = yaml,
                comments = allComments
            };
        }

    }
}
