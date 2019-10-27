using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Tests
{
    public static class TestUtility
    {
        public static string TrimNewLines(string input)
        {
            //Trim off any leading of trailing new lines 
            input = input.TrimStart('\r', '\n');
            input = input.TrimEnd('\r', '\n');

            return input;
        }

    }
}
