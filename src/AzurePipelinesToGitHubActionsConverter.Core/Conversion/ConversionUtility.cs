using AzurePipelinesToGitHubActionsConverter.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public static class ConversionUtility
    {
        public static void WriteLine(string message, bool verbose)
        {
            if (verbose == true)
            {
                Console.WriteLine(message);
            }
        }

        public static string GenerateSpaces(int number)
        {
            return new String(' ', number);
        }

        // https://stackoverflow.com/questions/20411812/count-the-spaces-at-start-of-a-string
        public static int CountSpacesBeforeText(string input)
        {
            input = input.Replace(Environment.NewLine, "");
            return input.TakeWhile(char.IsWhiteSpace).Count();
        }

        public static string CleanYamlBeforeDeserialization(string yaml)
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
                StringBuilder sb = new StringBuilder();
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
                        if ((variablePrefixSpaceCount + 2) == line.TakeWhile(char.IsWhiteSpace).Count())
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
                    sb.Append(newLine);
                    sb.Append(System.Environment.NewLine);
                }
                processedYaml = sb.ToString();
            }

            ////Part 2b: Transform simple Variables to be complex variables:
            //StringBuilder newYaml = new StringBuilder();
            ////Search the YAML, line by line
            //foreach (string line in yaml.Split(System.Environment.NewLine))
            //{
            //    //If the search string is found, start processing it
            //    if (line.ToLower().IndexOf("variables:", StringComparison.OrdinalIgnoreCase) >= 0)
            //    {
            //        //Split the string by the :
            //        string[] items = line.Split(':');
            //        //if there are 2 sections, continue
            //        if (items.Length == 2 && items[1].ToString().Trim().Length > 0)
            //        {
            //            //Get the count of whitespaces in front of the variable
            //            int prefixSpaceCount = items[0].TakeWhile(char.IsWhiteSpace).Count();

            //            //start building the new string, with the white space count
            //            newYaml.Append(ConversionUtility.GenerateSpaces(prefixSpaceCount));
            //            //Add the main keyword
            //            newYaml.Append(items[0].Trim());
            //            newYaml.Append(": ");
            //            newYaml.Append(System.Environment.NewLine);
            //            //on the new line, add the white spaces + two more spaces for the indent
            //            newYaml.Append(ConversionUtility.GenerateSpaces(prefixSpaceCount + 2));
            //            newYaml.Append(newLineName);
            //            //The main value
            //            newYaml.Append(items[1].Trim());
            //            newYaml.Append(System.Environment.NewLine);
            //        }
            //        else
            //        {
            //            newYaml.Append(line);
            //            newYaml.Append(System.Environment.NewLine);
            //        }
            //    }
            //    else
            //    {
            //        newYaml.Append(line);
            //        newYaml.Append(System.Environment.NewLine);
            //    }
            //}

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

            //Process the trigger 
            if (processedYaml.ToLower().IndexOf("trigger:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                //Convert a (really) simple string trigger to a string[] trigger
                processedYaml = ProcessAndCleanElement(processedYaml, "trigger:", "- ");
                //Convert a simple string[] trigger to a complex Trigger 
            }
            //Process the pool 
            if (processedYaml.ToLower().IndexOf("pool:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessAndCleanElement(processedYaml, "pool:", "name: ");
            }
            //Then process the demands
            if (processedYaml.ToLower().IndexOf(" demands:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessAndCleanElement(processedYaml, "demands:", "- ");
            }
            //Then process the dependsOn
            if (processedYaml.ToLower().IndexOf(" dependson:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessAndCleanElement(processedYaml, "dependson:", "- ");
            }
            //Then process environment, to convert the simple string to a resourceName
            if (processedYaml.ToLower().IndexOf(" environment:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessAndCleanElement(processedYaml, " environment:", "resourceName: ");
            }
            //Then process the tags (almost identical to demands)
            if (processedYaml.ToLower().IndexOf(" tags:", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                processedYaml = ProcessAndCleanElement(processedYaml, " tags:", "- ");
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


        public static string ProcessAndCleanElement(string yaml, string searchString, string newLineName)
        {
            if (string.IsNullOrEmpty(yaml))
            {
                return null;
            }
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
                        int prefixSpaceCount = CountSpacesBeforeText(items[0]);

                        //start building the new string, with the white space count
                        newYaml.Append(GenerateSpaces(prefixSpaceCount));
                        //Add the main keyword
                        newYaml.Append(items[0].Trim());
                        newYaml.Append(": ");
                        newYaml.Append(System.Environment.NewLine);
                        //on the new line, add the white spaces + two more spaces for the indent
                        newYaml.Append(GenerateSpaces(prefixSpaceCount + 2));
                        newYaml.Append(newLineName);
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
            return ConversionUtility.RemoveFirstLine(newYaml.ToString().Trim());
        }

        //// Some elements have a simple, same line string, we need to make into a list
        //// for example "trigger:none", becomes "trigger:\n\r- none"
        //public static string ProcessNoneYamlElement(string yaml, string noneSearchString)
        //{
        //    if (yaml != null && yaml.Replace(" ", "").ToLower().IndexOf(noneSearchString) >= 0)
        //    {
        //        StringBuilder newYaml = new StringBuilder();
        //        foreach (string line in yaml.Split(Environment.NewLine))
        //        {
        //            if (line.Replace(" ", "").ToLower().IndexOf(noneSearchString) >= 0)
        //            {
        //                //Get the count of whitespaces in front of the variable
        //                int prefixSpaceCount = CountSpacesBeforeText(line);

        //                newYaml.Append(line.Replace("none", ""));
        //                newYaml.Append(System.Environment.NewLine);

        //                newYaml.Append(GenerateSpaces(prefixSpaceCount));
        //                newYaml.Append("- none");
        //                newYaml.Append(System.Environment.NewLine);
        //            }
        //            else
        //            {
        //                newYaml.Append(line);
        //                newYaml.Append(System.Environment.NewLine);
        //            }
        //        }
        //        return newYaml.ToString();
        //    }
        //    else
        //    {
        //        return yaml;
        //    }
        //}

        // Some elements have a simple, same line string, we need to make into a list
        // for example "trigger:none", becomes "trigger:\n\r- none"
        // This is a lot simplier in JSON, as it's already only returning the none string.
        public static string ProcessNoneJsonElement(string yaml)
        {
            if (yaml == "none")
            {
                return "[ none ]";
            }
            else
            {
                return yaml;
            }           
        }

        public static string ConvertMessageToYamlComment(string message)
        {
            //Append a comment to the message if one doesn't already exist
            if (message.TrimStart().StartsWith("#") == false)
            {
                message = "#" + message;
            }
            return message;
        }

        //Add a steps parent, to allow the processing of an individual step to proceed
        public static string StepsPreProcessing(string input)
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
                        string buffer = ConversionUtility.GenerateSpaces(indentLevel);
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

        //public static string JobsPreProcessing(string input)
        //{
        //    return input;
        //}

        public static List<string> SearchForVariables(string input)
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

        //public static string RemoveCommentsFromYaml(string yaml)
        //{
        //    if (yaml == null)
        //    {
        //        return yaml;
        //    }
        //    string processedYaml = yaml;

        //    //Part 1: remove full line comments. sometimes the yaml converter can't handle these - depending on where the # appears on the line (sometimes it's the first character, other times the first character after whitespace
        //    if (processedYaml.IndexOf("#") >= 0)
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        foreach (string line in processedYaml.Split(System.Environment.NewLine))
        //        {
        //            //Remove the comment if it's the a full line (after removing the preceeding white space)
        //            if (line.TrimStart().IndexOf("#") == 0)
        //            {
        //                //don't add line, remove
        //                Console.WriteLine(line);
        //            }
        //            else
        //            {
        //                sb.Append(line);
        //                sb.Append(System.Environment.NewLine);
        //            }
        //        }
        //        processedYaml = sb.ToString();
        //    }
        //    return processedYaml;
        //}

        //Remove the first line in a string
        public static string RemoveFirstLine(string input)
        {
            if (input == null)
            {
                return null;
            }
            input = input.Trim();

            StringBuilder sb = new StringBuilder();
            string[] lines = input.Split(Environment.NewLine);
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(lines[i]);
                    sb.Append(Environment.NewLine);
                }
            }

            return sb.ToString();
        }
    }
}
