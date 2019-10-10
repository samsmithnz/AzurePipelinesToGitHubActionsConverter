using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Step
    {
        public string name { get; set; }
        public string uses { get; set; }
        public string run { get; set; }
        public With with { get; set; }
    }
}
