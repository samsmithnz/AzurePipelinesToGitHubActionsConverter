using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public class ConversionResponse
    {
        public string pipelinesYaml { get; set; }
        public string actionsYaml { get; set; }
        public List<string> comments { get; set; }
        /// <summary>
        /// Don't use this property outside of unit testing, it's a migration, transition property and will be eventually removed.
        /// </summary>
        public bool v2ConversionSuccessful { get; set; }
    }
}
