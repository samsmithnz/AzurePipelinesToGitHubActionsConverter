using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public static class ConditionsProcessing
    {

        public static string TranslateConditions(string condition, bool isOuterContent = true)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return null;
            }
            //Sometimes conditions are spread over multiple lines, we are going to compress this to one line to make the processing easier
            condition = condition.Replace("\r\n", "");

            string processedCondition = "";

            //Get the condition. split the key word from the contents
            List<string> contentList = FindBracketedContentsInString(condition);

            //Examine the contents for last set of contents, the most complete piece of the contents, to get the keywords, recursively, otherwise, convert the contents to GitHub
            string contents = contentList[contentList.Count - 1];
            string conditionKeyWord = condition.Replace("(" + contents + ")", "").Trim();
            if (contents.IndexOf("(") >= 0)
            {
                //Need to count the number "(" brackets. If it's > 1, then iterate until we get to one.
                int bracketsCount = CountCharactersInString(contents, ')');
                if (bracketsCount > 1)
                {
                    //Split the strings by "," - but also respecting brackets
                    List<string> innerContents = SplitContents(contents);
                    StringBuilder innerContentsProcessed = new();
                    for (int i = 0; i < innerContents.Count; i++)
                    {
                        string innerContent = innerContents[i];
                        innerContentsProcessed.Append(TranslateConditions(innerContent, false));
                        if (i != innerContents.Count - 1)
                        {
                            innerContentsProcessed.Append(", ");
                        }
                    }
                    contents = innerContentsProcessed.ToString();
                }
            }

            //Join the pieces back together again
            processedCondition += ProcessCondition(conditionKeyWord, contents);

            //Translate any system variables
            processedCondition = SystemVariableProcessing.ProcessSystemVariables(processedCondition);

            //Replace any temp comma holders
            if (isOuterContent)
            {
                processedCondition = processedCondition.Replace("#=#", ",");
            }

            return processedCondition;
        }

        private static string ProcessCondition(string condition, string contents)
        {
            switch (condition.ToLower().Trim())
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
                case "succeededorfailed": //Essentially the same as "always", but not cancelled
                    return "(${{ job.status }} != 'cancelled')";

                //Functions: 
                //Azure DevOps: https://docs.microsoft.com/en-us/azure/devops/pipelines/process/expressions?view=azure-devops#functions
                //GitHub Actions: https://help.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#functions
                case "le": //<=
                    return "(" + contents.Replace(",", " <=") + ")";
                case "lt": //<
                    return "(" + contents.Replace(",", " <") + ")";
                case "ne": //!=
                    return "(" + contents.Replace(",", " !=") + ")";
                case "not": //!= true
                    return "(" + contents + " != true)";
                case "ge": //>=
                    return "(" + contents.Replace(",", " >=") + ")";
                case "gt": //>
                    return "(" + contents.Replace(",", " >") + ")";
                case "eq": //==
                    return "(" + contents.Replace(",", " ==") + ")";
                case "and": //&&
                    return "(" + contents.Replace(",", " &&") + ")";
                case "or": //|| 
                    return "(" + contents.Replace(",", " ||") + ")";

                //These are confirmed to be valid/documented
                case "contains": //contains( search, item )
                case "startswith": //startsWith
                case "endswith": //endsWith
                case "format": //format
                case "join": //join
                //TODO: Confirm if these are valid conditions
                case "coalesce": //coalesce
                case "containsvalue": //containsValue
                case "in": //in
                case "notin": //notin
                case "xor": //xor
                case "counter": //counter
                    //Add in the new condition, and add a #=# placeholder. This is a bit of a hack, but we will fix it later
                    return condition + "(" + contents.Replace(",", "#=#") + ")";

                default:
                    return "";
            }
        }

        //Public so that it can be unit tested
        public static List<string> FindBracketedContentsInString(string text)
        {
            IEnumerable<string> results = Nested(text);
            List<string> list = results.ToList<string>();
            //Remove the last item - that is the current item we don't need
            if (list.Count > 1)
            {
                list.RemoveAt(list.Count - 1);
            }

            return list;
        }

        private static IEnumerable<string> Nested(string value)
        {
            ////From: https://stackoverflow.com/questions/38479148/separate-nested-parentheses-in-c-sharp
            //if (string.IsNullOrEmpty(value))
            //{
            //    yield break; // or throw exception
            //}

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


        //////RegEx cannot handle nested brackets, so this first version doesn't work
        //public static List<string> SplitContents(string text)
        //{
        //    MatchCollection results = Regex.Matches(text, @",(?![^\(\[]*[\]\)])");
        //    List<string> list = new List<string>();
        //    int startIndex = 0;
        //    int endIndex;
        //    foreach (Match result in results)
        //    {
        //        endIndex = result.Index;
        //        list.Add(text.Substring(startIndex, (endIndex - startIndex)).Trim());
        //        startIndex = result.Index + 1;
        //    }
        //    endIndex = text.Length;
        //    list.Add(text.Substring(startIndex, (endIndex - startIndex)).Trim());
        //    return list;
        //}

        //Public so that it can be unit tested
        //Takes a string, and splits it by commas, respecting(). For example,
        //the string "eq('ABCDE', 'BCD'), ne(0, 1)", is split into "eq('ABCDE', 'BCD')" and "ne(0, 1)"
        //This was originally RegEx, but RegEx cannot handle nested brackets, so we wrote our own simple parser
        public static List<string> SplitContents(string text)
        {
            char splitCharacter = ',';
            List<string> list = new List<string>();
            StringBuilder sb = new StringBuilder();
            int openBracketCount = 0;

            foreach (char nextChar in text)
            {
                //If we have no open brackets, and the split character has been found, add the current string to the list 
                if (openBracketCount == 0 && nextChar == splitCharacter)
                {
                    list.Add(sb.ToString());
                    sb = new StringBuilder();
                }
                else if (nextChar == '(')
                {
                    //We found a open bracket - this is nested, but track it
                    openBracketCount++;
                    sb.Append(nextChar);
                }
                else if (nextChar == ')')
                {
                    //We found a closed bracket - if this is 0, we are not tracking anymore.
                    openBracketCount--;
                    sb.Append(nextChar);
                }
                else
                {
                    //Otherwise, append the character
                    sb.Append(nextChar);
                }
            }
            list.Add(sb.ToString());

            return list;
        }

    }
}
