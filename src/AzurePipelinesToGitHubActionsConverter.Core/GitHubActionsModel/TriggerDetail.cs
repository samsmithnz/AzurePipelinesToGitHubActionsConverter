namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class TriggerDetail
    {
        //    branches:    
        //    - main
        //    - Develop
        //    branches-ignore:
        //    - 'mona/octocat'
        //    paths:
        //    - '**.js'
        //    paths-ignore:
        //    - 'docs/**'
        //    tags:        
        //    - v1             
        //    - v1.*           
        //    tags-ignore:        
        //    - v1             
        //    - v1.*           
        public string[] branches { get; set; }
        public string[] branches_ignore { get; set; }
        public string[] paths { get; set; }
        public string[] paths_ignore { get; set; }
        public string[] tags { get; set; }
        public string[] tags_ignore { get; set; }
    }
}
