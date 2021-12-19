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
    public class Variable
    {
        public string name { get; set; }
        public string value { get; set; }
        //TODO: There is currently no conversion path for groups and templates
        public string group { get; set; }
        public string template { get; set; }
        public bool @readonly { get; set; }
    }
}
