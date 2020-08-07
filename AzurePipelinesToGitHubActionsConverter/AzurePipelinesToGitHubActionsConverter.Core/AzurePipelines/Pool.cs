namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //pool: 
    //  name: string  # name of the pool to run this job in
    //  demands: string # see the following "Demands" topic
    //  vmImage: string # name of the VM image you want to use; valid only in the Microsoft-hosted pool

    //OR

    //pool:
    //  name: string  # name of the pool to run this job in
    //  demands: [ string ] # see the following "Demands" topic
    //  vmImage: string # name of the VM image you want to use; valid only in the Microsoft-host

    //Note that there is a 3rd variation, that is just a Pool name (e.g. a string)
    public class Pool
    {
        public string vmImage { get; set; }
        public string name { get; set; }
        public string[] demands { get; set; }
    }

}
