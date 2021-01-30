namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema%2Cparameter-schema#resources
    //resources:
    //  repositories:
    //  - repository: string  # identifier (A-Z, a-z, 0-9, and underscore)
    //    type: enum  # see the following "Type" topic
    //    name: string  # repository name (format depends on `type`)
    //    ref: string  # ref name to use; defaults to 'refs/heads/main'
    //    endpoint: string  # name of the service connection to use (for types that aren't Azure Repos)
    //    trigger:  # CI trigger for this repository, no CI trigger if skipped (only works for Azure Repos)
    //      branches:
    //        include: [ string ] # branch names which will trigger a build
    //        exclude: [ string ] # branch names which will not
    //      tags:
    //        include: [ string ] # tag names which will trigger a build
    //        exclude: [ string ] # tag names which will not
    //      paths:
    //        include: [ string ] # file paths which must match to trigger a build
    //        exclude: [ string ] # file paths which will not trigger a build
    public class Repositories
    {
        public string repository { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        //as "ref" is a reserved word in C#, added an "_", and remove this "_" when serializing
        public string _ref { get; set; }
        public string endpoint { get; set; }
        public string connection { get; set; }
        public string source { get; set; }
    }
}
