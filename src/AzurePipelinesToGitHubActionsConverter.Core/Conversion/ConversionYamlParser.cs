using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public class ConversionYamlParser
    {
        /// <summary>
        /// Returns a keyvaluepair of the element name, and it's child elements. 
        /// For example, a "trigger:\n- master", will be processed as a keyvault pair: <"trigger", "trigger\n:- master">
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Dictionary<string, string></returns>
        public static List<KeyValuePair<string, string>> GetYamlElements(string input, int spacesPrefix,
            bool useRootClass, bool useStageClass)
        {
            if (input == null)
            {
                return null;
            }

            List<KeyValuePair<string, string>> yamlElements = new List<KeyValuePair<string, string>>();
            string yamlElementName = "";
            StringBuilder yamlElementContent = new StringBuilder();
            foreach (string line in input.Split(System.Environment.NewLine))
            {
                //If our line contains a keyword, we need to create a new keyvalue pair for it
                bool rootCheck = useRootClass == true && ContainsRootKeyword(line) == true;
                bool stageCheck = useStageClass == true && ContainsStageKeyword(line) == true;
                if ((rootCheck == true || stageCheck == true) && ConversionUtility.CountSpacesBeforeText(line) == spacesPrefix)
                {
                    if (string.IsNullOrWhiteSpace(yamlElementContent.ToString().Trim()) == false)
                    {
                        //Add a new yaml element to the dictonary
                        yamlElements.Add(new KeyValuePair<string, string>(yamlElementName, yamlElementContent.ToString()));
                    }
                    //start a collection for our new keyword
                    yamlElementContent = new StringBuilder();
                    if (useRootClass == true)
                    {
                        yamlElementName = line.ToLower().Split(':')[0].Trim(); //Remove : and whitespace
                    }
                    else
                    {
                        yamlElementName = line.ToLower().Trim();
                    }
                    yamlElementContent.Append(line);
                    yamlElementContent.Append(System.Environment.NewLine);
                }
                else
                {
                    yamlElementContent.Append(line);
                    yamlElementContent.Append(System.Environment.NewLine);
                }
            }
            //Add a new yaml element to the dictonary
            yamlElements.Add(new KeyValuePair<string, string>(yamlElementName, yamlElementContent.ToString()));

            //string result = yamlItems.FirstOrDefault().Value.ToString();
            return yamlElements;
        }

        private static bool ContainsRootKeyword(string input)
        {
            //Use reflection to loop through all of the properties, looking to see if we are using that property
            AzurePipelinesRoot<string, string> root = new AzurePipelinesRoot<string, string>();
            foreach (var prop in root.GetType().GetProperties())
            {
                Debug.WriteLine(prop.Name);
                if (input.ToLower().IndexOf(prop.Name + ":", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    input.ToLower().IndexOf("-") < 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ContainsStageKeyword(string input)
        {
            //Use reflection to loop through all of the properties, looking to see if we are using that property
            Stage stage = new Stage();
            foreach (var prop in stage.GetType().GetProperties())
            {
                Debug.WriteLine(prop.Name);
                if (input.ToLower().IndexOf(prop.Name + ":", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
