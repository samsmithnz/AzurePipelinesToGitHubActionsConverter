using System.Text.RegularExpressions;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public static class SystemVariableProcessing
    {

        public static string ProcessSystemVariables(string input)
        {
            //Conditions
            //Create a rule to look for the entire branch path (e.g. "refs/heads/feature-branch-1"). If this is the default branch, it will just be ""
            input = Replace(input, "variables['Build.SourceBranch']", "github.ref");
            input = Replace(input, "variables['Build.ArtifactStagingDirectory']", "github.workspace");
            input = Replace(input, "variables['Build.BuildId']", "github.run_id");
            input = Replace(input, "variables['Build.BuildNumber']", "github.run_number");
            input = Replace(input, "variables['Build.SourceBranch']", "github.ref");
            input = Replace(input, "variables['Build.Repository.Name']", "github.repository");
            // input = Replace(input, "variables['Build.SourceBranchName']", "github.ref");
            input = Replace(input, "variables['Build.SourcesDirectory']", "github.workspace");
            input = Replace(input, "variables['Build.StagingDirectory']", "github.workspace");
            input = Replace(input, "variables['System.DefaultWorkingDirectory']", "github.workspace");
            input = Replace(input, "variables['Agent.OS']", "runner.os");
            //Create a rule to look for the branch name (e.g. "feature-branch-1" from "refs/heads/feature-branch-1"). 
            //Note that only the left brackets need to exist, so that the other side of the equation still exists
            //input = Replace(input, "eq(variables['Build.SourceBranchName']", "endsWith(github.ref");

            //System variables
            input = Replace(input, "$(Build.ArtifactStagingDirectory)", "${{ github.workspace }}");
            input = Replace(input, "$(Build.BuildId)", "${{ github.run_id }}");
            input = Replace(input, "$(Build.BuildNumber)", "${{ github.run_number }}");
            input = Replace(input, "$(Build.SourceBranch)", "${{ github.ref }}");
            input = Replace(input, "$(Build.Repository.Name)", "${{ github.repository }}");
            // input = Replace(input, "$(Build.SourceBranchName)", "${{ github.ref }}");
            input = Replace(input, "$(Build.SourcesDirectory)", "${{ github.workspace }}");
            input = Replace(input, "$(Build.StagingDirectory)", "${{ github.workspace }}");
            input = Replace(input, "$(System.DefaultWorkingDirectory)", "${{ github.workspace }}");
            input = Replace(input, "$(Agent.OS)", "${{ runner.os }}");

            return input;
        }

        //Reference: https://stackoverflow.com/questions/6275980/string-replace-ignoring-case
        private static string Replace(string input, string pattern, string replacement)
        {
            return Regex.Replace(input, Regex.Escape(pattern), replacement.Replace("$", "$$"), RegexOptions.IgnoreCase);
        }

    }
}
