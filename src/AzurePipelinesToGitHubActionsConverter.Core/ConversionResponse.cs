using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class ConversionResponse
    {
        public string pipelinesYaml { get; set; }
        public string actionsYaml { get; set; }
        public List<string> comments { get; set; }
    }
}
