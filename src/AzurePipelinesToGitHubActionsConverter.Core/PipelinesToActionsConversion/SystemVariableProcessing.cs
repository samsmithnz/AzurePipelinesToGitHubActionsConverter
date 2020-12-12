using System.Text.RegularExpressions;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public static class SystemVariableProcessing
    {

        public static string ProcessSystemVariables(string input)
        {
            //TODO: Make the processing work for both conditions and environment variables. Can it be the same variable? Or do I need a branch to decide if there should be GitHub.Ref and GITHUB_REF
            //TODO: Add more variables.

            //Conditions
            //TODO: Note that this format (variables['name']) is conditions specific.
            if (input.IndexOf("variables['Build.SourceBranch']") >= 0)
            {
                input = Replace(input, "variables['Build.SourceBranch']", "github.ref");
            }
            else if (input.IndexOf("eq(variables['Build.SourceBranchName']") >= 0)
            {
                input = Replace(input, "eq(variables['Build.SourceBranchName']", "endsWith(github.ref");
            }

            //System variables
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
