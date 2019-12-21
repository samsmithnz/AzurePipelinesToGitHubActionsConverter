using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class AzurePipelinesSerialization<T>
    {

        public static AzurePipelinesRoot<string[]> DeserializeSimpleTrigger(string input)
        {
            input = CleanInput(input);
            AzurePipelinesRoot<string[]> azurePipeline = Global.SerializeYaml<AzurePipelinesRoot<string[]>>(input);
            return azurePipeline;
        }

        public static AzurePipelinesRoot<AzurePipelines.Trigger> DeserializeComplexTrigger(string input)
        {
            input = CleanInput(input);
            AzurePipelinesRoot<AzurePipelines.Trigger> azurePipeline = Global.SerializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger>>(input);
            return azurePipeline;
        }

        private static string CleanInput(string input)
        {
            //Handle a null input
            if (input == null)
            {
                input = "";
            }

            //Not well documented, but repo:self is redundent, and hence we remove it if detected (https://stackoverflow.com/questions/53860194/azure-devops-resources-repo-self)
            input = input.Replace("- repo: self", "");

            return input;
        }
    }
}
