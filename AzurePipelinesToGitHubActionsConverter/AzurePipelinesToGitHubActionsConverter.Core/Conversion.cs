using AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization;
using AzurePipelinesToGitHubActionsConverter.Core.Extensions;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public class Conversion
    {
        private string _matrixVariableName;

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

            input = ProcessSimplePools(input);

            //Some of the types are complex, having two data types. 
            //We handle this with generics, but it creates a lot of combinations we need to code around. 
            //Currently we have 12 combinations (typically this would be 16, but one of the types filters out another).
            //There is a picture of this in the Docs folder
            //Simple Trigger: string[]
            //Complex Trigger: AzurePipelines.Trigger
            //Simple Pool: string
            //Complex Pool: AzurePipelines.Pool
            //Simple DemandsOn: string (only possible with Complex Pool)
            //Complex DemandsOn: string[] (only possible with Complex Pool)
            //Simple Variables: Dictionary<string, string>
            //Complex Variables: AzurePipelines.Variable[]

            //There are 12 combinations here:
            var success = false;

            //SimpleTriggerSimplePoolSimpleVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<string[], string, string, Dictionary<string, string>>.DeserializeSimpleTriggerSimplePoolSimpleVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<string[], string, string, Dictionary<string, string>>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        pipeline.trigger, null,
                        pipeline.pool, null,
                        pipeline.variables, null);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }

            //SimpleTriggerSimplePoolComplexVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<string[], string, string, AzurePipelines.Variable[]>.DeserializeSimpleTriggerSimplePoolComplexVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<string[], string, string, AzurePipelines.Variable[]>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        pipeline.trigger, null,
                        pipeline.pool, null,
                        null, pipeline.variables);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //SimpleTriggerComplexPoolSimpleDemandsOnSimpleVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<string[], AzurePipelines.Pool, string, Dictionary<string, string>>.DeserializeSimpleTriggerComplexPoolSimpleDemandsOnSimpleVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<string[], AzurePipelines.Pool, string, Dictionary<string, string>>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        pipeline.trigger, null,
                        null, pipeline.pool,
                        pipeline.variables, null);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //SimpleTriggerComplexPoolSimpleDemandsOnComplexVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<string[], AzurePipelines.Pool, string, AzurePipelines.Variable[]>.DeserializeSimpleTriggerComplexPoolSimpleDemandsOnComplexVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<string[], AzurePipelines.Pool, string, AzurePipelines.Variable[]>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        pipeline.trigger, null,
                        null, pipeline.pool,
                        null, pipeline.variables);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //SimpleTriggerComplexPoolComplexDemandsOnSimpleVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<string[], AzurePipelines.Pool, string[], Dictionary<string, string>>.DeserializeSimpleTriggerComplexPoolComplexDemandsOnSimpleVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<string[], AzurePipelines.Pool, string[], Dictionary<string, string>>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        pipeline.trigger, null,
                        null, pipeline.pool,
                        pipeline.variables, null);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //SimpleTriggerComplexPoolComplexDemandsOnComplexVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<string[], AzurePipelines.Pool, string[], AzurePipelines.Variable[]>.DeserializeSimpleTriggerComplexPoolComplexDemandsOnComplexVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<string[], AzurePipelines.Pool, string[], AzurePipelines.Variable[]>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        pipeline.trigger, null,
                        null, pipeline.pool,
                        null, pipeline.variables);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //ComplexTriggerSimplePoolSimpleVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<AzurePipelines.Trigger, string, string, Dictionary<string, string>>.DeserializeComplexTriggerSimplePoolSimpleVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<AzurePipelines.Trigger, string, string, Dictionary<string, string>>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        null, pipeline.trigger,
                        pipeline.pool, null,
                        pipeline.variables, null);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //ComplexTriggerSimplePoolComplexVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<AzurePipelines.Trigger, string, string, AzurePipelines.Variable[]>.DeserializeComplexTriggerSimplePoolComplexVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<AzurePipelines.Trigger, string, string, AzurePipelines.Variable[]>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        null, pipeline.trigger,
                        pipeline.pool, null,
                        null, pipeline.variables);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //ComplexTriggerComplexPoolSimpleDemandsOnSimpleVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<AzurePipelines.Trigger, AzurePipelines.Pool, string, Dictionary<string, string>>.DeserializeComplexTriggerComplexPoolSimpleDemandsOnSimpleVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<AzurePipelines.Trigger, AzurePipelines.Pool, string, Dictionary<string, string>>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        null, pipeline.trigger,
                        null, pipeline.pool,
                        pipeline.variables, null);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //ComplexTriggerComplexPoolSimpleDemandsOnComplexVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<AzurePipelines.Trigger, AzurePipelines.Pool, string, AzurePipelines.Variable[]>.DeserializeComplexTriggerComplexPoolSimpleDemandsOnComplexVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<AzurePipelines.Trigger, AzurePipelines.Pool, string, AzurePipelines.Variable[]>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        null, pipeline.trigger,
                        null, pipeline.pool,
                        null, pipeline.variables);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //ComplexTriggerComplexPoolComplexDemandsOnSimpleVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<AzurePipelines.Trigger, AzurePipelines.Pool, string[], Dictionary<string, string>>.DeserializeComplexTriggerComplexPoolComplexDemandsOnSimpleVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<AzurePipelines.Trigger, AzurePipelines.Pool, string[], Dictionary<string, string>>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        null, pipeline.trigger,
                        null, pipeline.pool,
                        pipeline.variables, null);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }


            //ComplexTriggerComplexPoolComplexDemandsOnComplexVariables
            if (!success)
            {
                var pipeline = AzurePipelinesSerialization<AzurePipelines.Trigger, AzurePipelines.Pool, string[], AzurePipelines.Variable[]>.DeserializeComplexTriggerComplexPoolComplexDemandsOnComplexVariables(input);
                if (pipeline != null)
                {
                    success = true;
                    var processing = new PipelineProcessing<AzurePipelines.Trigger, AzurePipelines.Pool, string[], AzurePipelines.Variable[]>();
                    gitHubActions = processing.ProcessPipeline(pipeline,
                        null, pipeline.trigger,
                        null, pipeline.pool,
                        null, pipeline.variables);
                    if (processing.MatrixVariableName != null)
                    {
                        _matrixVariableName = processing.MatrixVariableName;
                    }
                    variableList.AddRange(processing.VariableList);
                }
            }

            if (!success)
            {
                gitHubActions = null;
            }


            //Search for any other variables. Duplicates are ok, they are processed the same
            variableList.AddRange(SearchForVariables(input));

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
                        stepComments.Add(ConvertMessageToYamlComment(message));
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
            }

            //Append all of the comments to the top of the file
            foreach (string item in stepComments)
            {
                yaml = item + Environment.NewLine + yaml;
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
            string processedInput = StepsPreProcessing(input);
            GitHubActions.Step gitHubActionStep = new GitHubActions.Step();

            //Process the YAML for the individual job
            AzurePipelines.Job azurePipelinesJob = GenericObjectSerialization.DeserializeYaml<AzurePipelines.Job>(processedInput);
            if (azurePipelinesJob != null && azurePipelinesJob.steps != null && azurePipelinesJob.steps.Length > 0)
            {
                //As we needed to create an entire (but minimal) pipelines job, we need to now extract the step for processing
                StepsProcessing stepsProcessing = new StepsProcessing();
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
            return new ConversionResponse
            {
                pipelinesYaml = input,
                actionsYaml = yaml,
                comments = allComments
            };
        }

        public string ProcessSimplePools(string yaml)
        {
            //If the yaml contains pools, check if it's a "simple pool" (pool: string]), 
            //and convert it to a "complex pool", (pool: \n  name: string)

            //e.g. "  pool: myImage\n" will become:
            //     "  pool: \n
            //     "    name: myImage\n

            if (yaml.IndexOf("pool", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                StringBuilder newYaml = new StringBuilder();
                foreach (string line in yaml.Split(Environment.NewLine))
                {
                    if (line.IndexOf("pool", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string[] items = line.Split(':');
                        if (items.Length > 1)
                        {
                            int prefixSpaceCount = items[0].TakeWhile(Char.IsWhiteSpace).Count();
                            newYaml.Append(items[0].Trim());
                            newYaml.Append(Environment.NewLine);
                            newYaml.Append("  name: ");
                            newYaml.Append(items[1].Trim());
                            newYaml.Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        newYaml.Append(line);
                        newYaml.Append(Environment.NewLine);
                    }
                }
                return newYaml.ToString();
            }
            else
            {
                return yaml;
            }
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
                        string buffer = Utility.GenerateSpaces(indentLevel);
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

            if (input != null)
            {
                string[] stepLines = input.Split(Environment.NewLine);
                foreach (string line in stepLines)
                {
                    List<string> variableResults = FindPipelineVariablesInString(line);
                    variableResults.AddRange(FindPipelineParametersInString(line));
                    if (variableResults.Count > 0)
                    {
                        variableList.AddRange(variableResults);
                    }
                }
            }

            return variableList;
        }

        private static List<string> FindPipelineVariablesInString(string text)
        {
            //Used https://stackoverflow.com/questions/378415/how-do-i-extract-text-that-lies-between-parentheses-round-brackets
            //With the addition of the \$ search to capture strings like: "$(variable)"
            //\$\(           # $ char and escaped parenthesis, means "starts with a '$(' character"
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

        private static List<string> FindPipelineParametersInString(string text)
        {
            //Used https://stackoverflow.com/questions/378415/how-do-i-extract-text-that-lies-between-parentheses-round-brackets
            //With the addition of the \$ search to capture strings like: "$(variable)"
            //\$\(           # $ char and escaped parenthesis, means "starts with a '$(' character"
            //    (          # Parentheses in a regex mean "put (capture) the stuff 
            //               #     in between into the Groups array" 
            //       [^)]    # Any character that is not a ')' character
            //       *       # Zero or more occurrences of the aforementioned "non ')' char"
            //    )          # Close the capturing group
            //\)             # "Ends with a ')' character"  
            MatchCollection results = Regex.Matches(text, @"\$\{\{([^}}]*)\}\}");
            List<string> list = results.Cast<Match>().Select(match => match.Value).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                string item = list[i];

                //Remove leading "${{" and trailing "}}"
                if (list[i].Length > 5)
                {
                    list[i] = list[i].Substring(0, item.Length - 2);
                    list[i] = list[i].Remove(0, 3);
                }
            }

            return list;
        }
    }
}
