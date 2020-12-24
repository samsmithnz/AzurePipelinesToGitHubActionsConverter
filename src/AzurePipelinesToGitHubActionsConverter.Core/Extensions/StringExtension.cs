using System;

namespace AzurePipelinesToGitHubActionsConverter.Core.Extensions
{
    public static class StringExtension
    {
        //.NET Standard 2.0 doesn't have a string split function for strings, so adding it in here.
        //.NET Standard 2.1 DOES have a string split function, but that reduces compatible frameworks (for example, .NET Framework is not compatible): (https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support)
        public static string[] Split(this string input, string sep)
        {
            return input.Split(new[] { sep }, StringSplitOptions.None);
        }
    }
}