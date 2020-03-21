using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public static class GenericObjectSerialization
    {
        public static object Deserialize(string yaml)
        {
            return Global.DeserializeYaml<object>(yaml);
        }

    }
}
