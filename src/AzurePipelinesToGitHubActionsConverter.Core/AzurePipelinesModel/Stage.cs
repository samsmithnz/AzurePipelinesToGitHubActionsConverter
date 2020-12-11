using System.Collections.Generic;
namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Stage
    {
        //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema%2Cparameter-schema#stage
        //stages:
        //- stage: string  # name of the stage (A-Z, a-z, 0-9, and underscore)
        //  displayName: string  # friendly name to display in the UI
        //  dependsOn: string | [ string ]
        //  condition: string
        //  variables: # several syntaxes, see specific section
        //  jobs: [ job | templateReference]
        public string stage { get; set; }
        public string displayName { get; set; } //This variable is not needed in actions
        //Add dependsOn processing for stages
        public string[] dependsOn { get; set; }
        public string condition { get; set; }
        //Variables is similar to triggers, this can be a simple list, or a more complex variable object
        public Dictionary<string, string> variables { get; set; }
        //While not documented in official docs, stages can have pools
        public Pool pool { get; set; }
        public Job[] jobs { get; set; }
    }
}
