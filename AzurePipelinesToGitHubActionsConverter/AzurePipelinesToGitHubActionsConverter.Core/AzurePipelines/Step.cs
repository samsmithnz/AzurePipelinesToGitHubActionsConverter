using System.Collections.Generic;

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
        public string publish { get; set; }
        public string artifact { get; set; }
        public string displayName { get; set; }
        public string name { get; set; }
        public string condition { get; set; }
        public string continueOnError { get; set; }
        public string enabled { get; set; }
        public string timeoutInMinutes { get; set; }
        public string workingDirectory { get; set; }
        public string failOnStderr { get; set; }
        public Target target { get; set; }
        public Dictionary<string, string> inputs { get; set; }
        public Dictionary<string, string> env { get; set; }
    }
}
