namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //pool:
    //  vmImage: ubuntu-latest
    public class Pool
    {
        public string vmImage { get; set; }
        public string name { get; set; }
        public string demands { get; set; }
    }

}
