using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Workspace
    {
        //workspace:
        //  clean: outputs | resources | all # what to clean up before the job runs

        //TODO: Work out the equivalent conversion for clean in GitHub Actions
        public string clean { get; set; }
    }
}
