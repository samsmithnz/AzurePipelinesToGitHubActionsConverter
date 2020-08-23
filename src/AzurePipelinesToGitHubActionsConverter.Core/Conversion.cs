using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization;
using AzurePipelinesToGitHubActionsConverter.Core.Extensions;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using System.IO;
using Newtonsoft.Json.Linq;

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
        /// V3 plan:
        /// 1. get the yaml, converting it to a json document
        /// 2. break the json into pieces
        /// 3. deserialize each json piece into azure pipelines sub-objects
        /// 4. put it together into one azure pipelines object
        /// 5. convert the azure pipelines object to github action
        /// </summary>
        /// <param name="yaml"></param>
        /// <returns></returns>

        public ConversionResponse ConvertAzurePipelineToGitHubActionV3(string yaml)
        {
            string gitHubYaml = null;
            List<string> variableList = new List<string>();
            List<string> stepComments = new List<string>();

            //convert the yaml into json, it's easier to parse
            JObject json = null;
            if (yaml != null)
            {
                StringWriter sw = new StringWriter();
                StringReader sr = new StringReader(yaml);
                Deserializer deserializer = new Deserializer();
                var yamlObject = deserializer.Deserialize(sr);
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(sw, yamlObject);
                json = JsonConvert.DeserializeObject<JObject>(sw.ToString());
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
                    triggerYaml = ConversionUtility.ProcessNoneJsonElement(triggerYaml, "trigger:none");
                    gitHubActions.on = gp.ProcessTriggerV2(triggerYaml);
                }
                if (json["pr"] != null)
                {
                    string prYaml = json["pr"].ToString();
                    prYaml = ConversionUtility.ProcessNoneJsonElement(prYaml, "pr:none");
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
                
                //Create the GitHub YAML and apply some adjustments
                if (gitHubActions != null)
                {
                    gitHubYaml = GitHubActionsSerialization.Serialize(gitHubActions, variableList, _matrixVariableName);
                }
                else
                {
                    gitHubYaml = "";
                }
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
        /// V2 plan:
        /// 1. get the yaml
        /// 2. break it into yaml pieces
        /// 3. deserialize each yaml piece into azure pipelines sub-objects
        /// 4. put it together into one azure pipelines object
        /// 5. convert the azure pipelines object to github action
        /// </summary>
        /// <param name="yaml"></param>
        /// <returns></returns>
        public ConversionResponse ConvertAzurePipelineToGitHubActionV2(string yaml)
        {

            string gitHubYaml = null;
            List<string> variableList = new List<string>();
            List<string> stepComments = new List<string>();
            string processedYaml = ConversionUtility.RemoveCommentsFromYaml(yaml);
            processedYaml = processedYaml.Trim();

            //Pre-processing. These are exceptions and must be dealt with correctly
            yaml = ConversionUtility.ProcessNoneYamlElement(yaml, "trigger:none");
            yaml = ConversionUtility.ProcessNoneYamlElement(yaml, "pr:none");
            int spacesPrefix = ConversionUtility.CountSpacesBeforeText(yaml);

            //Get a list of all top level Yaml Elements
            List<KeyValuePair<string, string>> yamlElements = ConversionYamlParser.GetYamlElements(yaml, spacesPrefix, true, false);
            if (yamlElements != null)
            {
                GitHubActionsRoot gitHubActions = new GitHubActionsRoot();
                GeneralProcessing gp = new GeneralProcessing(_verbose);

                //Name
                string nameYaml = yamlElements.FirstOrDefault(c => c.Key == "name").Value;
                gitHubActions.name = gp.ProcessNameV2(nameYaml);

                //Trigger/PR/Schedules
                //Refactor to do them seperately
                //if (gitHubActions.on == null)
                //{
                //    gitHubActions.on = new GitHubActions.Trigger();
                //}
                //gitHubActions.on.schedule = schedules;
                string triggerYaml = yamlElements.FirstOrDefault(c => c.Key == "trigger").Value;
                string prYaml = yamlElements.FirstOrDefault(c => c.Key == "pr").Value;
                string schedulesYaml = yamlElements.FirstOrDefault(c => c.Key == "schedules").Value;
                gitHubActions.on = gp.ProcessTriggerPRAndSchedulesV2(triggerYaml, prYaml, schedulesYaml);

                //Pool
                //yamlElements.TryGetValue("pool", out string poolYaml);

                //Variables
                string parametersYaml = yamlElements.FirstOrDefault(c => c.Key == "parameters").Value;
                string variablesYaml = yamlElements.FirstOrDefault(c => c.Key == "variables").Value;
                gitHubActions.env = gp.ProcessParametersAndVariablesV2(parametersYaml, variablesYaml);

                //No Jobs/Jobs/Stages
                string stagesYaml = yamlElements.FirstOrDefault(c => c.Key == "stages").Value;
                string jobsYaml = yamlElements.FirstOrDefault(c => c.Key == "jobs").Value;
                if (stagesYaml != null)
                {
                    gitHubActions.jobs = gp.ProcessStagesV2(stagesYaml);
                }
                else if (jobsYaml != null)
                {
                    gitHubActions.jobs = gp.ProcessJobsV2(jobsYaml);
                }

                ////If there are no stages, or jobs, process the top level
                //string steps;
                //yamlElements.TryGetValue("steps", out steps);
                //if (jobs == null || jobs.Count == 0)
                //{
                //    gp.ProcessJob(pool, steps)
                //}

                //Search for any other variables. Duplicates are ok, they are processed the same
                variableList.AddRange(ConversionUtility.SearchForVariables(processedYaml));

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
        /// Convert an entire Azure DevOps Pipeline to a GitHub Actions 
        /// </summary>
        /// <param name="input">Yaml to convert</param>
        /// <returns>Converion object, with original yaml, processed yaml, and comments on the conversion</returns>
        public ConversionResponse ConvertAzurePipelineToGitHubAction(string input)
        {
            List<string> variableList = new List<string>();
            string yaml;
            GitHubActionsRoot gitHubActions = null;

            //Run some processing to convert simple pools and demands to the complex editions, to avoid adding to the combinations below.
            //Also clean and remove variables with reserved words that get into trouble during deserialization. HACK alert... :(
            string processedInput = ConversionUtility.CleanYamlBeforeDeserialization(input);

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
                yaml = GitHubActionsSerialization.Serialize(gitHubActions, variableList, _matrixVariableName);
            }
            else
            {
                yaml = "";
            }

            //Load failed task comments for processing
            List<string> stepComments = new List<string>();
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
                yaml = item + System.Environment.NewLine + yaml;
            }

            //Return the final conversion result, with the original (pipeline) yaml, processed (actions) yaml, and any comments
            return new ConversionResponse
            {
                pipelinesYaml = input,
                actionsYaml = yaml,
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public ConversionResponse ConvertAzurePipelineJobToGitHubActionJob(string input)
        {
            string yaml = "";
            string processedInput = ConversionUtility.JobsPreProcessing(input);
            //Run some processing to convert simple pools and demands to the complex editions, to avoid adding to the combinations below.
            //Also clean and remove variables with reserved words that get into trouble during deserialization. HACK alert... :(
            processedInput = ConversionUtility.CleanYamlBeforeDeserialization(processedInput);
            GitHubActions.Job gitHubActionJob;

            //Process the YAML for the individual job
            AzurePipelines.Job azurePipelinesJob = GenericObjectSerialization.DeserializeYaml<AzurePipelines.Job>(processedInput);
            if (azurePipelinesJob != null)
            {
                //As we needed to create an entire (but minimal) pipelines job, we need to now extract the step for processing
                JobProcessing jobProcessing = new JobProcessing(_verbose);
                gitHubActionJob = jobProcessing.ProcessJob(azurePipelinesJob, null);
                _matrixVariableName = jobProcessing.MatrixVariableName;

                //Find all variables in this text block, we need this for a bit later
                List<string> variableList = ConversionUtility.SearchForVariables(processedInput);

                //Create the GitHub YAML and apply some adjustments
                if (gitHubActionJob != null)
                {
                    //Finally, we can serialize the job back to yaml
                    yaml = GitHubActionsSerialization.SerializeJob(gitHubActionJob, variableList);
                }
            }


            List<string> allComments = new List<string>();

            return new ConversionResponse
            {
                pipelinesYaml = input,
                actionsYaml = yaml,
                comments = allComments
            };
        }


        //public string ProcessYAMLTest(string input)
        //{
        //    Dictionary<string, string> yamlItems = GetYamlElements(input);
        //    string result = yamlItems.FirstOrDefault().Value.ToString();
        //    return result;
        //}

        //public string ProcessTrigger(string input)
        //{
        //    string searchString = "trigger:";
        //    StringBuilder newYaml = new StringBuilder();
        //    if (input.ToLower().IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
        //    {
        //        //Convert a (really) simple string trigger to a string[] trigger
        //        input = ConversionUtility.ProcessAndCleanElement(input, "trigger:", "- ");
        //        //Convert a simple string[] trigger to a complex Trigger 
        //        string[] array = input.Split(System.Environment.NewLine);
        //        int lineStart = -1;
        //        int lineEnd = -1;
        //        List<string> branches = new List<string>();
        //        for (int i = 0; i < array.Length; i++)
        //        {
        //            string line = (string)array[i];
        //            //If the search string is found, start processing it
        //            if (line.ToLower().IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
        //            {
        //                lineStart = i + 1;
        //                newYaml.Append(line);
        //                newYaml.Append(System.Environment.NewLine);
        //            }
        //            else if (lineStart >= 0 && ContainsRootKeyword(line) == false)
        //            {
        //                if (line.Trim().StartsWith("-") == true)
        //                {
        //                    //Process a child trigger branch item
        //                    branches.Add(line);
        //                }
        //                else
        //                {
        //                    lineStart = -1;
        //                    newYaml.Append(line);
        //                    newYaml.Append(System.Environment.NewLine);
        //                }
        //            }
        //            else if (lineStart >= 0 && ContainsRootKeyword(line) == true)
        //            {
        //                //wrap it up
        //                lineEnd = i - 1;

        //                //Get the count of whitespaces in front of the variable
        //                int prefixSpaceCount = ConversionUtility.CountSpacesBeforeText(array[lineStart]);

        //                //start building the new string, with the white space count
        //                newYaml.Append(ConversionUtility.GenerateSpaces(prefixSpaceCount));
        //                //Add the main keyword
        //                newYaml.Append(array[lineStart].Trim());
        //                newYaml.Append(": ");
        //                newYaml.Append(System.Environment.NewLine);
        //                //on the new lines, recreate:
        //                //trigger:
        //                //  branches:
        //                //    include:
        //                //    - master
        //                newYaml.Append(ConversionUtility.GenerateSpaces(prefixSpaceCount + 2));
        //                newYaml.Append("branches:");
        //                newYaml.Append(System.Environment.NewLine);
        //                newYaml.Append(ConversionUtility.GenerateSpaces(prefixSpaceCount + 4));
        //                newYaml.Append("include:");
        //                newYaml.Append(System.Environment.NewLine);
        //                foreach (string branch in branches)
        //                {
        //                    //The branch values
        //                    newYaml.Append(ConversionUtility.GenerateSpaces(prefixSpaceCount + 6));
        //                    newYaml.Append("- ");
        //                    newYaml.Append(branch.Trim());
        //                    newYaml.Append(System.Environment.NewLine);
        //                }
        //            }
        //            else
        //            {
        //                newYaml.Append(line);
        //                newYaml.Append(System.Environment.NewLine);
        //            }
        //        }
        //    }
        //    return input;
        //}

    }
}
