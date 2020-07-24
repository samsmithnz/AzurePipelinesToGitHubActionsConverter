using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization
{
    public class AzurePipelinesSerialization<TTriggers, TVariables>
    {
        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a simple trigger/ string[] and simple variable list/ Dictionary<string, string>
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with simple trigger and simple variables</returns>
        public static AzurePipelinesRoot<string[], Dictionary<string, string>> DeserializeSimpleTriggerAndSimpleVariables(string yaml)
        {
            yaml = CleanYamlBeforeDeserialization(yaml);
            AzurePipelinesRoot<string[], Dictionary<string, string>> azurePipeline = GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], Dictionary<string, string>>>(yaml);
            return azurePipeline;
        }

        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a simple trigger/ string[] and simple variable list/ Dictionary<string, string>
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with simple trigger and complex variables</returns>
        public static AzurePipelinesRoot<string[], AzurePipelines.Variable[]> DeserializeSimpleTriggerAndComplexVariables(string yaml)
        {
            yaml = CleanYamlBeforeDeserialization(yaml);
            AzurePipelinesRoot<string[], AzurePipelines.Variable[]> azurePipeline = GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], AzurePipelines.Variable[]>>(yaml);
            return azurePipeline;
        }

        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a complex trigger and simple variable list
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with complex trigger and simple variables</returns>
        public static AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>> DeserializeComplexTriggerAndSimpleVariables(string yaml)
        {
            yaml = CleanYamlBeforeDeserialization(yaml);
            AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>> azurePipeline = GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>>>(yaml);
            return azurePipeline;
        }

        /// <summary>
        /// Deserialize an Azure DevOps Pipeline with a complex trigger and complex variable list
        /// </summary>
        /// <param name="yaml">yaml to convert</param>
        /// <returns>Azure DevOps Pipeline with complex trigger and complex variables</returns>
        public static AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variable[]> DeserializeComplexTriggerAndComplexVariables(string yaml)
        {
            yaml = CleanYamlBeforeDeserialization(yaml);
            AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variable[]> azurePipeline = GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variable[]>>(yaml);
            return azurePipeline;
        }

        private static string CleanYamlBeforeDeserialization(string yaml)
        {
            //Handle a null input
            if (yaml == null)
            {
                yaml = "";
            }

            //Not well documented, but repo:self is redundent, and hence we remove it if detected (https://stackoverflow.com/questions/53860194/azure-devops-resources-repo-self)
            yaml = yaml.Replace("- repo: self", "");

            //Fix some variables that we can't use for property names because the "-" character is not allowed in c# properties, or it's a reserved word (e.g. if)
            yaml = yaml.Replace("ref:", "_ref:");

            //Handle condition variable insertion syntax. This is a bit ugly. 
            if (yaml.IndexOf("variables") >= 0)
            {
                StringBuilder processedYaml = new StringBuilder();
                using (StringReader reader = new StringReader(yaml))
                {
                    int variablesIndentLevel = 0;
                    bool scanningForVariables = false;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        //Find the lines with variables
                        if (line.IndexOf("variables:") >= 0)
                        {
                            //Start tracking variables and record the variables indent level
                            scanningForVariables = true;
                            variablesIndentLevel = line.TakeWhile(Char.IsWhiteSpace).Count(); // https://stackoverflow.com/questions/20411812/count-the-spaces-at-start-of-a-string
                        }
                        else if (scanningForVariables == true)
                        {
                            //While scanning for variables, get the indent level. It should be (variablesIndentLevel + 2), if it's more than that, we have a variable insert.
                            Debug.WriteLine("Scanning for vars: " + line);
                            int lineIndentLevel = line.TakeWhile(Char.IsWhiteSpace).Count();
                            if ((variablesIndentLevel - (lineIndentLevel - 2)) == 0)
                            {
                                //If the line starts with a conditional insertation, then comment it out
                                if (line.Trim().StartsWith("${{") == true)
                                {
                                    line = "#" + line;
                                }
                            }
                            else if (variablesIndentLevel - (lineIndentLevel - 2) <= 0)
                            {
                                //we found a variable insert and need to remove the first two spaces from the front of the variable
                                line = line.Substring(2, line.Length - 2);
                            }
                            else if (variablesIndentLevel - (lineIndentLevel - 2) >= 0) //we are done with variables, and back at the next root node
                            {
                                scanningForVariables = false;
                            }
                        }
                        processedYaml.AppendLine(line);
                    }
                }
                yaml = processedYaml.ToString();
            }

            return yaml;
        }
    }
}
