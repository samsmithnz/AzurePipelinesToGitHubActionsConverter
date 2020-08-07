using System.Collections.Generic;
namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Stage
    {
        //stages:
        //- stage: Build
        //  displayName: 'Build/Test Stage'
        //  DemandsOn: PreBuild
        //  jobs:
        //  - job: Build
        //    displayName: 'Build job'
        //    pool:
        //      vmImage: $(vmImage)
        //    steps:
        public string stage { get; set; }
        public string displayName { get; set; }
        //Add Demandson processing for stages
        public string DemandsOn { get; set; }
        public string condition { get; set; }
        //Variables is similar to triggers, this can be a simple list, or a more complex variable object
        public List<Variable> variables { get; set; }
        public Job[] jobs { get; set; }
    }
}
