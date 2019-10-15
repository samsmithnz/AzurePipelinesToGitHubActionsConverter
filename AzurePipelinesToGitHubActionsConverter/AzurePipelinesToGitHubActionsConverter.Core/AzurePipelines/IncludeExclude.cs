using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class IncludeExclude
    {
        public string[] include { get; set; }
        public string[] exclude { get; set; }
    }
}
