using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
//environment:                # create environment and/or record deployments
//  name: string              # name of the environment to run this job on.
//  resourceName: string      # name of the resource in the environment to record the deployments against
//  resourceId: number        # resource identifier
//  resourceType: string      # type of the resource you want to target. Supported types - virtualMachine, Kubernetes
//  tags: string | [ string ] # tag names to filter the resources in the environment

//Technically can also be a simple string, but we replace that in the intial conversion

//environment: environmentName.resourceName


    public class JobEnvironment
    {
        //TODO: add code to process containers
        public string container { get; set; }
        public string image { get; set; }
        public string options { get; set; }
        public string endpoint { get; set; }
        public Dictionary<string, string> env { get; set; }
        public string[] ports { get; set; }
        public string[] volumes { get; set; }
    }
}
