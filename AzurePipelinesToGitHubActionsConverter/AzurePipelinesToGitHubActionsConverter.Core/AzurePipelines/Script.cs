using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Script
    {
        //- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
        //  displayName: dotnet build $(buildConfiguration)
        public string script { get; set; }
        public string displayName { get; set; }
    }
}
