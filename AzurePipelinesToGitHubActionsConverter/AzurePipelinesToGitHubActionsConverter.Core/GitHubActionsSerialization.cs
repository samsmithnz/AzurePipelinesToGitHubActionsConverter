using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public static class GitHubActionsSerialization
    {
        public static GitHubActionsRoot Deserialize(string yaml)
        {
            return DeserializeGitHubActionsYaml(yaml);
        }

        public static string Serialize(GitHubActionsRoot gitHubActions, List<string> variableList = null, string matrixVariableName = null)
        {
            string yaml = Global.SerializeYaml<GitHubActionsRoot>(gitHubActions);

            yaml = ProcessGitHubActionYAML(yaml, variableList, matrixVariableName);

            return yaml;
        }

        public static string SerializeJob(GitHubActions.Job gitHubActionJob, List<string> variableList = null)
        {
            string yaml = Global.SerializeYaml<GitHubActions.Job>(gitHubActionJob);

            yaml = ProcessGitHubActionYAML(yaml, variableList);

            yaml = StepsPostProcessing(yaml);

            return yaml;
        }

        private static string ProcessGitHubActionYAML(string yaml, List<string> variableList = null, string matrixVariableName = null)
        {
            //Fix some variables for serialization, the '-' character is not valid in property names, and some of the YAML standard uses reserved words (e.g. if)
            yaml = PrepareYamlPropertiesForGitHubSerialization(yaml);

            //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
            if (variableList != null)
            {
                yaml = PrepareYamlVariablesForGitHubSerialization(yaml, variableList, matrixVariableName);
            }

            //Trim off any leading of trailing new lines 
            yaml = yaml.TrimStart('\r', '\n');
            yaml = yaml.TrimEnd('\r', '\n');

            return yaml;
        }

        private static GitHubActionsRoot DeserializeGitHubActionsYaml(string yaml)
        {
            //Fix some variables that we can't use for property names because the - character is not allowed or it's a reserved word (e.g. if)
            yaml = yaml.Replace("runs-on", "runs_on");
            yaml = yaml.Replace("if", "_if");
            yaml = yaml.Replace("timeout-minutes", "timeout_minutes");
            yaml = yaml.Replace("pull-request", "pull_request");
            yaml = yaml.Replace("branches-ignore", "branches_ignore");
            yaml = yaml.Replace("paths-ignore", "paths_ignore");
            yaml = yaml.Replace("tags-ignore", "tags_ignore");
            yaml = yaml.Replace("max-parallel", "max_parallel");
            yaml = yaml.Replace("ref", "_ref");

            return Global.DeserializeYaml<GitHubActionsRoot>(yaml);
        }

        private static string PrepareYamlPropertiesForGitHubSerialization(string yaml)
        {
            //Fix system variables
            yaml = yaml.Replace("$(build.artifactstagingdirectory)", "${GITHUB_WORKSPACE}");

            //Fix some variables that we can't use for property names because the - character is not allowed or it's a reserved word (e.g. if)
            yaml = yaml.Replace("runs_on", "runs-on");
            yaml = yaml.Replace("_if", "if");
            yaml = yaml.Replace("timeout_minutes", "timeout-minutes");
            yaml = yaml.Replace("pull_request", "pull-request");
            yaml = yaml.Replace("branches_ignore", "branches-ignore");
            yaml = yaml.Replace("paths_ignore", "paths-ignore");
            yaml = yaml.Replace("tags_ignore", "tags-ignore");
            yaml = yaml.Replace("max_parallel", "max-parallel");
            yaml = yaml.Replace("_ref", "ref");
            yaml = yaml.Replace("step_message", "#");
            yaml = yaml.Replace("job_message", "#");

            //HACK: Sometimes when generating  yaml, a weird ">+" string appears. Not sure why yet, replacing it out of there for short term
            yaml = yaml.Replace("run: >-", "run: |"); //Replace a weird artifact in scripts when converting pipes
            yaml = yaml.Replace("run: >2-\r\n     |", "run: |");
            yaml = yaml.Replace("run: >2-\r\n         |", "run: |");
            yaml = yaml.Replace("run: 2-\r\n         |", "run: |");
            yaml = yaml.Replace("run: 2-\r\n         |", "run: |");
            yaml = yaml.Replace("run: >+", "run: ");
            yaml = yaml.Replace("run: >", "run: ");

            return yaml;
        }

        private static string PrepareYamlVariablesForGitHubSerialization(string yaml, List<string> variableList, string matrixVariableName = null)
        {
            if (matrixVariableName != null)
            {
                variableList.Add(matrixVariableName);
            }

            foreach (string item in variableList)
            {
                if (item == matrixVariableName)
                {
                    yaml = yaml.Replace("$(" + item + ")", "${{ matrix." + item + " }}");
                    yaml = yaml.Replace("$( " + item + " )", "${{ matrix." + item + " }}");
                }
                else
                {
                    //Replace variables with the format "$(MyVar)" with the format "$MyVar"
                    yaml = yaml.Replace("$(" + item + ")", "${{ env." + item + " }}");
                    yaml = yaml.Replace("$( " + item + " )", "${{ env." + item + " }}");
                    yaml = yaml.Replace("$(" + item + " )", "${{ env." + item + " }}");
                    yaml = yaml.Replace("$( " + item + ")", "${{ env." + item + " }}");
                    yaml = yaml.Replace("$" + item + "", "${{ env." + item + " }}");
                }
            }

            return yaml;
        }

        //Strip the steps off to focus on just the individual step
        private static string StepsPostProcessing(string input)
        {
            if (input.Trim().StartsWith("steps:") == true)
            {
                //we need to remove steps, before we do, we need to see if the task needs to remove indent
                string[] stepLines = input.Split(Environment.NewLine);
                if (stepLines.Length > 0)
                {
                    int i = 0;
                    //Search for the first non empty line
                    while (string.IsNullOrEmpty(stepLines[i].Trim()) == true || stepLines[i].Trim().StartsWith("steps:") == true)
                    {
                        i++;
                    }
                    if (stepLines[i].StartsWith("-") == true)
                    {
                        int indentLevel = stepLines[i].IndexOf("-");
                        if (indentLevel >= 2)
                        {
                            indentLevel -= 2;
                        }
                        string buffer = Global.GenerateSpaces(indentLevel);
                        StringBuilder newInput = new StringBuilder();
                        foreach (string line in stepLines)
                        {
                            if (line.Trim().StartsWith("steps:") == false)
                            {
                                newInput.Append(buffer);
                                newInput.Append(line);
                                newInput.Append(Environment.NewLine);
                            }
                        }
                        input = newInput.ToString();
                    }
                }
                input = input.TrimEnd('\r', '\n');
            }

            return input;

        }

    }
}
