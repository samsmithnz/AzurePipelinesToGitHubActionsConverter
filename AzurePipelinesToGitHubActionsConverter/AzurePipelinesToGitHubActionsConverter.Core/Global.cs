using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
