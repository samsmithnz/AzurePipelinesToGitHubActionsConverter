using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
  //repositories:
  //- repository: string  # identifier (A-Z, a-z, 0-9, and underscore)
  //  type: enum  # see below
  //  name: string  # repository name (format Demands on `type`)
  //  ref: string  # ref name to use, defaults to 'refs/heads/master'
  //  endpoint: string  # name of the service connection to use (for non-Azure Repos types)
    public class Repositories
    {
        //TODO: Add code to process repositories
        public string repository { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        //as "ref" is a reserved word in C#, added an "_", and remove this "_" when serializing
        public string _ref { get; set; } 
        public string endpoint { get; set; }
        public string connection { get; set; }
        public string source { get; set; }
    }
}
