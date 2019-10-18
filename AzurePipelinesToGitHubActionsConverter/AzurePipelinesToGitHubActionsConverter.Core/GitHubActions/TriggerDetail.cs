using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class TriggerDetail
    {
        //    branches:    
        //    - Master
        //    - Develop
        //    branches-ignore:
        //    - 'mona/octocat'
        //    paths:
        //    - '**.js'
        //    paths-ignore:
        //    - 'docs/**'
        public string[] branches {get;set;}
        public string[] branches_ignore { get;set;}
        public string[] paths {get;set;}
        public string[] paths_ignore {get;set;}
    }
}
