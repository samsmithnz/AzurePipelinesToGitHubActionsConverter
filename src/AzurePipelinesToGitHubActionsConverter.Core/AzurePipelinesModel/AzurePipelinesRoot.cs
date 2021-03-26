//using System.Collections.Generic;

////#Example Pipeline YAML:
////trigger:
////- main

////pool:
////  vmImage: ubuntu-latest

////variables:
////  buildConfiguration: Release

////steps:
////- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj
////  displayName: dotnet build $(buildConfiguration)

//namespace AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines
//{
//    //name: string  # build numbering format
//    //resources:
//    //  containers: [ containerResource ]
//    //  repositories: [ repositoryResource ]
//    //variables: { string: string } | [ variable | templateReference ]
//    //trigger: trigger
//    //pr: pr
//    //stages: [ stage | templateReference ]

//    public class AzurePipelinesRoot<TTriggers, TVariables>
//    {
//        public string name { get; set; }

//        public Dictionary<string, string> parameters { get; set; }

//        public string container { get; set; }
//        public Resources resources { get; set; }

//        //Trigger is a complicated case, where it can be a simple list, or a more complex trigger object
//        //To solve this, we added a generic to try to convert to a string[], and failing that, try to convert with Trigger
//        //All outputs will return the complex version
//        //https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema#triggers
//        public TTriggers trigger { get; set; }
//        //public string[] trigger { get; set; }         
//        //public Trigger trigger { get; set; }

//        public Trigger pr { get; set; }
//        public Schedule[] schedules { get; set; }
//        public Pool pool { get; set; }

//        public Strategy strategy { get; set; }

//        //Variables is similar to triggers, this can be a simple list, or a more complex variable object
//        public TVariables variables { get; set; }
//        //public Dictionary<string, string> variables { get; set; }

//        public Stage[] stages { get; set; }
//        public Job[] jobs { get; set; }
//        public Step[] steps { get; set; }
//        //TODO: There is currently no conversion path for services
//        public Dictionary<string, string> services { get; set; }        
//    }
//}
