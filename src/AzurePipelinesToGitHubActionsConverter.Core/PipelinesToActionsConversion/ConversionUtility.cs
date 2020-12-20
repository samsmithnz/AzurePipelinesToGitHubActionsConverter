﻿using AzurePipelinesToGitHubActionsConverter.Core.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public static class ConversionUtility
    {
        public static string GenerateSpaces(int number)
        {
            return new String(' ', number);
        }

        // https://stackoverflow.com/questions/20411812/count-the-spaces-at-start-of-a-string
        public static int CountSpacesBeforeText(string input)
        {
            input = input.Replace(Environment.NewLine, "");
            return input.TakeWhile(char.IsWhiteSpace).Count();
        }

        public static string CleanYamlBeforeDeserializationV2(string yaml)
        {
            string processedYaml = yaml;

            //Process conditional insertions/ variables
            if (processedYaml.IndexOf("{{#if") >= 0 || processedYaml.IndexOf("{{ #if") >= 0 ||
                processedYaml.IndexOf("${{if") >= 0 || processedYaml.IndexOf("${{ if") >= 0)
            {
                StringBuilder sb = new StringBuilder();
                int spacePrefixCount = 0;
                foreach (string line in processedYaml.Split(System.Environment.NewLine))
                {
                    if (line.IndexOf("{{#if") >= 0 || line.IndexOf("{{ #if") >= 0 ||
                        line.IndexOf("${{if") >= 0 || line.IndexOf("${{ if") >= 0)
                    {
                        //don't add line, we want to remove it, but track the spaces
                        spacePrefixCount = ConversionUtility.CountSpacesBeforeText(line);
                    }
                    else if (line.IndexOf("{{/if") >= 0) //ending if 
                    {
                        //don't add line, remove
                        spacePrefixCount = 0;
                    }
                    else
                    {
                        //DANGER WILL ROBINSON - DANGER 
                        //This is meant for variables, but may affect much more than it should
                        int currentLinespaceFrefixCount = ConversionUtility.CountSpacesBeforeText(line);
                        if (spacePrefixCount > 0 && spacePrefixCount == (currentLinespaceFrefixCount - 2))
                        {
                            //Correct the location. For example:
                            //    var1: value1
                            //becomes:
                            //  var1: value1
                            sb.Append(GenerateSpaces(spacePrefixCount));
                            sb.Append(line.Trim());
                        }
                        else if (spacePrefixCount > 0 && spacePrefixCount > (currentLinespaceFrefixCount - 2))
                        {
                            spacePrefixCount = 0;
                            sb.Append(line);
                        }
                        else
                        {
                            sb.Append(line);
                        }
                        sb.Append(System.Environment.NewLine);
                    }
                }
                processedYaml = sb.ToString();
            }

            return processedYaml;

        }

        // Some elements have a simple, same line string, we need to make into a list
        // for example "trigger:none", becomes "trigger:\n\r- none"
        // This is a lot simplier in JSON, as it's already only returning the none string.
        public static string ProcessNoneJsonElement(string yaml)
        {
            if (yaml == "none")
            {
                return "[ none ]";
            }
            else
            {
                return yaml;
            }
        }

        public static string ConvertMessageToYamlComment(string message)
        {
            //Append a comment to the message if one doesn't already exist
            if (message.TrimStart().StartsWith("#") == false)
            {
                message = "#" + message;
            }
            return message;
        }

        //Add a steps parent, to allow the processing of an individual step to proceed
        public static string StepsPreProcessing(string input)
        {
            //If the step isn't wrapped in a "steps:" node, we need to add this, so we can process the step
            if (input.Trim().StartsWith("steps:") == false && input.Trim().Length > 0)
            {
                //we need to add steps, before we do, we need to see if the task needs an indent
                string[] stepLines = input.Split(System.Environment.NewLine);
                if (stepLines.Length > 0)
                {
                    int i = 0;
                    //Search for the first non empty line
                    while (string.IsNullOrEmpty(stepLines[i].Trim()) == true)
                    {
                        i++;
                    }
                    if (stepLines[i].Trim().StartsWith("-") == true)
                    {
                        int indentLevel = stepLines[i].IndexOf("-");
                        indentLevel += 2;
                        string buffer = ConversionUtility.GenerateSpaces(indentLevel);
                        StringBuilder newInput = new StringBuilder();
                        foreach (string line in stepLines)
                        {
                            newInput.Append(buffer);
                            newInput.Append(line);
                            newInput.Append(System.Environment.NewLine);
                        }
                        input = newInput.ToString();
                    }

                    input = "steps:" + System.Environment.NewLine + input;
                }
            }
            return input;
        }


        //Used by jobs and stages
        public static string GenerateJobName(AzurePipelines.Job job, int currentIndex)
        {
            //Get the job name
            string jobName = job.job;
            if (jobName == null && job.deployment != null)
            {
                jobName = job.deployment;
            }
            if (jobName == null && job.template != null)
            {
                jobName = "Template";
            }
            if (string.IsNullOrEmpty(jobName) == true)
            {
                jobName = "job" + currentIndex.ToString();
            }
            return jobName;
        }

        public static void WriteLine(string message, bool verbose)
        {
            if (verbose == true)
            {
                Debug.WriteLine(message);
            }
        }
    }
}
