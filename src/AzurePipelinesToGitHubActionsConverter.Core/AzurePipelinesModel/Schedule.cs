namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //schedules:
    //- cron: string # cron syntax defining a schedule in UTC time
    //  displayName: string # friendly name given to a specific schedule
    //  branches:
    //    include: [ string ] # which branches the schedule applies to
    //    exclude: [ string ] # which branches to exclude from the schedule
    //  always: boolean # whether to always run the pipeline or only if there have been source code changes since the last successful scheduled run. The default is false.
    public class Schedule
    {
        public string cron { get; set; }
        public string displayName { get; set; }
        public IncludeExclude branches { get; set; }
        public bool always { get; set; }
    }
}
