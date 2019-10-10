using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Root
    {
        public Root()
        {
           // jobs = new List<Job>();
        }

        public string name { get; set; }
        public string on { get; set; }
        public Job[] jobs { get; set; }
    }
}
