using AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
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
            ConversionResponse conversionResponse = null;

            try
            {
                conversionResponse = ConvertAzurePipelineToGitHubActionV2(yaml);
            }
            catch (Exception ex2)
            {
                Debug.WriteLine("Conversion V2 failed. Trying V1: " + ex2.Message);
                try
                {
                    conversionResponse = ConvertAzurePipelineToGitHubActionV1(yaml);
                }
                catch (Exception ex1)
                {
                    Debug.WriteLine("Conversion V1 failed. Trying V2: " + ex1.Message);
                }
            }

            return conversionResponse;
        }

        /// <summary>
        /// V1 plan:
        /// 1. get the yaml
        /// 2. try to deserialize the entire doc on a few common combinations
        /// 3. convert the azure pipelines objects into a github action
        /// </summary>
        /// <param name="yaml"></param>
        /// <returns></returns>
        private ConversionResponse ConvertAzurePipelineToGitHubActionV1(string yaml)
        {
            string gitHubYaml;
            List<string> variableList = new List<string>();
            List<string> stepComments = new List<string>();
            GitHubActionsRoot gitHubActions = null;

            //Run some processing to convert simple pools and demands to the complex editions, to avoid adding to the combinations below.
            //Also clean and remove variables with reserved words that get into trouble during deserialization. HACK alert... :(
            string processedInput = ConversionUtility.CleanYamlBeforeDeserialization(yaml);

            //Start the main deserialization methods
            bool success = false;
            if (success == false)
            {
                var azurePipelineWithSimpleTriggerAndSimpleVariables = AzurePipelinesSerialization<string[], Dictionary<string, string>>.DeserializeSimpleTriggerAndSimpleVariables(processedInput);
                if (azurePipelineWithSimpleTriggerAndSimpleVariables != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<string[], Dictionary<string, string>>(_verbose);
                    gitHubActions = processing.ProcessPipeline(azurePipelineWithSimpleTriggerAndSimpleVariables, azurePipelineWithSimpleTriggerAndSimpleVariables.trigger, null, azurePipelineWithSimpleTriggerAndSimpleVariables.variables, null);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }

            if (success == false)
            {
                var azurePipelineWithSimpleTriggerAndComplexVariables = AzurePipelinesSerialization<string[], AzurePipelines.Variable[]>.DeserializeSimpleTriggerAndComplexVariables(processedInput);
                if (azurePipelineWithSimpleTriggerAndComplexVariables != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<string[], AzurePipelines.Variable[]>(_verbose);
                    gitHubActions = processing.ProcessPipeline(azurePipelineWithSimpleTriggerAndComplexVariables, azurePipelineWithSimpleTriggerAndComplexVariables.trigger, null, null, azurePipelineWithSimpleTriggerAndComplexVariables.variables);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }

            if (success == false)
            {
                var azurePipelineWithComplexTriggerAndSimpleVariables = AzurePipelinesSerialization<AzurePipelines.Trigger, Dictionary<string, string>>.DeserializeComplexTriggerAndSimpleVariables(processedInput);
                if (azurePipelineWithComplexTriggerAndSimpleVariables != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<AzurePipelines.Trigger, Dictionary<string, string>>(_verbose);
                    gitHubActions = processing.ProcessPipeline(azurePipelineWithComplexTriggerAndSimpleVariables, null, azurePipelineWithComplexTriggerAndSimpleVariables.trigger, azurePipelineWithComplexTriggerAndSimpleVariables.variables, null);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }

            if (success == false)
            {
                var azurePipelineWithComplexTriggerAndComplexVariables = AzurePipelinesSerialization<AzurePipelines.Trigger, AzurePipelines.Variable[]>.DeserializeComplexTriggerAndComplexVariables(processedInput);
                if (azurePipelineWithComplexTriggerAndComplexVariables != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<AzurePipelines.Trigger, AzurePipelines.Variable[]>(_verbose);
                    gitHubActions = processing.ProcessPipeline(azurePipelineWithComplexTriggerAndComplexVariables, null, azurePipelineWithComplexTriggerAndComplexVariables.trigger, null, azurePipelineWithComplexTriggerAndComplexVariables.variables);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }
            if (success == false && string.IsNullOrEmpty(processedInput?.Trim()) == false)
            {
                throw new NotSupportedException("All deserialisation methods failed... oops! Please create a GitHub issue so we can fix this");
            }

            //Search for any other variables. Duplicates are ok, they are processed the same
            variableList.AddRange(ConversionUtility.SearchForVariables(processedInput));

            //Create the GitHub YAML and apply some adjustments
            if (gitHubActions != null)
            {
                gitHubYaml = GitHubActionsSerialization.Serialize(gitHubActions, variableList, _matrixVariableName);
            }
            else
            {
                gitHubYaml = "";
            }

            //Load failed task comments for processing
            if (gitHubActions != null)
            {
                //Add any header messages
                if (gitHubActions.messages != null)
                {
                    foreach (string message in gitHubActions.messages)
                    {
                        stepComments.Add(ConversionUtility.ConvertMessageToYamlComment(message));
                    }
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
                comments = stepComments,
                v2ConversionSuccessful = false
            };
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
                string processedYaml = ConversionUtility.CleanYamlBeforeDeserializationV2(yaml);
                json = JSONSerialization.DeserializeStringToObject(processedYaml);
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
                if (json["trigger"] != null)
                {
                    string triggerYaml = json["trigger"].ToString();
                    triggerYaml = ConversionUtility.ProcessNoneJsonElement(triggerYaml);
                    gitHubActions.on = gp.ProcessTriggerV2(triggerYaml);
                }
                if (json["pr"] != null)
                {
                    string prYaml = json["pr"].ToString();
                    prYaml = ConversionUtility.ProcessNoneJsonElement(prYaml);
                    GitHubActions.Trigger prTrigger = gp.ProcessPullRequestV2(prYaml);
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
                    GitHubActions.Trigger schedules = gp.ProcessSchedulesV2(schedulesYaml);
                    if (gitHubActions.on == null)
                    {
                        gitHubActions.on = schedules;
                    }
                    else
                    {
                        gitHubActions.on.schedule = schedules.schedule;
                    }
                }

                //Parameters
                string parametersYaml = null;
                if (json["parameters"] != null)
                {
                    parametersYaml = json["parameters"].ToString();
                }
                //Variables
                string variablesYaml = null;
                if (json["variables"] != null)
                {
                    variablesYaml = json["variables"].ToString();
                }
                gitHubActions.env = gp.ProcessParametersAndVariablesV2(parametersYaml, variablesYaml);

                string resourcesYaml = null;
                if (json["resources"] != null)
                {
                    resourcesYaml = json["resources"].ToString();
                    if (json["resources"]["pipelines"] != null)
                    {
                        gitHubActions.messages.Add("TODO: Resource pipelines conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/8");
                    }
                    if (json["resources"]["repositories"] != null)
                    {
                        gitHubActions.messages.Add("TODO: Resource repositories conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/8");
                    }
                    //Container
                    if (json["resources"]["containers"] != null)
                    {
                        gitHubActions.messages.Add("TODO: Container conversion not yet done, we need help - our container skills are woeful: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39");
                    }
                }
                //Strategy
                string strategyYaml = null;
                if (json["strategy"] != null)
                {
                    strategyYaml = json["strategy"].ToString();
                }

                //We have stages
                if (json["stages"] != null)
                {
                    gitHubActions.jobs = gp.ProcessStagesV2(json["stages"], strategyYaml);
                }
                //We just have jobs
                else if (json["stages"] == null && json["jobs"] != null)
                {
                    gitHubActions.jobs = gp.ProcessJobsV2(gp.ExtractAzurePipelinesJobsV2(json["jobs"], strategyYaml), gp.ExtractResourcesV2(resourcesYaml));
                }
                //We just have steps, and need to load them into a jobS
                else if (json["stages"] == null && json["jobs"] == null)
                {
                    //Pool
                    string poolYaml = null;
                    if (json["pool"] != null)
                    {
                        poolYaml = json["pool"].ToString();
                        //pool/demands
                        if (poolYaml.IndexOf("\"demands\":") >= 0)
                        {
                            gitHubActions.messages.Add("Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet");
                        }
                    }
                    //Steps
                    string stepsYaml = null;
                    if (json["steps"] != null)
                    {
                        stepsYaml = json["steps"].ToString();
                    }
                    AzurePipelines.Job[] pipelineJobs = gp.ProcessJobFromPipelineRootV2(poolYaml, strategyYaml, stepsYaml);
                    gitHubActions.jobs = gp.ProcessJobsV2(pipelineJobs, gp.ExtractResourcesV2(resourcesYaml));
                }
                _matrixVariableName = gp.MatrixVariableName;
                if (gitHubActions.jobs != null && gitHubActions.jobs.Count == 0)
                {
                    gitHubActions.messages.Add("Note that although having no jobs is valid YAML, it is not a valid GitHub Action.");
                }

                //Load in all variables. Duplicates are ok, they are processed the same
                variableList.AddRange(ConversionUtility.SearchForVariables(yaml));
                variableList.AddRange(gp.SearchForVariablesV2(gitHubActions));

                //Create the GitHub YAML and apply some adjustments
                if (gitHubActions != null)
                {
                    gitHubYaml = GitHubActionsSerialization.Serialize(gitHubActions, variableList, _matrixVariableName);
                }
                else
                {
                    gitHubYaml = "";
                }

                //Load failed task comments for processing
                if (gitHubActions != null)
                {
                    //Add any header messages
                    if (gitHubActions.messages != null)
                    {
                        foreach (string message in gitHubActions.messages)
                        {
                            stepComments.Add(ConversionUtility.ConvertMessageToYamlComment(message));
                        }
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
                comments = stepComments,
                v2ConversionSuccessful = true
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
            AzurePipelines.Job azurePipelinesJob = GenericObjectSerialization.DeserializeYaml<AzurePipelines.Job>(processedInput);
            if (azurePipelinesJob != null && azurePipelinesJob.steps != null && azurePipelinesJob.steps.Length > 0)
            {
                //As we needed to create an entire (but minimal) pipelines job, we need to now extract the step for processing
                StepsProcessing stepsProcessing = new StepsProcessing();
                gitHubActionStep = stepsProcessing.ProcessStep(azurePipelinesJob.steps[0]);

                //Find all variables in this text block, we need this for a bit later
                List<string> variableList = ConversionUtility.SearchForVariables(processedInput);

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
