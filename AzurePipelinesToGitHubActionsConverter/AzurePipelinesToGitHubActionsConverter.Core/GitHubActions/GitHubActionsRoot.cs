using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class GitHubActionsRoot
    {
        public string name { get; set; }
        public Trigger on { get; set; }
        public Dictionary<string, string> env { get; set; }
        public Dictionary<string, Job> jobs { get; set; }
    }
}