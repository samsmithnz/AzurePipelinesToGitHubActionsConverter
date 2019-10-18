using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Trigger
    {
        //on:
        //  push:
        //    branches:    
        //    - Master
        //    - Develop
        //    branches-ignore:
        //    - 'mona/octocat'
        //    paths:
        //    - '**.js'
        //    paths-ignore:
        //    - 'docs/**'
        //  pull-request
        //    branches:    
        //    - Master
        //    - Develop
        //    branches-ignore:
        //    - 'mona/octocat'
        //    paths:
        //    - '**.js'
        //    paths-ignore:
        //    - 'docs/**'
        public TriggerDetail push { get; set; }
        public TriggerDetail pull_request { get; set; }
    }
}
