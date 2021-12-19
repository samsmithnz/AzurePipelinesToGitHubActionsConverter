using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //containers:
    //- container: string  # identifier (A-Z, a-z, 0-9, and underscore)
    //  image: string  # container image name
    //  options: string  # arguments to pass to container at startup
    //  endpoint: string  # reference to a service connection for the private registry
    //  env: { string: string }  # list of environment variables to add
    //  ports: [ string ] # ports to expose on the container
    //  volumes: [ string ] # volumes to mount on the container
    public class Containers
    {
        //TODO: There is currently no conversion path for containers
        public string container { get; set; }
        public string image { get; set; }
        public string options { get; set; }
        public string endpoint { get; set; }
        public Dictionary<string, string> env { get; set; }
        public string[] ports { get; set; }
        public string[] volumes { get; set; }
    }
}
