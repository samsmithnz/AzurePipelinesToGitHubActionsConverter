using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //variables:
    //  buildConfiguration: Release

    public class Variables
    {
        public string buildConfiguration { get; set; }
        public int anotherVariable { get; set; }
        public string yetAnotherVariable { get; set; }
    }

}
