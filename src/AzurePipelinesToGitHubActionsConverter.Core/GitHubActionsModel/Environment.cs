using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    ////https://devblogs.microsoft.com/devops/i-need-manual-approvers-for-github-actions-and-i-got-them-now/
    //environment:
    //  name: abelNodeDemoAppEnv.prod
    //  url: https://abel-node-gh-accelerator.azurewebsites.net

    public class Environment
    {
        public string name { get; set; }
        //public string url { get; set; }
    }
}
