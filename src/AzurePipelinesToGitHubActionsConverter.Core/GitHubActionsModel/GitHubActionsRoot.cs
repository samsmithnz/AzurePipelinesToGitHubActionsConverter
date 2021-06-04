using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class GitHubActionsRoot
    {
        public string name { get; set; }
        public Trigger on { get; set; }
        public Dictionary<string, string> env { get; set; }
        public Dictionary<string, Job> jobs { get; set; }

        //This is used for tracking errors, so we don't want it to convert to YAML
        [YamlIgnore]
        public List<string> messages { get; set; }

        public GitHubActionsRoot()
        {
            messages = new List<string>();
        }
    }
}