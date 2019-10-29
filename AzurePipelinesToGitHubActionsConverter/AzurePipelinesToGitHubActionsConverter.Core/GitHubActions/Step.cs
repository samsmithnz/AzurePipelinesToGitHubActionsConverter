using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Step
    {
        public string name { get; set; }
        public string uses { get; set; }
        public string run { get; set; }
        public string shell { get; set; }
        public Dictionary<string, string> with { get; set; } //A key value pair similar to env
    }
}
