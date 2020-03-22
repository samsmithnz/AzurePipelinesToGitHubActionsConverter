using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public static class ConditionsProcessing
    {

        public static string TranslateConditions(string condition)
        {
            if (condition == null)
            {
                return null;
            }

            string processedCondition = "";

            //Get the condition. split the key word from the contents
            List<string> contentList = FindBracketedContentsInString(condition);

            //Examine the contents for last set of contents, the most complete piece of the contents, to get the keywords, recursively, otherwise, convert the contents to GitHub
            string contents = contentList[contentList.Count - 1];
            string conditionKeyWord = condition.Replace("(" + contents + ")", "");
            if (contents.IndexOf("(") >= 0)
            {
                //Need to count the number "(" brackets. If it's > 1, then iterate to get to one.
                int bracketsCount = CountCharactersInString(contents, ')');
                if (bracketsCount > 1)
                {
                    //Split the strings by "," - but also respecting brackets
                    List<string> innerContents = SplitContents(contents);
                    string innerContentsProcessed = "";
                    for (int i = 0; i < innerContents.Count; i++)
                    {
                        string innerContent = innerContents[i];
                        innerContentsProcessed += TranslateConditions(innerContent);
                        if (i != innerContents.Count - 1)
                        {
                            innerContentsProcessed += ",";
                        }
                    }
                    contents = innerContentsProcessed;
                }
            }

            //Join the pieces back together again
            processedCondition += ProcessCondition(conditionKeyWord, contents);
            
            //Translate any system variables
            processedCondition = ProcessVariables(processedCondition);

            return processedCondition;
        }

        private static string ProcessVariables(string condition)
        {
            if (condition.IndexOf("variables['Build.SourceBranch']") >= 0)
            {
                condition = condition.Replace("variables['Build.SourceBranch']", "github.ref");
            }
            else if (condition.IndexOf("eq(variables['Build.SourceBranchName']") >= 0)
            {
                condition = condition.Replace("eq(variables['Build.SourceBranchName']", "endsWith(github.ref");
            }

            return condition;
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
                    return condition + "(" + contents + ")";
                case "failed":
                    return "failure(" + contents + ")";
                case "succeeded":
                    return "success(" + contents + ")";
                case "succeededOrFailed": //Essentially the same as always, but not cancelled
                    return "ne(${{ job.status }}, 'cancelled')";

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
                case "coalesce": //coalesce
                case "containsValue": //containsValue
                case "endsWith": //endsWith
                case "format": //format
                case "in": //in
                case "join": //join
                case "notin": //notin
                case "startsWith": //startsWith
                case "xor": //xor
                case "counter": //counter
                    return condition + "(" + contents + ")";

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
                    //Possible Future enhancement: you may want to check if close ']' has corresponding open '('
                    // i.e. stack has values: if (!brackets.Any()) throw ...
                    int openBracket = brackets.Pop();

                    yield return value.Substring(openBracket + 1, i - openBracket - 1);
                }
            }

            //Possible Future enhancement: you may want to check here if there're too many '('
            // i.e. stack still has values: if (brackets.Any()) throw ... 
            yield return value;
        }

        private static int CountCharactersInString(string text, char character)
        {
            return text.Count(x => x == character);
        }

        //Takes a string, and splits it by commas, respecting (). For example, 
        // the string "eq('ABCDE', 'BCD'), ne(0, 1)", is split into "eq('ABCDE', 'BCD')" and "ne(0, 1)"
        private static List<string> SplitContents(string text)
        {
            MatchCollection results = Regex.Matches(text, @",(?![^\(\[]*[\]\)])");
            List<string> list = new List<string>();
            int startIndex = 0;
            int endIndex = 0;
            foreach (Match result in results)
            {
                endIndex = result.Index;
                list.Add(text.Substring(startIndex, (endIndex - startIndex)).Trim());
                startIndex = result.Index + 1;
            }
            endIndex = text.Length;
            list.Add(text.Substring(startIndex, (endIndex - startIndex)).Trim());
            return list;
        }

    }
}
