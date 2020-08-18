using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Job
    {
        //jobs:
        //- job: string  # name of the job, A-Z, a-z, 0-9, and underscore
        //  displayName: string  # friendly name to display in the UI
        //  dependsOn: string | [ string ]
        //  condition: string
        //  strategy:
        //    parallel: # parallel strategy, see below
        //    matrix: # matrix strategy, see below
        //    maxParallel: number # maximum number of matrix jobs to run simultaneously
        //  continueOnError: boolean  # 'true' if future jobs should run even if this job fails; defaults to 'false'
        //  pool: pool # see pool schema
        //  workspace:
        //    clean: outputs | resources | all # what to clean up before the job runs
        //  container: containerReference # container to run this job inside
        //  timeoutInMinutes: number # how long to run the job before automatically cancelling
        //  cancelTimeoutInMinutes: number # how much time to give 'run always even if cancelled tasks' before killing them
        //  variables: { string: string } | [ variable | variableReference ] 
        //  steps: [ script | bash | pwsh | powershell | checkout | task | templateReference ]
        //  services: { string: string | container } # container resources to run as a service container
        public string job { get; set; }
        public string deployment { get; set; }
        public Environment environment { get; set; }
        public string displayName { get; set; }
        public string[] dependsOn { get; set; }
        [DefaultValue("succeeded()")]
        public string condition { get; set; } //https://docs.microsoft.com/en-us/azure/devops/pipelines/process/conditions?tabs=yaml&view=azure-devops
        public bool continueOnError { get; set; }
        [DefaultValue(0)]
        public int timeoutInMinutes { get; set; } = 0;
        [DefaultValue(1)]
        public int cancelTimeoutInMinutes { get; set; } = 1;
        public Strategy strategy { get; set; }
        public Pool pool { get; set; }
        public Dictionary<string, string> variables { get; set; }
        public Step[] steps { get; set; }
        //TODO: Add code for services
        public Dictionary<string, string> services { get; set; }
        public string template { get; set; }
        public Dictionary<string, string> parameters { get; set; }
        public Workspace workspace { get; set; }

    }
}
