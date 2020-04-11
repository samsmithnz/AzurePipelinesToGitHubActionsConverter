using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
{
    //variables:
    //# a regular variable
    //- name: myvariable
    //  value: myvalue
    //# a variable group
    //- group: myvariablegroup
    //# a reference to a variable template
    //- template: myvariabletemplate.yml
    public class Variables
    {
        public string name { get; set; }
        public string value { get; set; }
        //TODO: What to do with groups and templates in actions?
        public string group { get; set; }
        public string template { get; set; }
    }
}
