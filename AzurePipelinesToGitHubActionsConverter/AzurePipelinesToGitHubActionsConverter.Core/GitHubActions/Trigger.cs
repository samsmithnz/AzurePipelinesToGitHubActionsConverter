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
        //    tags:        
        //    - v1             # Push events to v1 tag
        //    - v1.*           # Push events to v1.0, v1.1, and v1.9 tags
        public TriggerDetail push { get; set; }
        public TriggerDetail pull_request { get; set; }
    }
}
