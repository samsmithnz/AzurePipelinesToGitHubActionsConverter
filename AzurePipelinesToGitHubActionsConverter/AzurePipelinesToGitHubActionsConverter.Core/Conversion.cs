using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class Conversion
    {
        private string _matrixVariableName;

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
