using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //trigger:
    //  batch: true
    //  branches:
    //    include:
    //    - features/*
    //    exclude:
    //    - features/experimental/*
    //  paths:
    //    exclude:
    //    - README.md
    public class Trigger
    {
        //TODO: Investigate where the batch property is in actions
        public string batch { get; set; }
        public IncludeExclude branches { get; set; }
        public IncludeExclude paths { get; set; }
    }
}
