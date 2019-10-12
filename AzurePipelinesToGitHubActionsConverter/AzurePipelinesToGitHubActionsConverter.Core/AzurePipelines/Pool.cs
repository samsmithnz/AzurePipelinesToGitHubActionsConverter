using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //pool:
    //  vmImage: ubuntu-latest

    public class Pool
    {
        public string vmImage { get; set; }
    }


}
