using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Step
    {

        public string name { get; set; }
        public string uses { get; set; }
        public string run { get; set; }
        public string shell { get; set; }
        public Dictionary<string, string> with { get; set; } //A key value pair similar to env
        public Dictionary<string, string> env { get; set; } //Similar to the job env: https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idenv

        //This is used for tracking errors, so we don't want it to convert to YAML
        [YamlIgnore]
        public string comment;
    }
}
