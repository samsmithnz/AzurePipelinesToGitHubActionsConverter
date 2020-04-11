using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema#resources
    public class Resources
    {
        //TODO: Add code to process resources
        public Pipelines[] pipelines { get; set; }
        public Repositories[] repositories { get; set; }
        public Containers[] containers { get; set; }
    }
}
