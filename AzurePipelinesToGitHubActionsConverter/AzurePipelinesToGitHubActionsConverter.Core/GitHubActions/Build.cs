using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Build
    {
        public string runsOn { get; set; }
        public Step[] steps { get; set; }

    }
}
