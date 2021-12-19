namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema%2Cparameter-schema#environment
    //environment:                # create environment and/or record deployments
    //  name: string              # name of the environment to run this job on.
    //  resourceName: string      # name of the resource in the environment to record the deployments against
    //  resourceId: number        # resource identifier
    //  resourceType: string      # type of the resource you want to target. Supported types - virtualMachine, Kubernetes
    //  tags: string | [ string ] # tag names to filter the resources in the environment
    //
    //Technically can also be a simple string, but we replace that in the intial conversion
    //environment: environmentName.resourceName

    public class Environment
    {
        public string name { get; set; }
        public string resourceName { get; set; }
        public string resourceId { get; set; }
        public string resourceType { get; set; }
        public string[] tags { get; set; }
    }
}
