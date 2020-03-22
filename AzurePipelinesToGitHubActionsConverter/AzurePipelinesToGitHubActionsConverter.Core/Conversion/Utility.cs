using System;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public static class Utility
    {
        public static string GenerateSpaces(int number)
        {
            return new String(' ', number);
        }
    }
}
