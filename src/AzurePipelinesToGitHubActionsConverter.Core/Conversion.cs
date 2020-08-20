using AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;

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
        /// <param name="input">Yaml to convert</param>
        /// <returns>Converion object, with original yaml, processed yaml, and comments on the conversion</returns>
        public ConversionResponse ConvertAzurePipelineToGitHubAction(string input)
        {
            List<string> variableList = new List<string>();
            string yaml;
            GitHubActionsRoot gitHubActions = null;

            //Run some processing to convert simple pools and demands to the complex editions, to avoid adding to the combinations below.
            //Also clean and remove variables with reserved words that get into trouble during deserialization. HACK alert... :(
            input = ConversionUtility.CleanYamlBeforeDeserialization(input);

            //Start the main deserialization methods
            bool success = false;
            if (success == false)
            {
                var azurePipelineWithSimpleTriggerAndSimpleVariables = AzurePipelinesSerialization<string[], Dictionary<string, string>>.DeserializeSimpleTriggerAndSimpleVariables(input);
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
                var azurePipelineWithSimpleTriggerAndComplexVariables = AzurePipelinesSerialization<string[], AzurePipelines.Variable[]>.DeserializeSimpleTriggerAndComplexVariables(input);
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
                var azurePipelineWithComplexTriggerAndSimpleVariables = AzurePipelinesSerialization<AzurePipelines.Trigger, Dictionary<string, string>>.DeserializeComplexTriggerAndSimpleVariables(input);
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
                var azurePipelineWithComplexTriggerAndComplexVariables = AzurePipelinesSerialization<AzurePipelines.Trigger, AzurePipelines.Variable[]>.DeserializeComplexTriggerAndComplexVariables(input);
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
            if (success == false && string.IsNullOrEmpty(input?.Trim()) == false)
            {
                throw new NotSupportedException("All deserialisation methods failed... oops! Please create a GitHub issue so we can fix this");
            }

            //Search for any other variables. Duplicates are ok, they are processed the same
            variableList.AddRange(ConversionUtility.SearchForVariables(input));

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


    }
}
