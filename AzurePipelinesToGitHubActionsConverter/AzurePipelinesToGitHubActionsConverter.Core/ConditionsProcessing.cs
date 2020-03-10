using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public static class ConditionsProcessing
    {

        public static string GenerateConditions(string condition)
        {
            string[] individualConditions = condition.Split(',');

            string processedCondition = "";
            foreach (string individualCondition in individualConditions)
            {
                string contents = "";
                processedCondition += ProcessCondition(individualCondition, contents);
            }

            return processedCondition;
            //return condition;
        }

        private static string ProcessCondition(string condition, string contents)
        {
            switch (condition)
            {
                //Job/step status check functions: 
                //Azure DevOps: https://docs.microsoft.com/en-us/azure/devops/pipelines/process/expressions?view=azure-devops#job-status-functions
                //GitHub Actions: https://help.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#job-status-check-functions

                case "always()":
                    return "always()";
                case "canceled()":
                    return "cancelled()";
                case "failed()":
                    return "failure()";
                case "succeeded()":
                    return "success()";
                case "succeededOrFailed":
                    return ""; //TODO

                //Functions: 
                //Azure DevOps: https://docs.microsoft.com/en-us/azure/devops/pipelines/process/expressions?view=azure-devops#functions
                //GitHub Actions: https://help.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#functions

                //contains( search, item )
                case "contains()":
                    return "contains(" + contents + ")";

                //and
                //coalesce

                //containsValue
                //counter
                //endsWith
                //eq
                //format
                //ge
                //gt
                //in
                //join
                //le
                //lt
                //ne
                //not
                //notin
                //or
                //startsWith
                //xor
                default:
                    return "";
            }
        }

        public static List<string> FindBracketedContentsInString(string text)
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
            MatchCollection results = Regex.Matches(text, @"(?<=\[).+?(?=\])");
            List<string> list = results.Cast<Match>().Select(match => match.Value).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                string item = list[i];

                //Remove leading "$(" and trailing ")"
                //if (list[i].Length > 3)
                //{
                //    list[i] = list[i].Substring(0, item.Length - 1);
                //    list[i] = list[i].Remove(0, 2);
                //}
            }

            return list;
        }

    }
}
