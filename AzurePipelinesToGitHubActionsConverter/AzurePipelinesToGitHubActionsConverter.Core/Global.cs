using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public static class Global
    {

        public static string GetHeaderComment()
        {
            return "# converted to GitHub Actions by https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter on " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:sstt");
        }

        public static string GetLineComment()
        {
            return "# WARNING: This line is unknown and may not have been migrated correctly";
        }

        public static string GenerateSpaces(int number)
        {
            return new String(' ', number);
        }

        public static List<string> FindVariables(string text)
        {
            //Used https://stackoverflow.com/questions/378415/how-do-i-extract-text-that-lies-between-parentheses-round-brackets
            //With the addition of the \$ search to capture strings like: "$(variable)"
            //\$\(            # $ char and escaped parenthesis, means "starts with a '$(' character"
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
                //list[i] = item.Remove(item.Length - 2, item.Length - 1);
                if (list[i].Length > 3)
                {
                    list[i] = list[i].Substring(0, item.Length - 1);
                    list[i] = list[i].Remove(0, 2);
                }
            }

            return list;
        }

    }
}
