using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    //    container:
    //      image: node:10.16-jessie
    //      env:
    //        NODE_ENV: development
    //      ports:
    //        - 80
    //      volumes:
    //        - my_docker_volume:/volume_mount
    //      options: --cpus 1
    public class Container
    {
        public string image { get; set; }
        public Dictionary<string, string> env { get; set; }
        public string[] ports { get; set; }
        public string[] volumes { get; set; }
        public string options { get; set; }
    }
}
