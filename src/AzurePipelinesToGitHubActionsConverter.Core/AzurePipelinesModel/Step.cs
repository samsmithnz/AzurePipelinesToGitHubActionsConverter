using System.Collections.Generic;
using System.ComponentModel;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema#steps
    public class Step
    {
        //- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
        //  displayName: dotnet build $(buildConfiguration)

        //steps: [script | bash | pwsh | powershell | checkout | task | templateReference]

        private string _script = null;
        public string script {
            get {
                return _script;
            }
            set {
                //Spaces on the beginning or end seem to be a problem for the YAML serialization
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Trim();
                }
                _script = value;
            }
        }
        private string _bash = null;
        public string bash {
            get {
                return _bash;
            }
            set {
                //Spaces on the beginning or end seem to be a problem for the YAML serialization
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Trim();
                }
                _bash = value;
            }
        }
        private string _pwsh = null;
        public string pwsh {
            get {
                return _pwsh;
            }
            set {
                //Spaces on the beginning or end seem to be a problem for the YAML serialization
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Trim();
                }
                _pwsh = value;
            }
        }
        private string _powershell = null;
        public string powershell {
            get {
                return _powershell;
            }
            set {
                //Spaces on the beginning or end seem to be a problem for the YAML serialization
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.Trim();
                }
                _powershell = value;
            }
        }
        public string checkout { get; set; }
        public string task { get; set; }
        public string template { get; set; }
        public string publish { get; set; }
        public string download { get; set; }
        public string artifact { get; set; }
        public string patterns { get; set; }
        public string displayName { get; set; }
        public string name { get; set; }
        public string condition { get; set; }
        [DefaultValue(false)]
        public bool continueOnError { get; set; } = false;
        [DefaultValue(true)]
        public bool enabled { get; set; } = true;
        public int timeoutInMinutes { get; set; }
        public string workingDirectory { get; set; }
        public string failOnStderr { get; set; }
        public Target target { get; set; }
        public Dictionary<string, string> inputs { get; set; }
        public Dictionary<string, string> env { get; set; }
        public Dictionary<string, string> parameters { get; set; }
        public string clean { get; set; }
        public string fetchDepth { get; set; }
        public string fetchTags { get; set; }
        public string lfs { get; set; }
        public string submodules { get; set; }
        public string path { get; set; }
        public string persistCredentials { get; set; }

    }
}
