using AzurePipelinesToGitHubActionsConverter.Core;
using System;

namespace AzurePipelinesToGitHubActionsConverter.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "" + Environment.NewLine +
                "trigger:" + Environment.NewLine +
                "- master" + Environment.NewLine +
                "pool:" + Environment.NewLine +
                "  vmImage: 'ubuntu-latest'" + Environment.NewLine +
                "variables:" + Environment.NewLine +
                "  buildConfiguration: 'Release'" + Environment.NewLine +
                "steps:" + Environment.NewLine +
                "- script: dotnet build --configuration $(buildConfiguration) WebApplication1/WebApplication1.Service/WebApplication1.Service.csproj" + Environment.NewLine +
                "  displayName: 'dotnet build $(buildConfiguration)'";

            //Process the input
            Conversion conversion = new Conversion();
            string output = conversion.ConvertAzurePipelineToGitHubAction(input);

            //Output the result
            Console.WriteLine("Azure Pipelines YAML: " + Environment.NewLine + input);
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("GitHub Actions YAML: " + Environment.NewLine + output);
        }
    }
}
