using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class AzurePipelinesSerialization<T, T2>
    {
        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a simple trigger/ string[] and simple variable list/ Dictionary<string, string>
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with simple trigger and simple variables</returns>
        public static AzurePipelinesRoot<string[], Dictionary<string, string>> DeserializeSimpleTriggerAndSimpleVariables(string yaml)
        {
            yaml = CleanYaml(yaml);
            AzurePipelinesRoot<string[], Dictionary<string, string>> azurePipeline = Global.DeserializeYaml<AzurePipelinesRoot<string[], Dictionary<string, string>>>(yaml);
            return azurePipeline;
        }

        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a simple trigger/ string[] and simple variable list/ Dictionary<string, string>
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with simple trigger and complex variables</returns>
        public static AzurePipelinesRoot<string[], AzurePipelines.Variables[]> DeserializeSimpleTriggerAndComplexVariables(string yaml)
        {
            yaml = CleanYaml(yaml);
            AzurePipelinesRoot<string[], AzurePipelines.Variables[]> azurePipeline = Global.DeserializeYaml<AzurePipelinesRoot<string[], AzurePipelines.Variables[]>>(yaml);
            return azurePipeline;
        }

        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a complex trigger and simple variable list
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with complex trigger and simple variables</returns>
        public static AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>> DeserializeComplexTriggerAndSimpleVariables(string yaml)
        {
            yaml = CleanYaml(yaml);
            AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>> azurePipeline = Global.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>>>(yaml);
            return azurePipeline;
        }

        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a complex trigger and complex variable list
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with complex trigger and complex variables</returns>
        public static AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variables[]> DeserializeComplexTriggerAndComplexVariables(string yaml)
        {
            yaml = CleanYaml(yaml);
            AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variables[]> azurePipeline = Global.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variables[]>>(yaml);
            return azurePipeline;
        }

        private static string CleanYaml(string yaml)
        {
            //Handle a null input
            if (yaml == null)
            {
                yaml = "";
            }

            //Not well documented, but repo:self is redundent, and hence we remove it if detected (https://stackoverflow.com/questions/53860194/azure-devops-resources-repo-self)
            yaml = yaml.Replace("- repo: self", "");

            return yaml;
        }
    }
}
