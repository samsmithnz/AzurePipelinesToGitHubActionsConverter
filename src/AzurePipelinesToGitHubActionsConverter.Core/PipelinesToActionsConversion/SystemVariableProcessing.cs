using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public static class SystemVariableProcessing
    {

        public static string ProcessSystemVariables(string input)
        {
            //Conditions
            //TODO: Move this into a generic class for processing system variables.
            //TODO: Convert this to a case insenstive search and replace.
            //TODO: Add more variables. Note that this format (variables['name']) is conditions specific.
            if (input.IndexOf("variables['Build.SourceBranch']") >= 0)
            {
                input = input.Replace("variables['Build.SourceBranch']", "github.ref");
            }
            else if (input.IndexOf("eq(variables['Build.SourceBranchName']") >= 0)
            {
                input = input.Replace("eq(variables['Build.SourceBranchName']", "endsWith(github.ref");
            }

            return input;
        }

    }
}
