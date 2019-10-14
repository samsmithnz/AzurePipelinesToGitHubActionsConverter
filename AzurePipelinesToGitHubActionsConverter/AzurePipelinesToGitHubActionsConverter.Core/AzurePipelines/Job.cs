using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Job
    {
        //stages:
        //- stage: Build
        //  displayName: 'Build/Test Stage'
        //  jobs:
        //  - job: Build
        //    displayName: 'Build job'
        //    pool:
        //      vmImage: $(vmImage)
        //    steps:
        public string job { get; set; }
        public string displayName { get; set; }
        public string dependsOn { get; set; }
        public string condition { get; set; }
        public string timeoutInMinutes { get; set; }
        public Pool pool { get; set; }
        public Dictionary<string, string> variables { get; set; }
        public Step[] steps { get; set; }
    }
}
