namespace AzurePipelinesToGitHubActionsConverter.Core.GitHubActions
{
    public class Trigger
    {
        //on:
        //  push:
        //    branches:    
        //    - main
        //    - Develop
        //    branches-ignore:
        //    - 'mona/octocat'
        //    paths:
        //    - '**.js'
        //    paths-ignore:
        //    - 'docs/**'
        //  pull-request
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
        //    - v1             # Push events to v1 tag
        //    - v1.*           # Push events to v1.0, v1.1, and v1.9 tags
        //  schedule:
        //  - cron:  '*/15 * * * *' # * is a special character in YAML so you have to quote this string
        //  workflow_dispatch: # Allows you to run this workflow manually from the Actions tab

        public TriggerDetail push { get; set; }
        public TriggerDetail pull_request { get; set; }
        public string[] schedule { get; set; }
        public string workflow_dispatch { get; set; }
    }
}
