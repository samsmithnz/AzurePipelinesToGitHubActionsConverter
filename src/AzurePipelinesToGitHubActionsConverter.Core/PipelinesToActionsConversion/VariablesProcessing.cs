using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using GitHubActionsDotNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GitHubActions = GitHubActionsDotNet.Models;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public class VariablesProcessing
    {
        private readonly bool _verbose;
        public List<string> VariableList { get; set; }
        public VariablesProcessing(bool verbose)
        {
            _verbose = verbose;
            VariableList = new List<string>();
        }

        //process all (simple) variables
        public Dictionary<string, string> ProcessSimpleVariables(Dictionary<string, string> variables)
        {
            if (variables != null)
            {
                //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
                foreach (string item in variables.Keys)
                {
                    VariableList.Add(item);
                }
            }

            return variables;
        }

        public Dictionary<string, string> ProcessComplexVariablesV2(List<AzurePipelines.Variable> variables)
        {
            Dictionary<string, string> processedVariables = new Dictionary<string, string>();
            if (variables != null)
            {
                //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
                for (int i = 0; i < variables.Count; i++)
                {
                    //name/value pairs
                    if (variables[i].name != null && variables[i].value != null)
                    {
                        processedVariables.Add(variables[i].name, variables[i].value);
                        VariableList.Add(variables[i].name);
                    }
                    //groups
                    if (variables[i].group != null)
                    {
                        if (!processedVariables.ContainsKey("group"))
                        {
                            processedVariables.Add("group", variables[i].group);
                        }
                        else
                        {
                            ConversionUtility.WriteLine("group: only 1 variable group is supported at present", _verbose);
                        }
                    }
                    //template
                    if (variables[i].template != null)
                    {
                        processedVariables.Add("template", variables[i].template);
                    }
                }
            }
            return processedVariables;
        }

        public Dictionary<string, string> ProcessComplexParametersV2(List<AzurePipelines.Parameter> parameter)
        {
            Dictionary<string, string> processedVariables = new Dictionary<string, string>();
            if (parameter != null)
            {
                //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
                for (int i = 0; i < parameter.Count; i++)
                {
                    //name/value pairs
                    if (parameter[i].name != null)
                    {
                        if (parameter[i].@default == null)
                        {
                            parameter[i].@default = "";
                        }
                        processedVariables.Add(parameter[i].name, parameter[i].@default);
                        VariableList.Add(parameter[i].name);
                    }
                }
            }
            return processedVariables;
        }


        public Dictionary<string, string> ProcessParametersAndVariablesV2(string parametersYaml, string variablesYaml)
        {
            List<Parameter> parameters = null;
            if (parametersYaml != null)
            {
                try
                {
                    Dictionary<string, string> simpleParameters = YamlSerialization.DeserializeYaml<Dictionary<string, string>>(parametersYaml);
                    parameters = new List<Parameter>();
                    foreach (KeyValuePair<string, string> item in simpleParameters)
                    {
                        parameters.Add(new Parameter
                        {
                            name = item.Key,
                            @default = item.Value
                        });
                    }
                }
                catch (Exception ex)
                {
                    ConversionUtility.WriteLine($"DeserializeYaml<Dictionary<string, string>>(parametersYaml) swallowed an exception: " + ex.Message, _verbose);
                    parameters = YamlSerialization.DeserializeYaml<List<Parameter>>(parametersYaml);
                }
            }

            List<Variable> variables = null;
            if (variablesYaml != null)
            {
                try
                {
                    Dictionary<string, string> simpleVariables = YamlSerialization.DeserializeYaml<Dictionary<string, string>>(variablesYaml);
                    variables = new List<Variable>();
                    foreach (KeyValuePair<string, string> item in simpleVariables)
                    {
                        variables.Add(new Variable
                        {
                            name = item.Key,
                            value = item.Value
                        });
                    }
                }
                catch (Exception ex)
                {
                    ConversionUtility.WriteLine($"DeserializeYaml<Dictionary<string, string>>(variablesYaml) swallowed an exception: " + ex.Message, _verbose);
                    variables = YamlSerialization.DeserializeYaml<List<Variable>>(variablesYaml);
                }
            }

            Dictionary<string, string> env = new Dictionary<string, string>();
            Dictionary<string, string> processedParameters = ProcessComplexParametersV2(parameters);
            Dictionary<string, string> processedVariables = ProcessComplexVariablesV2(variables);
            foreach (KeyValuePair<string, string> item in processedParameters)
            {
                if (!env.ContainsKey(item.Key))
                {
                    env.Add(item.Key, item.Value);
                }
            }
            foreach (KeyValuePair<string, string> item in processedVariables)
            {
                if (!env.ContainsKey(item.Key))
                {
                    env.Add(item.Key, item.Value);
                }
            }

            if (env.Count > 0)
            {
                return env;
            }
            else
            {
                return null;
            }
        }

        public List<string> SearchForVariables(string input)
        {
            List<string> variableList = new List<string>();

            if (input != null)
            {
                string[] stepLines = input.Split(System.Environment.NewLine);
                foreach (string line in stepLines)
                {
                    List<string> variableResults = FindPipelineVariablesInString(line);
                    variableResults.AddRange(FindPipelineParametersInString(line));
                    if (variableResults.Count > 0)
                    {
                        variableList.AddRange(variableResults);
                    }
                }
            }

            return variableList;
        }

        //Search GitHub object for all environment variables
        public List<string> SearchForVariablesV2(GitHubActions.GitHubActionsRoot gitHubActions)
        {
            List<string> variables = new List<string>();
            if (gitHubActions.env != null)
            {
                foreach (KeyValuePair<string, string> env in gitHubActions.env)
                {
                    variables.Add(env.Key);
                }
            }
            if (gitHubActions.jobs != null)
            {
                foreach (KeyValuePair<string, GitHubActions.Job> job in gitHubActions.jobs)
                {
                    if (job.Value.env != null)
                    {
                        foreach (KeyValuePair<string, string> env in job.Value.env)
                        {
                            variables.Add(env.Key);
                        }
                    }
                }
            }
            return variables;
        }

        private List<string> FindPipelineVariablesInString(string text)
        {
            //Used https://stackoverflow.com/questions/378415/how-do-i-extract-text-that-lies-between-parentheses-round-brackets
            //With the addition of the \$ search to capture strings like: "$(variable)"
            //\$\(           # $ char and escaped parenthesis, means "starts with a '$(' character"
            //    (          # Parentheses in a regex mean "put (capture) the stuff 
            //               #     in between into the Groups array" 
            //       [^)]    # Any character that is not a ')' character
            //       *       # Zero or more occurrences of the aforementioned "non ')' char"
            //    )          # Close the capturing group
            //\)             # "Ends with a ')' character"  
            MatchCollection results = Regex.Matches(text, @"\$\(([^)]*)\)");
            List<string> list = results.Cast<Match>().Select(match => match.Value).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                string item = list[i];

                //Remove leading "$(" and trailing ")"
                if (list[i].Length > 3)
                {
                    list[i] = list[i].Substring(0, item.Length - 1);
                    list[i] = list[i].Remove(0, 2);
                }
            }

            return list;
        }

        private List<string> FindPipelineParametersInString(string text)
        {
            //Used https://stackoverflow.com/questions/378415/how-do-i-extract-text-that-lies-between-parentheses-round-brackets
            //With the addition of the \$ search to capture strings like: "$(variable)"
            //\$\(           # $ char and escaped parenthesis, means "starts with a '$(' character"
            //    (          # Parentheses in a regex mean "put (capture) the stuff 
            //               #     in between into the Groups array" 
            //       [^)]    # Any character that is not a ')' character
            //       *       # Zero or more occurrences of the aforementioned "non ')' char"
            //    )          # Close the capturing group
            //\)             # "Ends with a ')' character"  
            MatchCollection results = Regex.Matches(text, @"\$\{\{([^}}]*)\}\}");
            List<string> list = results.Cast<Match>().Select(match => match.Value).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                string item = list[i];

                //Remove leading "${{" and trailing "}}"
                if (list[i].Length > 5)
                {
                    list[i] = list[i].Substring(0, item.Length - 2);
                    list[i] = list[i].Remove(0, 3);
                }
            }

            return list;
        }
    }
}
