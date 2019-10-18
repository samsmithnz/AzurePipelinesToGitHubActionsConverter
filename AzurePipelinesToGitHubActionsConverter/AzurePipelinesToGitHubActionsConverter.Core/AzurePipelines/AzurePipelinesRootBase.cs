using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //name: string  # build numbering format
    //resources:
    //  containers: [ containerResource ]
    //  repositories: [ repositoryResource ]
    //variables: { string: string } | [ variable | templateReference ]
    //trigger: trigger
    //pr: pr
    //stages: [ stage | templateReference ]

    public abstract class AzurePipelinesRootBase<T>
    {
        public string name { get; set; }
        //TODO: add resources support
        //public string resources { get; set; }
        //TODO: Add all trigger options
        public T trigger { get; set; }
        //public string[] trigger { get; set; } //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema#triggers
        //public Trigger trigger { get; set; }
        //TODO: add pr support
        //public string[] pr { get; set; }
        public Pool pool { get; set; }
        public Dictionary<string, string> variables { get; set; }
        public Stage[] stages { get; set; }
        public Job[] jobs { get; set; }
        public Step[] steps { get; set; }
    }
}

//trigger:
//- master

//pool:
//  vmImage: ubuntu-latest

//variables:
//  buildConfiguration: Release

//steps:
//- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
//  displayName: dotnet build $(buildConfiguration)
