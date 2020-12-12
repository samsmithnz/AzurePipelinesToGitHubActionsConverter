using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
                input = Replace(input, "variables['Build.SourceBranch']", "github.ref");
            }
            else if (input.IndexOf("eq(variables['Build.SourceBranchName']") >= 0)
            {
                input = Replace(input, "eq(variables['Build.SourceBranchName']", "endsWith(github.ref");
            }

            //System variables
            //TODO: Move this into a generic place for processing system variables and expand
            //TODO: Convert this to a case insenstive search and replace.
            input = Replace(input, "$(Build.ArtifactStagingDirectory)", "${GITHUB_WORKSPACE}");
            //input = Replace(input, "$(build.artifactstagingdirectory)", "${GITHUB_WORKSPACE}");
            input = Replace(input, "$(Build.SourcesDirectory)", "${GITHUB_WORKSPACE}");
            //input = Replace(input, "$(build.sourcesDirectory)", "${GITHUB_WORKSPACE}");
            //input = Replace(input, "$(build.sourcesdirectory)", "${GITHUB_WORKSPACE}");
            input = Replace(input, "$(Agent.OS)", "${{ runner.OS }}");

            return input;
        }

        //Reference: https://stackoverflow.com/questions/6275980/string-replace-ignoring-case
        private static string Replace(string input, string pattern, string replacement)
        {
            return Regex.Replace(input, Regex.Escape(pattern), replacement.Replace("$", "$$"), RegexOptions.IgnoreCase);
        }

    }
}
