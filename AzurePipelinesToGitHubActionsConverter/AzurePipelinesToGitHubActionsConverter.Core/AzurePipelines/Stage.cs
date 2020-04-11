namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Stage
    {
        //stages:
        //- stage: Build
        //  displayName: 'Build/Test Stage'
        //  dependsOn: PreBuild
        //  jobs:
        //  - job: Build
        //    displayName: 'Build job'
        //    pool:
        //      vmImage: $(vmImage)
        //    steps:
        public string stage { get; set; }
        public string displayName { get; set; }
        //Add dependson processing for stages
        public string dependsOn { get; set; }
        public string condition { get; set; }
        public Job[] jobs { get; set; }
    }
}
