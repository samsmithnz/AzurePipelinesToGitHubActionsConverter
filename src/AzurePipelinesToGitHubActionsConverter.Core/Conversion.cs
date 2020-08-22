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
        /// New structure:
        /// 1. get the yaml
        /// 2. break it into yaml pieces
        /// 3. deserialize each yaml piece into a azure pipelines sub-object
        /// 4. put it together into one azure pipelines object
        /// 5. convert the azure pipelines object to github action
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public ConversionResponse ConvertAzurePipelineToGitHubActionV2(string input)
        {
            List<string> variableList = new List<string>();
            string yaml = null;
            List<string> stepComments = new List<string>();
            string processedInput = ConversionUtility.RemoveCommentsFromYaml(input);

            //Pre-processing. These are exceptions and must be dealt with correctly
            input = ConversionUtility.ProcessNoneElement(input, "trigger:none");
            input = ConversionUtility.ProcessNoneElement(input, "pr:none");

            Dictionary<string, string> yamlElements = GetYamlElements(input);
            if (yamlElements != null)
            {
                GitHubActionsRoot gitHubActions = new GitHubActionsRoot();
                GeneralProcessing gp = new GeneralProcessing(_verbose);

                //Name
                yamlElements.TryGetValue("name", out string nameYaml);
                gitHubActions.name = gp.ProcessNameV2(nameYaml);

                //Trigger/PR/Schedules
                yamlElements.TryGetValue("trigger", out string triggerYaml);
                yamlElements.TryGetValue("pr", out string prYaml);
                yamlElements.TryGetValue("schedules", out string schedulesYaml);
                //Refactor to do them seperately
                //if (gitHubActions.on == null)
                //{
                //    gitHubActions.on = new GitHubActions.Trigger();
                //}
                //gitHubActions.on.schedule = schedules;
                gitHubActions.on = gp.ProcessTriggerPRAndSchedulesV2(triggerYaml, prYaml, schedulesYaml);

                //Pool
                //yamlElements.TryGetValue("pool", out string poolYaml);

                //Variables
                yamlElements.TryGetValue("parameters", out string parametersYaml);
                yamlElements.TryGetValue("variables", out string variablesYaml);
                gitHubActions.env = gp.ProcessParametersAndVariablesV2(parametersYaml, variablesYaml);

                //No Jobs/Jobs/Stages
                yamlElements.TryGetValue("stages", out string stagesYaml);
                yamlElements.TryGetValue("jobs", out string jobsYaml);
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
                variableList.AddRange(ConversionUtility.SearchForVariables(processedInput));

                //Create the YAML and apply some adjustments
                if (gitHubActions != null)
                {
                    yaml = GitHubActionsSerialization.Serialize(gitHubActions, variableList, _matrixVariableName);
                }
                else
                {
                    yaml = "";
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
                    yaml = item + System.Environment.NewLine + yaml;
                }
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

            //Create the YAML and apply some adjustments
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

                //Create the YAML and apply some adjustments
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

                //Create the YAML and apply some adjustments
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


        //Wait.
        //Loop through the text, line by line, looking for keywords. 
        //if a keyword is found, create a key value pair with the string and the name

        /// <summary>
        /// Returns a keyvaluepair of the element name, and it's child elements. 
        /// For example, a "trigger:\n- master", will be processed as a keyvault pair: <"trigger", "trigger\n:- master">
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Dictionary<string, string></returns>
        public Dictionary<string, string> GetYamlElements(string input)
        {
            if (input == null)
            {
                return null;
            }
            Dictionary<string, string> yamlElements = new Dictionary<string, string>();
            string yamlElementName = "";
            StringBuilder yamlElementContent = new StringBuilder();
            foreach (string line in input.Split(System.Environment.NewLine))
            {
                //If our line contains a keyword, we need to create a new keyvalue pair for it
                if (ContainsRootKeyword(line) == true && ConversionUtility.CountSpacesBeforeText(line) == 0)
                {
                    if (string.IsNullOrWhiteSpace(yamlElementContent.ToString().Trim()) == false)
                    {
                        //Add a new yaml element to the dictonary
                        yamlElements.Add(yamlElementName, yamlElementContent.ToString());
                    }
                    //start a collection for our new keyword
                    yamlElementContent = new StringBuilder();
                    yamlElementName = line.ToLower().Split(":")[0].Trim(); //Remove : and whitespace
                    yamlElementContent.Append(line);
                    yamlElementContent.Append(System.Environment.NewLine);
                }
                else
                {
                    yamlElementContent.Append(line);
                    yamlElementContent.Append(System.Environment.NewLine);
                }
            }
            //Add a new yaml element to the dictonary
            yamlElements.Add(yamlElementName, yamlElementContent.ToString());

            //string result = yamlItems.FirstOrDefault().Value.ToString();
            return yamlElements;
        }

        public string ProcessYAMLTest(string input)
        {
            Dictionary<string, string> yamlItems = GetYamlElements(input);
            string result = yamlItems.FirstOrDefault().Value.ToString();
            return result;
        }

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

        private bool ContainsRootKeyword(string input)
        {
            //Use reflection to loop through all of the properties, looking to see if we are using that property
            AzurePipelinesRoot<string, string> root = new AzurePipelinesRoot<string, string>();
            foreach (var prop in root.GetType().GetProperties())
            {
                Debug.WriteLine(prop.Name);
                if (input.ToLower().IndexOf(prop.Name + ":", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    input.ToLower().IndexOf("-") < 0)
                {
                    return true;
                }
            }
            return false;
            //if (input.ToLower().IndexOf("name:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("parameters:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("container:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("resources:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("trigger:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("pr:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("schedules:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("pool:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("strategy:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("variables:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("stages:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("jobs:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("steps:", StringComparison.OrdinalIgnoreCase) >= 0 ||
            //input.ToLower().IndexOf("services:", StringComparison.OrdinalIgnoreCase) >= 0)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

        }
    }
}
