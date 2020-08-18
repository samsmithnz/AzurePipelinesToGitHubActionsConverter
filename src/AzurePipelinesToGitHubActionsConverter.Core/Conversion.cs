using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
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
            input = CleanYamlBeforeDeserialization(input);

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

        public string CleanYamlBeforeDeserialization(string yaml)
        {
            if (yaml == null)
            {
                return yaml;
            }
            string processedYaml = yaml;

            //Part 1: remove full line comments. sometimes the yaml converter can't handle these - depending on where the # appears on the line (sometimes it's the first character, other times the first character after whitespace
            if (processedYaml.IndexOf("#") >= 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string line in processedYaml.Split(System.Environment.NewLine))
                {
                    //Remove the comment if it's the a full line (after removing the preceeding white space)
                    if (line.TrimStart().IndexOf("#") == 0)
                    {
                        //don't add line, remove
                        Console.WriteLine(line);
                    }
                    else
                    {
                        sb.Append(line);
                        sb.Append(System.Environment.NewLine);
                    }
                }
                processedYaml = sb.ToString();
            }

            //Part 2
            //Process the variables, looking for reserved words
            if (processedYaml.ToLower().IndexOf("variables:", StringComparison.OrdinalIgnoreCase) >= 0 ||
                processedYaml.ToLower().IndexOf("parameters:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                StringBuilder newYaml = new StringBuilder();
                int variablePrefixSpaceCount = -1;
                foreach (string line in processedYaml.Split(System.Environment.NewLine))
                {
                    string newLine = line;
                    if (line.ToLower().IndexOf("variables:", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        line.ToLower().IndexOf("parameters:", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string[] items = line.Split(':');
                        if (items.Length == 2 && items[0].ToString().Trim().Length > 0)
                        {
                            variablePrefixSpaceCount = items[0].TakeWhile(char.IsWhiteSpace).Count();
                            //now that we have the variables start, we need to loop through the variable prefix space count + 2
                        }
                    }
                    else if (variablePrefixSpaceCount >= 0)
                    {
                        if (variablePrefixSpaceCount + 2 == line.TakeWhile(char.IsWhiteSpace).Count())
                        {
                            if (line.IndexOf("environment", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                newLine = line.Replace("environment", "environment2");
                            }
                            if (line.IndexOf("strategy", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                newLine = line.Replace("strategy", "strategy2");
                            }
                            if (line.IndexOf("pool", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                newLine = line.Replace("pool", "pool2");
                            }
                        }
                        else
                        {
                            variablePrefixSpaceCount = -1;
                        }
                    }
                    else
                    {
                        variablePrefixSpaceCount = -1;
                    }
                    newYaml.Append(newLine);
                    newYaml.Append(System.Environment.NewLine);
                }
                processedYaml = newYaml.ToString();
            }

            //Part 3
            //If the yaml contains pools, check if it's a "simple pool" (pool: string]), 
            //and convert it to a "complex pool", (pool: \n  name: string)
            //
            //e.g. "  pool: myImage\n" will become:
            //     "  pool: \n
            //     "    name: myImage\n
            //
            //We also repeat this same logic with demands, converting string to string[]
            //And also environment and tags

            //Process the pool 
            if (processedYaml.ToLower().IndexOf("pool:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessSection(processedYaml, "pool:", "name: ");
            }
            //Then process the demands
            if (processedYaml.ToLower().IndexOf(" demands:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessSection(processedYaml, "demands:", "- ");
            }
            //Then process the dependsOn
            if (processedYaml.ToLower().IndexOf(" dependson:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessSection(processedYaml, "dependson:", "- ");
            }
            //Then process environment, to convert the simple string to a resourceName
            if (processedYaml.ToLower().IndexOf(" environment:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessSection(processedYaml, " environment:", "resourceName: ");
            }

            //Then process the tags (almost identical to demands)
            if (processedYaml.ToLower().IndexOf(" tags:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessSection(processedYaml, " tags:", "- ");
            }


            //Part 4: conditional insertions/ variables
            //Process conditional variables
            if (processedYaml.IndexOf("{{#if") >= 0 || processedYaml.IndexOf("{{ #if") >= 0 ||
                processedYaml.IndexOf("${{if") >= 0 || processedYaml.IndexOf("${{ if") >= 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string line in processedYaml.Split(System.Environment.NewLine))
                {
                    if (line.IndexOf("{{#if") >= 0 || line.IndexOf("{{ #if") >= 0 ||
                        line.IndexOf("${{if") >= 0 || line.IndexOf("${{ if") >= 0)
                    {
                        //don't add line, remove
                    }
                    else if (line.IndexOf("{{/if") >= 0) //ending if 
                    {
                        //don't add line, remove
                    }
                    else
                    {
                        sb.Append(line);
                        sb.Append(System.Environment.NewLine);
                    }
                }
                processedYaml = sb.ToString();
            }

            return processedYaml;

        }

        private string ProcessSection(string yaml, string searchString, string newLineName)
        {
            StringBuilder newYaml = new StringBuilder();
            //Search the YAML, line by line
            foreach (string line in yaml.Split(System.Environment.NewLine))
            {
                //If the search string is found, start processing it
                if (line.ToLower().IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    line.ToLower().IndexOf(searchString + " |") == -1) //We don't want to catch the docker tags. This isn't perfect, but should catch most situations.
                {
                    //Split the string by the :
                    string[] items = line.Split(':');
                    //if there are 2 sections, continue
                    if (items.Length == 2 && items[1].ToString().Trim().Length > 0)
                    {
                        //Get the count of whitespaces in front of the variable
                        int prefixSpaceCount = items[0].TakeWhile(char.IsWhiteSpace).Count();

                        //start building the new string, with the white space count
                        newYaml.Append(Utility.GenerateSpaces(prefixSpaceCount));
                        //Add the main keyword
                        newYaml.Append(items[0].Trim());
                        newYaml.Append(": ");
                        newYaml.Append(System.Environment.NewLine);
                        //on the new line, add the white spaces + two more spaces for the indent
                        newYaml.Append(Utility.GenerateSpaces(prefixSpaceCount + 2)); newYaml.Append(newLineName);
                        //The main value
                        newYaml.Append(items[1].Trim());
                        newYaml.Append(System.Environment.NewLine);
                    }
                    else
                    {
                        newYaml.Append(line);
                        newYaml.Append(System.Environment.NewLine);
                    }
                }
                else
                {
                    newYaml.Append(line);
                    newYaml.Append(System.Environment.NewLine);
                }
            }
            return newYaml.ToString();
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
                string[] stepLines = input.Split(System.Environment.NewLine);
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
                            newInput.Append(System.Environment.NewLine);
                        }
                        input = newInput.ToString();
                    }

                    input = "steps:" + System.Environment.NewLine + input;
                }
            }
            return input;
        }

        private static List<string> SearchForVariables(string input)
        {
            List<string> variableList = new List<string>();

            if (input != null)
            {
                string[] stepLines = input.Split(System.Environment.NewLine);
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
