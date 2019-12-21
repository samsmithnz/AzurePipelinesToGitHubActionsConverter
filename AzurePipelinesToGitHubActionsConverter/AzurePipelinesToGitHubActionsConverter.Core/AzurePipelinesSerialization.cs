using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class AzurePipelinesSerialization<T>
    {
        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a simple trigger (string[])
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with simple trigger</returns>
        public static AzurePipelinesRoot<string[]> DeserializeSimpleTrigger(string yaml)
        {
            yaml = CleanYaml(yaml);
            AzurePipelinesRoot<string[]> azurePipeline = Global.DeserializeYaml<AzurePipelinesRoot<string[]>>(yaml);
            return azurePipeline;
        }

        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a complex trigger
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with complex trigger</returns>
        public static AzurePipelinesRoot<AzurePipelines.Trigger> DeserializeComplexTrigger(string yaml)
        {
            yaml = CleanYaml(yaml);
            AzurePipelinesRoot<AzurePipelines.Trigger> azurePipeline = Global.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger>>(yaml);
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
