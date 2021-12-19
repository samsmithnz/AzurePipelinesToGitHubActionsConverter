namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class RunOnce
    {
        public Deploy preDeploy { get; set; }
        public Deploy deploy { get; set; }
        public Deploy routeTraffic { get; set; }
        public Deploy postRouteTraffic { get; set; }
        public On on { get; set; }
    }
}
