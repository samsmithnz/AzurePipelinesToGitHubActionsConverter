using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class ConversionResult
    {
        public string pipelinesYaml { get; set; }
        public string actionsYaml { get; set; }
        public List<string> comments { get; set; }
    }
}
