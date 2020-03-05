using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class Conversion
    {
        private string _matrixVariableName;

        /// <summary>
        /// Convert a single Azure DevOps Pipeline task to a GitHub Actions task
        /// </summary>
        /// <param name="input">Yaml to convert</param>
        /// <returns>Converion object, with original yaml, processed yaml, and comments on the conversion</returns>
        public ConversionResult ConvertAzurePipelineTaskToGitHubActionTask(string input)
        {
            string yaml = "";
            string processedInput = StepsPreProcessing(input);
            GitHubActions.Step gitHubActionStep = new GitHubActions.Step();

            //Process the YAML for the individual job
            AzurePipelines.Job azurePipelinesJob = Global.DeserializeYaml<AzurePipelines.Job>(processedInput);
            if (azurePipelinesJob != null && azurePipelinesJob.steps != null && azurePipelinesJob.steps.Length > 0)
            {
                //As we needed to create an entire (but minimal) pipelines job, we need to now extract the step for processing
                AzurePipelinesStepsProcessing stepsProcessing = new AzurePipelinesStepsProcessing();
                gitHubActionStep = stepsProcessing.ProcessStep(azurePipelinesJob.steps[0]);

                //Find all variables in this text block, we need this for a bit later
                List<string> variableList = SearchForVariables(processedInput);

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
            return new ConversionResult
            {
                pipelinesYaml = input,
                actionsYaml = yaml,
                comments = allComments
            };

        }

        /// <summary>
        /// Convert an entire Azure DevOps Pipeline to a GitHub Actions 
        /// </summary>
        /// <param name="input">Yaml to convert</param>
        /// <returns>Converion object, with original yaml, processed yaml, and comments on the conversion</returns>
        public ConversionResult ConvertAzurePipelineToGitHubAction(string input)
        {
            List<string> variableList = new List<string>();
            string yaml;
            GitHubActionsRoot gitHubActions = null;

            //Triggers are hard, as there are two data types that can exist, so we need to go with the most common type and handle the less common type with generics
            AzurePipelinesRoot<string[], Dictionary<string, string>> azurePipelineWithSimpleTriggerAndSimpleVariables = null;
            AzurePipelinesRoot<string[], AzurePipelines.Variables[]> azurePipelineWithSimpleTriggerAndComplexVariables = null;
            AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>> azurePipelineWithComplexTriggerAndSimpleVariables = null;
            AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variables[]> azurePipelineWithComplexTriggerAndComplexVariables = null;
            try
            {
                azurePipelineWithSimpleTriggerAndSimpleVariables = AzurePipelinesSerialization<string[], Dictionary<string, string>>.DeserializeSimpleTriggerAndSimpleVariables(input);
            }
            catch
            {
                try
                {
                    azurePipelineWithComplexTriggerAndSimpleVariables = AzurePipelinesSerialization<AzurePipelines.Trigger, Dictionary<string, string>>.DeserializeComplexTriggerAndSimpleVariables(input);
                }
                catch
                {
                    try
                    {
                        azurePipelineWithSimpleTriggerAndComplexVariables = AzurePipelinesSerialization<string[], AzurePipelines.Variables[]>.DeserializeSimpleTriggerAndComplexVariables(input);
                    }
                    catch
                    {
                        azurePipelineWithComplexTriggerAndComplexVariables = AzurePipelinesSerialization<AzurePipelines.Trigger, AzurePipelines.Variables[]>.DeserializeComplexTriggerAndComplexVariables(input);
                    }
                }
            }

            //Generate the github actions
            if (azurePipelineWithSimpleTriggerAndSimpleVariables != null)
            {
                AzurePipelinesProcessing<string[], Dictionary<string, string>> processing = new AzurePipelinesProcessing<string[], Dictionary<string, string>>();
                gitHubActions = processing.ProcessPipeline(azurePipelineWithSimpleTriggerAndSimpleVariables, azurePipelineWithSimpleTriggerAndSimpleVariables.trigger, null, azurePipelineWithSimpleTriggerAndSimpleVariables.variables, null);

                if (processing.MatrixVariableName != null)
                {
                    _matrixVariableName = processing.MatrixVariableName;
                }
                variableList.AddRange(processing.VariableList);
            }
            else if (azurePipelineWithSimpleTriggerAndComplexVariables != null)
            {
                AzurePipelinesProcessing<string[], AzurePipelines.Variables[]> processing = new AzurePipelinesProcessing<string[], AzurePipelines.Variables[]>();
                gitHubActions = processing.ProcessPipeline(azurePipelineWithSimpleTriggerAndComplexVariables, azurePipelineWithSimpleTriggerAndComplexVariables.trigger, null, null, azurePipelineWithSimpleTriggerAndComplexVariables.variables);

                if (processing.MatrixVariableName != null)
                {
                    _matrixVariableName = processing.MatrixVariableName;
                }
                variableList.AddRange(processing.VariableList);
            }
            else if (azurePipelineWithComplexTriggerAndSimpleVariables != null)
            {
                AzurePipelinesProcessing<AzurePipelines.Trigger, Dictionary<string, string>> processing = new AzurePipelinesProcessing<AzurePipelines.Trigger, Dictionary<string, string>>();
                gitHubActions = processing.ProcessPipeline(azurePipelineWithComplexTriggerAndSimpleVariables, null, azurePipelineWithComplexTriggerAndSimpleVariables.trigger, azurePipelineWithComplexTriggerAndSimpleVariables.variables, null);

                if (processing.MatrixVariableName != null)
                {
                    _matrixVariableName = processing.MatrixVariableName;
                }
                variableList.AddRange(processing.VariableList);
            }
            else if (azurePipelineWithComplexTriggerAndComplexVariables != null)
            {
                AzurePipelinesProcessing<AzurePipelines.Trigger, AzurePipelines.Variables[]> processing = new AzurePipelinesProcessing<AzurePipelines.Trigger, AzurePipelines.Variables[]>();
                gitHubActions = processing.ProcessPipeline(azurePipelineWithComplexTriggerAndComplexVariables, null, azurePipelineWithComplexTriggerAndComplexVariables.trigger, null, azurePipelineWithComplexTriggerAndComplexVariables.variables);

                if (processing.MatrixVariableName != null)
                {
                    _matrixVariableName = processing.MatrixVariableName;
                }
                variableList.AddRange(processing.VariableList);
            }

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
            if (gitHubActions != null && gitHubActions.jobs != null)
            {
                //Add any header messages
                if (gitHubActions.messages != null)
                {
                    foreach (string message in gitHubActions.messages)
                    {
                        stepComments.Add(ConvertMessageToYamlComment(message));
                    }
                }
                //Add each individual step comments
                foreach (KeyValuePair<string, GitHubActions.Job> job in gitHubActions.jobs)
                {
                    if (job.Value.steps != null)
                    {
                        if (job.Value.job_message != null)
                        {
                            stepComments.Add(ConvertMessageToYamlComment(job.Value.job_message));
                        }
                        foreach (GitHubActions.Step step in job.Value.steps)
                        {
                            if (step != null && string.IsNullOrEmpty(step.step_message) == false)
                            {
                                stepComments.Add(ConvertMessageToYamlComment(step.step_message));
                            }
                        }
                    }
                }
            }

            //Append all of the comments to the top of the file
            foreach (string item in stepComments)
            {
                yaml = item + Environment.NewLine + yaml;
            }

            //Return the final conversion result, with the original (pipeline) yaml, processed (actions) yaml, and any comments
            return new ConversionResult
            {
                pipelinesYaml = input,
                actionsYaml = yaml,
                comments = stepComments
            };
        }

        private string ConvertMessageToYamlComment(string message)
        {
            //Append a comment to the message if one doesn't already exist
            if (message.TrimStart().StartsWith("#") == false)
            {
                message = "#" + message;
            }
            return message;
        }

        //Add a steps parent, to allow the processing of an individual step to proceed
        private string StepsPreProcessing(string input)
        {
            //If the step isn't wrapped in a "steps:" node, we need to add this, so we can process the step
            if (input.Trim().StartsWith("steps:") == false)
            {
                //we need to add steps, before we do, we need to see if the task needs an indent
                string[] stepLines = input.Split(Environment.NewLine);
                if (stepLines.Length > 0)
                {
                    int i = 0;
                    //Search for the first non empty line
                    while (string.IsNullOrEmpty(stepLines[i].Trim()) == true)
                    {
                        i++;
                    }
                    if (stepLines[i].Trim().StartsWith("-") == true)
                    {
                        int indentLevel = stepLines[i].IndexOf("-");
                        indentLevel += 2;
                        string buffer = Global.GenerateSpaces(indentLevel);
                        StringBuilder newInput = new StringBuilder();
                        foreach (string line in stepLines)
                        {
                            newInput.Append(buffer);
                            newInput.Append(line);
                            newInput.Append(Environment.NewLine);
                        }
                        input = newInput.ToString();
                    }

                    input = "steps:" + Environment.NewLine + input;
                }
            }
            return input;
        }

        private static List<string> SearchForVariables(string input)
        {
            List<string> variableList = new List<string>();

            string[] stepLines = input.Split(Environment.NewLine);
            foreach (string line in stepLines)
            {
                List<string> results = FindPipelineVariablesInString(line);
                if (results.Count > 0)
                {
                    variableList.AddRange(results);
                }
            }

            return variableList;
        }

        private static List<string> FindPipelineVariablesInString(string text)
        {
            //Used https://stackoverflow.com/questions/378415/how-do-i-extract-text-that-lies-between-parentheses-round-brackets
            //With the addition of the \$ search to capture strings like: "$(variable)"
            //\$\(            # $ char and escaped parenthesis, means "starts with a '$(' character"
            //    (          # Parentheses in a regex mean "put (capture) the stuff 
            //               #     in between into the Groups array" 
            //       [^)]    # Any character that is not a ')' character
            //       *       # Zero or more occurrences of the aforementioned "non ')' char"
            //    )          # Close the capturing group
            //\)             # "Ends with a ')' character"  
            MatchCollection results = Regex.Matches(text, @"\$\(([^)]*)\)");
            List<string> list = results.Cast<Match>().Select(match => match.Value).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                string item = list[i];

                //Remove leading "$(" and trailing ")"
                if (list[i].Length > 3)
                {
                    list[i] = list[i].Substring(0, item.Length - 1);
                    list[i] = list[i].Remove(0, 2);
                }
            }

            return list;
        }
    }
}
