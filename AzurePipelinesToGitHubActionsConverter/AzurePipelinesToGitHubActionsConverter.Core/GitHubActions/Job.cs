using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Job
    {
        public string name { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idname
        public string runs_on { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idruns-on
        public string needs { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idneeds
        public Dictionary<string, string> env { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idenv
        public string _if { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idif
        public Step[] steps { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idsteps
        //TODO: Add strategy support
        //public string strategy { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idstrategy
        public string timeout_minutes { get; set; } //https://help.github.com/en/articles/workflow-syntax-for-github-actions#jobsjob_idtimeout-minutes
    }
}
