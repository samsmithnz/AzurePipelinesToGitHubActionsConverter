using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class GitHubConversion
    {
        public string yaml { get; set; }
        public List<string> comments { get; set; }
    }
}
