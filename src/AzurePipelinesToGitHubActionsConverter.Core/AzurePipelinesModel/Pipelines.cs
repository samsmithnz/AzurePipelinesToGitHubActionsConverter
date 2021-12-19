namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //pipelines:
    //- pipeline: string  # identifier for the pipeline resource
    //  project:  string # project for the build pipeline; optional input for current project
    //  source: string  # source pipeline definition name
    //  branch: string  # branch to pick the artifact, optional; defaults to all branches
    //  version: string # pipeline run number to pick artifact; optional; defaults to last successfully completed run
    //  trigger:     # optional; Triggers are not enabled by default.
    //    branches:  
    //      include: [string] # branches to consider the trigger events, optional; defaults to all branches.
    //      exclude: [string] # branches to discard the trigger events, optional; defaults to none. 
    //TODO: There is currently no conversion path for pipelines
    public class Pipelines
    {
        public string pipeline { get; set; }
        public string project { get; set; }
        public string source { get; set; }
        public string branch { get; set; }
        public string version { get; set; }
        public Trigger trigger { get; set; }
    }
}
