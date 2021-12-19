namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Workspace
    {
        //workspace:
        //  clean: outputs | resources | all # what to clean up before the job runs

        //TODO: There is currently no conversion path for clean
        public string clean { get; set; }
    }
}
