using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class AzurePipelinesRoot
    {
        public string[] trigger { get; set; }
        public Pool pool { get; set; }
        public Variables variables { get; set; }
        public Script[] steps { get; set; }
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
