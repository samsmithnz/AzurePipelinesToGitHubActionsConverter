using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization
{
    public class AzurePipelinesSerialization<TTriggers, TPool, TDemands, TVariables>
    {
        //Simple Trigger: string[]
        //Complex Trigger: AzurePipelines.Trigger
        //Simple Pool: string
        //Complex Pool: AzurePipelines.Pool
        //Simple DemandsOn: string (only possible with Complex Pool)
        //Complex DemandsOn: string[] (only possible with Complex Pool)
        //Simple Variables: Dictionary<string, string>
        //Complex Variables: AzurePipelines.Variable[]

        //DeserializeSimpleTriggerSimplePoolSimpleVariables
        public static AzurePipelinesRoot<string[], string, string, Dictionary<string, string>> DeserializeSimpleTriggerSimplePoolSimpleVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], string, string, Dictionary<string, string>>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }

        }

        //DeserializeSimpleTriggerSimplePoolComplexVariables
        public static AzurePipelinesRoot<string[], string, string, AzurePipelines.Variable[]> DeserializeSimpleTriggerSimplePoolComplexVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], string, string, AzurePipelines.Variable[]>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeSimpleTriggerComplexPoolSimpleDemandsOnSimpleVariables
        public static AzurePipelinesRoot<string[], AzurePipelines.Pool, string, Dictionary<string, string>> DeserializeSimpleTriggerComplexPoolSimpleDemandsOnSimpleVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], AzurePipelines.Pool, string, Dictionary<string, string>>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeSimpleTriggerComplexPoolSimpleDemandsOnComplexVariables
        public static AzurePipelinesRoot<string[], AzurePipelines.Pool, string, AzurePipelines.Variable[]> DeserializeSimpleTriggerComplexPoolSimpleDemandsOnComplexVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], AzurePipelines.Pool, string, AzurePipelines.Variable[]>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeSimpleTriggerComplexPoolComplexDemandsOnSimpleVariables
        public static AzurePipelinesRoot<string[], AzurePipelines.Pool, string[], Dictionary<string, string>> DeserializeSimpleTriggerComplexPoolComplexDemandsOnSimpleVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], AzurePipelines.Pool, string[], Dictionary<string, string>>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeSimpleTriggerComplexPoolComplexDemandsOnComplexVariables
        public static AzurePipelinesRoot<string[], AzurePipelines.Pool, string[], AzurePipelines.Variable[]> DeserializeSimpleTriggerComplexPoolComplexDemandsOnComplexVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], AzurePipelines.Pool, string[], AzurePipelines.Variable[]>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeComplexTriggerSimplePoolSimpleVariables
        public static AzurePipelinesRoot<AzurePipelines.Trigger, string, string, Dictionary<string, string>> DeserializeComplexTriggerSimplePoolSimpleVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, string, string, Dictionary<string, string>>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeComplexTriggerSimplePoolComplexVariables
        public static AzurePipelinesRoot<AzurePipelines.Trigger, string, string, AzurePipelines.Variable[]> DeserializeComplexTriggerSimplePoolComplexVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, string, string, AzurePipelines.Variable[]>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeComplexTriggerComplexPoolSimpleDemandsOnSimpleVariables
        public static AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Pool, string, Dictionary<string, string>> DeserializeComplexTriggerComplexPoolSimpleDemandsOnSimpleVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Pool, string, Dictionary<string, string>>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeComplexTriggerComplexPoolSimpleDemandsOnComplexVariables
        public static AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Pool, string, AzurePipelines.Variable[]> DeserializeComplexTriggerComplexPoolSimpleDemandsOnComplexVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Pool, string, AzurePipelines.Variable[]>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeComplexTriggerComplexPoolComplexDemandsOnSimpleVariables
        public static AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Pool, string[], Dictionary<string, string>> DeserializeComplexTriggerComplexPoolComplexDemandsOnSimpleVariables(string yaml)
        {
            try
            {
                yaml = CleanYamlBeforeDeserialization(yaml);
                return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Pool, string[], Dictionary<string, string>>>(yaml);
            }
            catch (Exception ex)
            {
                //Don't do anything with this error - we always want to throw this one away.
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        //DeserializeComplexTriggerComplexPoolComplexDemandsOnComplexVariables
        public static AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Pool, string[], AzurePipelines.Variable[]> DeserializeComplexTriggerComplexPoolComplexDemandsOnComplexVariables(string yaml)
        {
            //This isn't a mistake, don't have the try/catch on this function. 
            //It's the last deserialization, 

            //var result = new AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Pool, string[], AzurePipelines.Variable[]>();
            //try
            //{
            yaml = CleanYamlBeforeDeserialization(yaml);
            return GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Pool, string[], AzurePipelines.Variable[]>>(yaml);
            //}
            //catch (Exception ex)
            //{
            //    //Don't do anything with this error - we always want to throw this one away.
            //    Debug.WriteLine(ex.ToString());
            //}

        }

        ///// <summary>
        ///// Deserialize an Azure DevOps Pipeline with a simple trigger/ string[] and simple variable list/ Dictionary<string, string>
        ///// </summary>
        ///// <param name="yaml">yaml to convert</param>
        ///// <returns>Azure DevOps Pipeline with simple trigger and simple variables</returns>
        //public static AzurePipelinesRoot<string[], Dictionary<string, string>> DeserializeSimpleTriggerAndSimpleVariables(string yaml)
        //{
        //    yaml = CleanYamlBeforeDeserialization(yaml);
        //    AzurePipelinesRoot<string[], Dictionary<string, string>> azurePipeline = GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], Dictionary<string, string>>>(yaml);
        //    
        //}

        ///// <summary>
        ///// Deserialize an Azure DevOps Pipeline with a simple trigger/ string[] and simple variable list/ Dictionary<string, string>
        ///// </summary>
        ///// <param name="yaml">yaml to convert</param>
        ///// <returns>Azure DevOps Pipeline with simple trigger and complex variables</returns>
        //public static AzurePipelinesRoot<string[], AzurePipelines.Variable[]> DeserializeSimpleTriggerAndComplexVariables(string yaml)
        //{
        //    yaml = CleanYamlBeforeDeserialization(yaml);
        //    AzurePipelinesRoot<string[], AzurePipelines.Variable[]> azurePipeline = GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<string[], AzurePipelines.Variable[]>>(yaml);
        //    
        //}

        ///// <summary>
        ///// Deserialize an Azure DevOps Pipeline with a complex trigger and simple variable list
        ///// </summary>
        ///// <param name="yaml">yaml to convert</param>
        ///// <returns>Azure DevOps Pipeline with complex trigger and simple variables</returns>
        //public static AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>> DeserializeComplexTriggerAndSimpleVariables(string yaml)
        //{
        //    yaml = CleanYamlBeforeDeserialization(yaml);
        //    AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>> azurePipeline = GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, Dictionary<string, string>>>(yaml);
        //    
        //}

        ///// <summary>
        ///// Deserialize an Azure DevOps Pipeline with a complex trigger and complex variable list
        ///// </summary>
        ///// <param name="yaml">yaml to convert</param>
        ///// <returns>Azure DevOps Pipeline with complex trigger and complex variables</returns>
        //public static AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variable[]> DeserializeComplexTriggerAndComplexVariables(string yaml)
        //{
        //    yaml = CleanYamlBeforeDeserialization(yaml);
        //    AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variable[]> azurePipeline = GenericObjectSerialization.DeserializeYaml<AzurePipelinesRoot<AzurePipelines.Trigger, AzurePipelines.Variable[]>>(yaml);
        //    
        //}

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
