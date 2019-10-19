using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema#steps
    public class Step
    {
        //- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
        //  displayName: dotnet build $(buildConfiguration)

        //steps: [script | bash | pwsh | powershell | checkout | task | templateReference]

        public string script { get; set; }
        public string bash { get; set; }
        public string pwsh { get; set; }
        public string powershell { get; set; }
        //public string checkout { get; set; }
        public string task { get; set; }
        //public string templateReference { get; set; }
        
        public string displayName { get; set; }
        public Dictionary<string, string> inputs { get; set; }
    }
}
