using AzurePipelinesToGitHubActionsConverter.Core;
using System;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //            string input = @"
            //trigger:
            //- master
            //variables:
            //  buildConfiguration: Release
            //  vmImage: ubuntu-latest
            //jobs:
            //- job: Build
            //  displayName: Build job
            //  pool: 
            //    vmImage: ubuntu-latest
            //  variables:
            //    buildConfiguration: Debug
            //    myJobVariable: 'data'
            //    myJobVariable2: 'data2'
            //  steps: 
            //  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
            //    displayName: dotnet build part 1
            //- job: Build2
            //  displayName: Build job
            //  dependsOn: Build
            //  pool: 
            //    vmImage: ubuntu-latest
            //  variables:
            //    myJobVariable: 'data'
            //  steps:
            //  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
            //    displayName: dotnet build part 2
            //  - script: dotnet build WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj --configuration $(buildConfiguration) 
            //    displayName: dotnet build part 3";

            //            //Process the input
            //            Conversion conversion = new Conversion();
            //            ConversionResult gitHubOutput = conversion.ConvertAzurePipelineToGitHubAction(input);

            //            //Output the result
            //            Console.WriteLine("Azure Pipelines YAML: " + Environment.NewLine + input);
            //            Console.WriteLine(Environment.NewLine);
            //            Console.WriteLine("GitHub Actions YAML: " + Environment.NewLine + gitHubOutput.actionsYaml);

            //string expected = @"
            //- name: Run Selenium smoke tests on website
            //  run: |                            
            //    Write-Host ""Test1""
            //    Write-Host ""Test2""
            //   shell: powershell
            //";
            string expected = @"
-name: Run Selenium smoke tests on website
 shell: powershell
";

            Conversion conversion = new Conversion();
            Temp tmpObj = ReadYamlFile<Temp>(expected);
            string result = WriteYAMLFile<Temp>(tmpObj);
            Console.WriteLine("Result: " + Environment.NewLine + result);

        }

        //Read in a YAML file and convert it to a T object
        private static T ReadYamlFile<T>(string yaml)
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            T yamlObject = deserializer.Deserialize<T>(yaml);

            return yamlObject;
        }

        //Write a YAML file using the T object
        private static string WriteYAMLFile<T>(T obj)
        {
            //Convert the object into a YAML document
            ISerializer serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull) //New as of YamlDotNet 8.0.0: https://github.com/aaubry/YamlDotNet/wiki/Serialization.Serializer#configuredefaultvalueshandlingdefaultvalueshandling
                .Build();
            string yaml = serializer.Serialize(obj);

            return yaml;
        }
    }

    public class Temp
    {
        public string name { get; set; }
        public string run { get; set; }
        public string shell { get; set; }
    }
}
