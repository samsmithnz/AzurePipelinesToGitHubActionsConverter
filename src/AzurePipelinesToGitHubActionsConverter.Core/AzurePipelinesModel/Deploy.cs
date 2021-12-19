namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Deploy
    {
        public Pool pool { get; set; }
        public Step[] steps { get; set; }
    }
}
