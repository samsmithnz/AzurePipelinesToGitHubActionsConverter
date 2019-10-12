using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class GitHubActionsRoot
    {
        public string name { get; set; }
        public string on { get; set; }
        public Job[] jobs { get; set; }
    }
}
