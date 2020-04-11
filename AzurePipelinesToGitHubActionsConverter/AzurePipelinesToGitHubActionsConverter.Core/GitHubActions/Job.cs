using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Job
    {
        public string name { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idname
        public string runs_on { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idruns-on
        public Strategy strategy { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idstrategy
        //public T container { get; set; }
        public Container container { get; set; } //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema#job
        //public string container { get; set; } //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema#job
        public string timeout_minutes { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idtimeout-minutes
        public string needs { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idneeds
        public Dictionary<string, string> env { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idenv
        //as "if" is a reserved word in C#, added an "_", and remove this "_" when serializing
        public string _if { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idif
        public Step[] steps { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idsteps

        //This is used for tracking errors, so we don't want it to convert to YAML
        //[YamlIgnore]
        public string job_message;
    }
}
