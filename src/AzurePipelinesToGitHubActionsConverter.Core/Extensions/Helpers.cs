using System;
namespace AzurePipelinesToGitHubActionsConverter.Core.Extensions
{
    public static class Helpers
    {
        public static string[] Split(this string input, string sep)
        {
            return input.Split(new[] { sep }, StringSplitOptions.None);
        }
    }
}