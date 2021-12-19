namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Parameter
    {
        public string name { get; set; }
        public string displayName { get; set; }
        public string type { get; set; }
        public string @default { get; set; }
        public string[] values { get; set; }
    }
}