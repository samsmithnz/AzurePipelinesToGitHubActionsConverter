namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //trigger:
    //  batch: boolean # batch changes if true (the default); start a new build for every push if false
    //  branches:
    //    include: [ string ] # branch names which will trigger a build
    //    exclude: [ string ] # branch names which will not
    //  tags:
    //    include: [ string ] # tag names which will trigger a build
    //    exclude: [ string ] # tag names which will not
    //  paths:
    //    include: [ string ] # file paths which must match to trigger a build
    //    exclude: [ string ] # file paths which will not trigger a build
    public class Trigger
    {
        public bool? batch { get; set; } = null; //Note: There is no batch property in actions
        public bool? autoCancel { get; set; } = null; //Note: There is no autoCancel property in actions
        public IncludeExclude branches { get; set; }
        public IncludeExclude tags { get; set; }
        public IncludeExclude paths { get; set; }
    }
}
