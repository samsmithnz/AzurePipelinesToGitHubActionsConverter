using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    //Azure Pipelines YAML
    //pool:
    //  vmImage: 'ubuntu-latest'

    //GitHub Actions YAML
    //    runs-on: ubuntu-latest
    public class Pool
    {
        string VmImage { get; set; }
    }


}
