using System;
using System.IO;
using System.Text;
using System.Xml;
using YamlDotNet.RepresentationModel;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using System.Runtime.CompilerServices;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class Conversion
    {
        private List<string> _variableList;

        public string ConvertAzurePipelineToGitHubAction(string input)
        {
            //Triggers are hard, as there are two data types that can exist, so we need to go with the most common type and handle the less common type with generics
            AzurePipelinesRoot<string[]> azurePipelineWithSimpleTrigger = null;
            AzurePipelinesRoot<Trigger> azurePipelineWithComplexTrigger = null;
            try
            {
                azurePipelineWithSimpleTrigger = ReadYamlFile<AzurePipelinesRoot<string[]>>(input);
            }
            catch (Exception)
            {
                azurePipelineWithComplexTrigger = ReadYamlFile<AzurePipelinesRoot<Trigger>>(input);
            }
            _variableList = new List<string>();

            //Generate the github actions
            GitHubActionsRoot gitHubActions = null;
            if (azurePipelineWithSimpleTrigger != null)
            {
                AzurePipelinesProcessing<string[]> processing = new AzurePipelinesProcessing<string[]>();
                gitHubActions = processing.ProcessPipeline(azurePipelineWithSimpleTrigger, azurePipelineWithSimpleTrigger.trigger, null);

                _variableList.AddRange(processing.VariableList);
            }
            else if (azurePipelineWithComplexTrigger != null)
            {
                AzurePipelinesProcessing<Trigger> processing = new AzurePipelinesProcessing<Trigger>();
                gitHubActions = processing.ProcessPipeline(azurePipelineWithComplexTrigger, null, azurePipelineWithComplexTrigger.trigger);

                _variableList.AddRange(processing.VariableList);
            }

            if (gitHubActions != null)
            {
                string yaml = WriteYAMLFile<GitHubActionsRoot>(gitHubActions);

                //TODO: Resolve Sequence.Flow issue for on-push array to be on a single line (https://github.com/aaubry/YamlDotNet/issues/9)
                yaml = yaml.Replace("on:" + Environment.NewLine + "- push", "on: [push]");

                //Fix some variables for serialization, the '-' character is not valid in property names, and some of the YAML standard uses reserved words (e.g. if)
                yaml = PrepareYamlPropertiesForGitHubSerialization(yaml);

                //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
                yaml = PrepareYamlVariablesForGitHubSerialization(yaml);

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

            return ReadYamlFile<GitHubActionsRoot>(yaml);
        }
        private string PrepareYamlPropertiesForGitHubSerialization(string yaml)
        {
            //Fix some variables that we can't use for property names because the - character is not allowed or it's a reserved word (e.g. if)
            yaml = yaml.Replace("runs_on", "runs-on");
            yaml = yaml.Replace("_if", "if");
            yaml = yaml.Replace("timeout_minutes", "timeout-minutes");
            return yaml;
        }

        private string PrepareYamlVariablesForGitHubSerialization(string yaml)
        {
            foreach (string item in _variableList)
            {
                //Replace variables with the format "$(MyVar)" with the format "$MyVar"
                yaml = yaml.Replace("$(" + item + ")", "$" + item);
            }

            return yaml;
        }

        public T ReadYamlFile<T>(string yaml)
        {
            IDeserializer deserializer = new DeserializerBuilder()
                        .Build();
            T yamlObject = deserializer.Deserialize<T>(yaml);

            return yamlObject;
        }

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
