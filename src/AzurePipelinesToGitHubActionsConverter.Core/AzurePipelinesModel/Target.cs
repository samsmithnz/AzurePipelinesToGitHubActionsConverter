namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Target
    {
        //target:
        //  container: string # where this step will run; values are the container name or the word 'host'
        //  commands: enum  # whether to process all logging commands from this step; values are `any` (default) or `restricted`

        //TODO: There is currently no conversion path for target
        public string container { get; set; }
        public string commands { get; set; }
    }
}
