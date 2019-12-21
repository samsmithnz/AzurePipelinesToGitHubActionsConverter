using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class Conversion
    {
        private string _matrixVariableName;

        public ConversionResult ConvertAzurePinelineTaskToGitHubActionTask(string input)
        {
            string yaml = "";
            string processedInput = StepsPreProcessing(input);

            GitHubActions.Step gitHubActionStep = new GitHubActions.Step();

            AzurePipelines.Job azurePipelinesJob = Global.SerializeYaml<AzurePipelines.Job>(processedInput);
            if (azurePipelinesJob != null && azurePipelinesJob.steps != null && azurePipelinesJob.steps.Length > 0)
            {
                AzurePipelines.Step azurePipelinesStep = azurePipelinesJob.steps[0];

                AzurePipelinesStepsProcessing stepsProcessing = new AzurePipelinesStepsProcessing();
                gitHubActionStep = stepsProcessing.ProcessStep(azurePipelinesStep);

                //Find all variables in this text block
                List<string> variableList = SearchForVariables(processedInput);

                //Create the YAML and apply some adjustments
                if (gitHubActionStep != null)
                {
                    //add the step into a github job so it renders correctly
                    GitHubActions.Job gitHubJob = new GitHubActions.Job
                    {
                        steps = new GitHubActions.Step[1] //create an array of size 1
                    };
                    gitHubJob.steps[0] = gitHubActionStep;

                    yaml = GitHubActionsSerialization.SerializeJob(gitHubJob, variableList);
                }
            }
            else
            {
                yaml = "";
            }

            //Load failed tasks and comments for processing
            List<string> allComments = new List<string>();
            if (gitHubActionStep != null)
            {
                allComments.Add(gitHubActionStep.step_message);
            }

            return new ConversionResult
            {
                pipelinesYaml = input,
                actionsYaml = yaml,
                comments = allComments
            };

        }

        public ConversionResult ConvertAzurePipelineToGitHubAction(string input)
        {
            List<string> variableList = new List<string>();
            string yaml;
            GitHubActionsRoot gitHubActions = null;

            //Triggers are hard, as there are two data types that can exist, so we need to go with the most common type and handle the less common type with generics
            AzurePipelinesRoot<string[]> azurePipelineWithSimpleTrigger = null;
            AzurePipelinesRoot<AzurePipelines.Trigger> azurePipelineWithComplexTrigger = null;
            try
            {
                azurePipelineWithSimpleTrigger = AzurePipelinesSerialization<string[]>.DeserializeSimpleTrigger(input);
            }
            catch
            {
                azurePipelineWithComplexTrigger = AzurePipelinesSerialization<AzurePipelines.Trigger>.DeserializeComplexTrigger(input);
            }

            //Generate the github actions
            if (azurePipelineWithSimpleTrigger != null)
            {
                AzurePipelinesProcessing<string[]> processing = new AzurePipelinesProcessing<string[]>();
                gitHubActions = processing.ProcessPipeline(azurePipelineWithSimpleTrigger, azurePipelineWithSimpleTrigger.trigger, null);

                if (processing.MatrixVariableName != null)
                {
                    _matrixVariableName = processing.MatrixVariableName;
                }
                variableList.AddRange(processing.VariableList);
            }
            else if (azurePipelineWithComplexTrigger != null)
            {
                AzurePipelinesProcessing<AzurePipelines.Trigger> processing = new AzurePipelinesProcessing<AzurePipelines.Trigger>();
                gitHubActions = processing.ProcessPipeline(azurePipelineWithComplexTrigger, null, azurePipelineWithComplexTrigger.trigger);

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
                        stepComments.Add(message);
                    }
                }
                //Add each individual step comments
                foreach (KeyValuePair<string, GitHubActions.Job> job in gitHubActions.jobs)
                {
                    if (job.Value.steps != null)
                    {
                        if (job.Value.job_message != null)
                        {
                            stepComments.Add(job.Value.job_message);
                        }
                        foreach (GitHubActions.Step step in job.Value.steps)
                        {
                            if (step != null && string.IsNullOrEmpty(step.step_message) == false)
                            {
                                stepComments.Add(step.step_message);
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

            return new ConversionResult
            {
                pipelinesYaml = input,
                actionsYaml = yaml,
                comments = stepComments
            };
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
                List<string> results = Global.FindPipelineVariablesInString(line);
                if (results.Count > 0)
                {
                    variableList.AddRange(results);
                }
            }

            return variableList;
        }

    }

}
