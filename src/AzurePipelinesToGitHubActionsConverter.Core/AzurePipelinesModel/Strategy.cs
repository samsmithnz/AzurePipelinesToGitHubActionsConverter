using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    public class Strategy
    {
        //strategy:
        //  parallel: # parallel strategy, see below
        //  matrix: # matrix strategy, see below
        //  maxParallel: number # maximum number of matrix jobs to run simultaneously

        //strategy:
        //  matrix:
        //    unit_test_linux:
        //      imageName: 'ubuntu-16.04'
        //      TYPE: 'unit'
        //    cucumber:
        //      imageName: 'ubuntu-16.04'
        //      TYPE: 'cucumber'

        //strategy:
        //  matrix:
        //    linux:
        //      imageName: "ubuntu-16.04"
        //    mac:
        //      imageName: "macos-10.13"
        //    windows:
        //      imageName: "vs2017-win2016"

        //Note that Parallel doesn't seem to currently have an equivalent in Actions
        public string parallel { get; set; }
        public Dictionary<string, Dictionary<string, string>> matrix { get; set; }
        public string maxParallel { get; set; }

        public RunOnce runOnce { get; set; }
        public Canary canary { get; set; }
        public Rolling rolling { get; set; }
    }
}
