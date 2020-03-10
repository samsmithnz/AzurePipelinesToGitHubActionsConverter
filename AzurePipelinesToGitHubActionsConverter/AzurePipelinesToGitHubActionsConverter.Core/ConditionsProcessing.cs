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
            string processedCondition = "";

            //Get the condition. split the key word from the contents
            List<string> contentList = FindBracketedContentsInString(condition);

            //Examine the contents for last set of contents, the most complete piece of the contents, to get the keywords, recursively, otherwise, convert the contents to GitHub
            string contents = GenerateConditions(contentList[contentList.Count - 1]);
            string conditionKeyWord = condition.Replace("(" + contentList[contentList.Count - 1] + ")", contents);

            //Join the pieces back together again
            processedCondition += ProcessCondition(conditionKeyWord, contents);

            //string processedCondition = "";
            //List<string> contentList = FindBracketedContentsInString(condition);
            //for (int i = contentList.Count - 1; i >= 0; i--)
            //{
            //    string contents = contentList[i];
            //    string conditionKeyWord = condition.Replace("(" + contentList[i] + ")", "");
            //    processedCondition += ProcessCondition(conditionKeyWord, contents);
            //}

            return processedCondition;
        }

        private static string ProcessCondition(string condition, string contents)
        {
            switch (condition)
            {
                //Job/step status check functions: 
                //Azure DevOps: https://docs.microsoft.com/en-us/azure/devops/pipelines/process/expressions?view=azure-devops#job-status-functions
                //GitHub Actions: https://help.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#job-status-check-functions

                case "always":
                case "canceled":
                case "failed":
                case "succeeded":
                    return condition + "(" + contents + ")";
                case "succeededOrFailed":
                    return ""; //TODO

                //Functions: 
                //Azure DevOps: https://docs.microsoft.com/en-us/azure/devops/pipelines/process/expressions?view=azure-devops#functions
                //GitHub Actions: https://help.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#functions

                case "le": //le
                case "lt": //lt
                case "ne": //ne
                case "not": //not
                case "ge": //ge
                case "gt": //gt
                case "eq": //eq
                case "and": //and
                case "or": //or
                case "contains": //contains( search, item )
                    return condition + "(" + contents + ")";

                //coalesce
                //containsValue
                //counter
                //endsWith
                //format
                //in
                //join
                //notin
                //startsWith
                //xor

                default:
                    return "";
            }
        }

        public static List<string> FindBracketedContentsInString(string text)
        {
            IEnumerable<string> results = Nested(text);
            List<string> list = results.ToList<string>();
            //Remove the last item
            if (list.Count > 1)
            {
                list.RemoveAt(list.Count - 1);
            }

            return list;
        }

        private static IEnumerable<string> Nested(string value)
        {
            //From: https://stackoverflow.com/questions/38479148/separate-nested-parentheses-in-c-sharp
            if (string.IsNullOrEmpty(value))
            {
                yield break; // or throw exception
            }

            Stack<int> brackets = new Stack<int>();

            for (int i = 0; i < value.Length; ++i)
            {
                char ch = value[i];

                if (ch == '(')
                {
                    brackets.Push(i);
                }
                else if (ch == ')')
                {
                    //TODO: you may want to check if close ']' has corresponding open '['
                    // i.e. stack has values: if (!brackets.Any()) throw ...
                    int openBracket = brackets.Pop();

                    yield return value.Substring(openBracket + 1, i - openBracket - 1);
                }
            }

            //TODO: you may want to check here if there're too many '['
            // i.e. stack still has values: if (brackets.Any()) throw ... 
            yield return value;
        }

    }
}
