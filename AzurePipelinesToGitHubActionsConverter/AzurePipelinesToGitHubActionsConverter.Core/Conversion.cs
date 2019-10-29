using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class Conversion
    {
        private string _matrixVariableName;

        public string ConvertAzurePinelineTaskToGitHubActionTask(string input)
        {
            List<string> variableList = new List<string>();

            input = StepsPreProcessing(input);

            GitHubActions.Step gitHubActionStep = new GitHubActions.Step();
            AzurePipelinesProcessing<string[]> processing = new AzurePipelinesProcessing<string[]>();

            AzurePipelines.Job azurePipelinesJob = ReadYamlFile<AzurePipelines.Job>(input);
            if (azurePipelinesJob != null && azurePipelinesJob.steps != null && azurePipelinesJob.steps.Length > 0)
            {
                AzurePipelines.Step azurePipelinesStep = azurePipelinesJob.steps[0];

                AzurePipelinesStepsProcessing stepsProcessing = new AzurePipelinesStepsProcessing();
                gitHubActionStep = stepsProcessing.ProcessStep(azurePipelinesStep);

                //Find all variables in this text block
                variableList = SearchForVariables(input);
                //Global.FindVariables();

                //Create the YAML and apply some adjustments
                if (gitHubActionStep != null)
                {
                    //add the step into a github job so it renders correctly
                    GitHubActions.Job gitHubJob = new GitHubActions.Job
                    {
                        steps = new GitHubActions.Step[1]
                    };
                    gitHubJob.steps[0] = gitHubActionStep;
                    string yaml = WriteYAMLFile<GitHubActions.Job>(gitHubJob);

                    //Fix some variables for serialization, the '-' character is not valid in property names, and some of the YAML standard uses reserved words (e.g. if)
                    yaml = PrepareYamlPropertiesForGitHubSerialization(yaml);

                    //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
                    yaml = PrepareYamlVariablesForGitHubSerialization(yaml, variableList);

                    yaml = StepsPostProcessing(yaml);

                    //Trim off any leading of trailing new lines 
                    yaml = yaml.TrimStart('\r', '\n');
                    yaml = yaml.TrimEnd('\r', '\n');

                    return yaml;
                }
            }
            return "";

        }

        public string ConvertAzurePipelineToGitHubAction(string input)
        {
            List<string> variableList = new List<string>();

            //Triggers are hard, as there are two data types that can exist, so we need to go with the most common type and handle the less common type with generics
            AzurePipelinesRoot<string[]> azurePipelineWithSimpleTrigger = null;
            AzurePipelinesRoot<AzurePipelines.Trigger> azurePipelineWithComplexTrigger = null;
            try
            {
                azurePipelineWithSimpleTrigger = ReadYamlFile<AzurePipelinesRoot<string[]>>(input);
            }
            catch (Exception)
            {
                azurePipelineWithComplexTrigger = ReadYamlFile<AzurePipelinesRoot<AzurePipelines.Trigger>>(input);
            }

            //Generate the github actions
            GitHubActionsRoot gitHubActions = null;
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
                string yaml = WriteYAMLFile<GitHubActionsRoot>(gitHubActions);

                //Fix some variables for serialization, the '-' character is not valid in property names, and some of the YAML standard uses reserved words (e.g. if)
                yaml = PrepareYamlPropertiesForGitHubSerialization(yaml);

                //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
                yaml = PrepareYamlVariablesForGitHubSerialization(yaml, variableList);

                //Trim off any leading of trailing new lines 
                yaml = yaml.TrimStart('\r', '\n');
                yaml = yaml.TrimEnd('\r', '\n');

                return yaml;
            }
            else
            {
                return "";
            }
        }

        public GitHubActionsRoot ReadGitHubActionsYaml(string yaml)
        {
            //Fix some variables that we can't use for property names because the - character is not allowed or it's a reserved word (e.g. if)
            yaml = yaml.Replace("runs-on", "runs_on");
            yaml = yaml.Replace("if", "_if");
            yaml = yaml.Replace("timeout-minutes", "timeout_minutes");
            yaml = yaml.Replace("pull-request", "pull_request");
            yaml = yaml.Replace("branches-ignore", "branches_ignore");
            yaml = yaml.Replace("paths-ignore", "paths_ignore");
            yaml = yaml.Replace("tags-ignore", "tags_ignore");
            yaml = yaml.Replace("max-parallel", "max_parallel");

            return ReadYamlFile<GitHubActionsRoot>(yaml);
        }

        private string PrepareYamlPropertiesForGitHubSerialization(string yaml)
        {
            //Fix system variables
            yaml = yaml.Replace("$(build.artifactstagingdirectory)", "${GITHUB_WORKSPACE}");

            //Fix some variables that we can't use for property names because the - character is not allowed or it's a reserved word (e.g. if)
            yaml = yaml.Replace("runs_on", "runs-on");
            yaml = yaml.Replace("_if", "if");
            yaml = yaml.Replace("timeout_minutes", "timeout-minutes");
            yaml = yaml.Replace("pull_request", "pull-request");
            yaml = yaml.Replace("branches_ignore", "branches-ignore");
            yaml = yaml.Replace("paths_ignore", "paths-ignore");
            yaml = yaml.Replace("tags_ignore", "tags-ignore");
            yaml = yaml.Replace(">-", "|"); //Replace a weird artifact in scripts when converting pipes

            yaml = yaml.Replace("max_parallel", "max-parallel");

            return yaml;
        }

        private string PrepareYamlVariablesForGitHubSerialization(string yaml, List<string> variableList)
        {
            if (_matrixVariableName != null)
            {
                variableList.Add(_matrixVariableName);
            }

            foreach (string item in variableList)
            {
                if (item == _matrixVariableName)
                {
                    yaml = yaml.Replace("$(" + item + ")", "${{ matrix." + item + " }}");
                    yaml = yaml.Replace("$( " + item + " )", "${{ matrix." + item + " }}");
                }
                else
                {
                    //Replace variables with the format "$(MyVar)" with the format "$MyVar"
                    yaml = yaml.Replace("$(" + item + ")", "$" + item);
                    yaml = yaml.Replace("$( " + item + " )", "$" + item);
                    yaml = yaml.Replace("$(" + item + " )", "$" + item);
                    yaml = yaml.Replace("$( " + item + ")", "$" + item);
                }
            }

            return yaml;
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

        //Strip the steps off to focus on just the individual step
        private string StepsPostProcessing(string input)
        {
            if (input.Trim().StartsWith("steps:") == true)
            {
                //we need to remove steps, before we do, we need to see if the task needs to remove indent
                string[] stepLines = input.Split(Environment.NewLine);
                if (stepLines.Length > 0)
                {
                    int i = 0;
                    //Search for the first non empty line
                    while (string.IsNullOrEmpty(stepLines[i].Trim()) == true || stepLines[i].Trim().StartsWith("steps:") == true)
                    {
                        i++;
                    }
                    if (stepLines[i].StartsWith("-") == true)
                    {
                        int indentLevel = stepLines[i].IndexOf("-");
                        if (indentLevel >= 2)
                        {
                            indentLevel -= 2;
                        }
                        string buffer = Global.GenerateSpaces(indentLevel);
                        StringBuilder newInput = new StringBuilder();
                        foreach (string line in stepLines)
                        {
                            if (line.Trim().StartsWith("steps:") == false)
                            {
                                newInput.Append(buffer);
                                newInput.Append(line);
                                newInput.Append(Environment.NewLine);
                            }
                        }
                        input = newInput.ToString();
                    }
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
                List<string> results = Global.FindVariables(line);
                if (results.Count > 0)
                {
                    variableList.AddRange(results);
                }
            }

            return variableList;
        }

        //Read in a YAML file and convert it to a T object
        public T ReadYamlFile<T>(string yaml)
        {
            IDeserializer deserializer = new DeserializerBuilder()
                        .Build();
            T yamlObject = deserializer.Deserialize<T>(yaml);

            return yamlObject;
        }

        //Write a YAML file using the T object
        public string WriteYAMLFile<T>(T obj)
        {
            //Convert the object into a YAML document
            ISerializer serializer = new SerializerBuilder()
                    .Build();
            string yaml = serializer.Serialize(obj);

            return yaml;
        }

    }

}
