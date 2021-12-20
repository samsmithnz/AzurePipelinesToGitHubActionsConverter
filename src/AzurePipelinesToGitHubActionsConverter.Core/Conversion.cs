using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using GitHubActionsDotNet.Common;
using GitHubActionsDotNet.Models;
using System.Collections.Generic;
using System.Text.Json;
using ConversionUtility = AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion.ConversionUtility;
using GitHubActions = GitHubActionsDotNet.Models;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public class Conversion
    {
        private string _matrixVariableName;
        private readonly bool _verbose;

        public Conversion(bool verbose = true)
        {
            _verbose = verbose;
        }

        /// <summary>
        /// Convert an entire Azure DevOps Pipeline to a GitHub Actions 
        /// </summary>
        /// <param name="yaml">Yaml to convert</param>
        /// <returns>Converion object, with original yaml, processed yaml, comments on the conversion, and which conversion method was used</returns>
        public ConversionResponse ConvertAzurePipelineToGitHubAction(string yaml, bool addWorkFlowDispatch = false)
        {
            return ConvertAzurePipelineToGitHubActionV2(yaml, addWorkFlowDispatch);
        }

        /// <summary>
        /// V2 plan:
        /// 1. get the yaml
        /// 2. converting the yaml into a json document
        /// 3. parse the json document into azure pipelines sub-objects
        /// 4. put it together into one azure pipelines object
        /// 5. convert the azure pipelines object to github action
        /// </summary>
        /// <param name="yaml"></param>
        /// <returns></returns>

        private ConversionResponse ConvertAzurePipelineToGitHubActionV2(string yaml, bool addWorkFlowDispatch)
        {
            string gitHubYaml = "";
            List<string> variableList = new List<string>();
            List<string> stepComments = new List<string>();

            //convert the yaml into json, it's easier to parse
            JsonElement jsonObject = new JsonElement();
            if (yaml != null)
            {
                //Clean up the YAML to remove conditional insert statements
                string processedYaml = ConversionUtility.CleanYamlBeforeDeserializationV2(yaml);
                jsonObject = JsonSerialization.DeserializeStringToJsonElement(processedYaml);
            }

            //Build up the GitHub object piece by piece
            GitHubActionsRoot gitHubActions = new GitHubActionsRoot();
            GeneralProcessing gp = new GeneralProcessing(_verbose);

            if (jsonObject.ValueKind != JsonValueKind.Undefined)
            {
                JsonElement jsonElement;
                //Name
                if (jsonObject.TryGetProperty("name", out jsonElement))
                {
                    string nameYaml = jsonElement.ToString();
                    gitHubActions.name = gp.ProcessNameV2(nameYaml);
                }

                //Trigger
                TriggerProcessing tp = new TriggerProcessing(_verbose);
                if (jsonObject.TryGetProperty("trigger", out jsonElement))
                {
                    string triggerYaml;
                    if (jsonElement.ToString() == "none")
                    {
                        triggerYaml = ConversionUtility.ProcessNoneJsonElement;
                    }
                    else
                    {
                        triggerYaml = jsonElement.ToString();
                    }
                    gitHubActions.on = tp.ProcessTriggerV2(triggerYaml);
                }
                //PR
                if (jsonObject.TryGetProperty("pr", out jsonElement))
                {
                    string prYaml;
                    if (jsonElement.ToString() == "none")
                    {
                        prYaml = ConversionUtility.ProcessNoneJsonElement;
                    }
                    else
                    {
                        prYaml = jsonElement.ToString();
                    }
                    GitHubActions.Trigger prTrigger = tp.ProcessPullRequestV2(prYaml);
                    if (gitHubActions.on == null)
                    {
                        gitHubActions.on = prTrigger;
                    }
                    else
                    {
                        gitHubActions.on.pull_request = prTrigger.pull_request;
                    }
                }
                //Schedules
                if (jsonObject.TryGetProperty("schedules", out jsonElement))
                {
                    string schedulesYaml = jsonElement.ToString();
                    GitHubActions.Trigger schedules = tp.ProcessSchedulesV2(schedulesYaml);
                    if (gitHubActions.on == null)
                    {
                        gitHubActions.on = schedules;
                    }
                    else
                    {
                        gitHubActions.on.schedule = schedules.schedule;
                    }
                }
                // Allows you to run this workflow manually from the Actions tab
                if (addWorkFlowDispatch)
                {
                    GitHubActions.Trigger workflowDispatch = new();
                    workflowDispatch.workflow_dispatch = "";
                    if (gitHubActions.on == null)
                    {
                        gitHubActions.on = workflowDispatch;
                    }
                    else
                    {
                        gitHubActions.on.workflow_dispatch = workflowDispatch.workflow_dispatch;
                    }
                }

                //Parameters 
                string parametersYaml = null;
                if (jsonObject.TryGetProperty("parameters", out jsonElement))
                {
                    parametersYaml = jsonElement.ToString();
                }
                //Variables
                string variablesYaml = null;
                if (jsonObject.TryGetProperty("variables", out jsonElement))
                {
                    variablesYaml = jsonElement.ToString();
                }
                VariablesProcessing vp = new VariablesProcessing(_verbose);
                if (parametersYaml != null || variablesYaml != null)
                {
                    gitHubActions.env = vp.ProcessParametersAndVariablesV2(parametersYaml, variablesYaml);
                }

                //Resources
                string resourcesYaml = null;
                AzurePipelines.Repositories[] repositories = null;
                if (jsonObject.TryGetProperty("resources", out jsonElement))
                {
                    resourcesYaml = jsonElement.ToString();

                    //Resource Pipelines
                    if (resourcesYaml?.IndexOf("\"pipelines\"") >= 0)
                    {
                        gitHubActions.messages.Add("TODO: Resource pipelines conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/8");
                    }
                    //Resource Repositories
                    if (resourcesYaml?.IndexOf("\"repositories\"") >= 0 && jsonElement.TryGetProperty("repositories", out jsonElement))
                    {
                        repositories = gp.ProcessRepositories(jsonElement.ToString());
                    }
                    //Resource Container
                    if (resourcesYaml?.IndexOf("\"containers\"") >= 0)
                    {
                        gitHubActions.messages.Add("TODO: Container conversion not yet done, we need help!: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39");
                    }
                }

                //Strategy
                JsonElement strategy = new JsonElement();
                if (jsonObject.TryGetProperty("strategy", out jsonElement))
                {
                    strategy = jsonElement;
                }

                //If we have stages, convert them into jobs first:
                if (jsonObject.TryGetProperty("stages", out jsonElement))
                {
                    StagesProcessing sp = new StagesProcessing(_verbose);
                    gitHubActions.jobs = sp.ProcessStagesV2(jsonElement, strategy);
                }
                //If we don't have stages, but have jobs:
                else if (!jsonObject.TryGetProperty("stages", out jsonElement) && jsonObject.TryGetProperty("jobs", out jsonElement))
                {
                    JobProcessing jp = new JobProcessing(_verbose);
                    gitHubActions.jobs = jp.ProcessJobsV2(jp.ExtractAzurePipelinesJobsV2(jsonElement, strategy), gp.ExtractResourcesV2(resourcesYaml));
                    _matrixVariableName = jp.MatrixVariableName;
                }
                //Otherwise, if we don't have stages or jobs, we just have steps, and need to load them into a new job
                else if (!jsonObject.TryGetProperty("stages", out jsonElement) && !jsonObject.TryGetProperty("jobs", out jsonElement))
                {
                    //Pool
                    string poolYaml = null;
                    if (jsonObject.TryGetProperty("pool", out jsonElement))
                    {
                        poolYaml = jsonElement.ToString();
                    }
                    //pool/demands
                    if (poolYaml?.IndexOf("\"demands\":") >= 0)
                    {
                        gitHubActions.messages.Add("Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet");
                    }

                    //Steps
                    string stepsYaml = null;
                    if (jsonObject.TryGetProperty("steps", out jsonElement))
                    {
                        stepsYaml = jsonElement.ToString();
                    }
                    JobProcessing jp = new JobProcessing(_verbose);
                    AzurePipelines.Job[] pipelineJobs = jp.ProcessJobFromPipelineRootV2(poolYaml, strategy, stepsYaml);
                    Resources resources = gp.ExtractResourcesV2(resourcesYaml);
                    gitHubActions.jobs = jp.ProcessJobsV2(pipelineJobs, resources);
                    _matrixVariableName = jp.MatrixVariableName;
                }

                //Process repositories
                if (repositories != null)
                {
                    gitHubActions = gp.ProcessStepsWithRepositories(gitHubActions, repositories);
                }

                if (gitHubActions.jobs != null && gitHubActions.jobs.Count == 0)
                {
                    gitHubActions.messages.Add("Note that although having no jobs is valid YAML, it is not a valid GitHub Action.");
                }

                //Load in all variables. Duplicates are ok, they are processed the same
                variableList.AddRange(vp.SearchForVariables(yaml));
                variableList.AddRange(vp.SearchForVariablesV2(gitHubActions));

                //Create the GitHub YAML and apply some adjustments
                gitHubYaml = GitHubActionsSerialization.Serialize(gitHubActions, variableList, _matrixVariableName);

                //Add failed task comments to the top of the converted YAML
                //Add any header messages
                foreach (string message in gitHubActions.messages)
                {
                    stepComments.Add(ConversionUtility.ConvertMessageToYamlComment(message));
                }
                //look through jobs for any individual step comments
                if (gitHubActions.jobs != null)
                {
                    foreach (KeyValuePair<string, GitHubActions.Job> job in gitHubActions.jobs)
                    {
                        if (job.Value.steps != null)
                        {
                            if (job.Value.job_message != null)
                            {
                                stepComments.Add(ConversionUtility.ConvertMessageToYamlComment(job.Value.job_message));
                            }
                            foreach (GitHubActions.Step step in job.Value.steps)
                            {
                                if (step != null && !string.IsNullOrEmpty(step.step_message))
                                {
                                    stepComments.Add(ConversionUtility.ConvertMessageToYamlComment(step.step_message));
                                }
                            }
                        }
                    }
                }

            }

            //Append all of the comments to the top of the file (traverse them in reverse so they appear in the correct order)
            List<KeyValuePair<int, string>> stepCommentWithLines = ProcessHeaderComments(gitHubYaml, stepComments);
            for (int i = stepCommentWithLines.Count - 1; i >= 0; i--)
            {
                gitHubYaml = stepCommentWithLines[i].Value.Replace("#Error:", "#Error (line " + stepCommentWithLines[i].Key.ToString() + "):") + System.Environment.NewLine + gitHubYaml;
                //gitHubYaml = stepComments[i] + System.Environment.NewLine + gitHubYaml;
            }
            //if (stepComments.Count > 0)
            //{
            //    gitHubYaml = "#" + stepComments.Count.ToString() + " conversion messages found:" + System.Environment.NewLine + gitHubYaml;
            //}
            //gitHubYaml = gitHubYaml + System.Environment.NewLine + "#Generated by https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/pull/222";

            if (gitHubYaml == "{}")
            {
                gitHubYaml = "";
            }

            //Return the final conversion result, with the original (pipeline) yaml, processed (actions) yaml, and any comments
            return new ConversionResponse
            {
                pipelinesYaml = yaml,
                actionsYaml = gitHubYaml,
                comments = stepComments
            };
        }

        //get line numbers for step comments
        private List<KeyValuePair<int, string>> ProcessHeaderComments(string gitHubYaml, List<string> stepComments)
        {
            //Start by loading each step comment, initialize with line 0 (which shouldn't exist)
            List<KeyValuePair<int, string>> stepCommentWithLines = new List<KeyValuePair<int, string>>();

            //for each comment, find it in the text
            if (gitHubYaml != null)
            {
                string[] lines = gitHubYaml.Split(System.Environment.NewLine);

                int currentLine = 0;
                for (int j = 0; j < stepComments.Count; j++)
                {
                    bool foundStepComment = false;
                    string stepComment = stepComments[j];
                    for (int i = currentLine; i < lines.Length; i++)
                    {
                        //Trim the front and back of the string to compare the message to the serialized line
                        string line = lines[i].Trim().Replace("- # \"", "");
                        if (line.Length > 0)
                        {
                            line = line.Substring(0, line.Length - 1);
                        }

                        //Compare the cleaned up line with the step comments
                        if (line.IndexOf(stepComment.Replace("#", "")) >= 0)
                        {
                            //The zero indexed current line + 1, number of comments //, and summary line 
                            int lineNumber = i + 1 + stepComments.Count;
                            stepCommentWithLines.Add(new KeyValuePair<int, string>(lineNumber, stepComment));
                            currentLine = i + 1;
                            foundStepComment = true;
                            break;
                        }
                    }
                    //If a comment wasn't found, its a misc comment and should just be appended
                    if (!foundStepComment)
                    {
                        stepCommentWithLines.Add(new KeyValuePair<int, string>(0, stepComment));
                    }
                }
            }

            return stepCommentWithLines;
        }

        /// <summary>
        /// Convert a single Azure DevOps Pipeline task to a GitHub Actions task
        /// </summary>
        /// <param name="input">Yaml to convert</param>
        /// <returns>Converion object, with original yaml, processed yaml, and comments on the conversion</returns>
        public ConversionResponse ConvertAzurePipelineTaskToGitHubActionTask(string input)
        {
            string yaml = "";
            string processedInput = ConversionUtility.StepsPreProcessing(input);
            GitHubActions.Step gitHubActionStep = new GitHubActions.Step();

            //Process the YAML for the individual job
            AzurePipelines.Job azurePipelinesJob = YamlSerialization.DeserializeYaml<AzurePipelines.Job>(processedInput);
            if (azurePipelinesJob != null && azurePipelinesJob.steps != null && azurePipelinesJob.steps.Length > 0)
            {
                //As we needed to create an entire (but minimal) pipelines job, we need to now extract the step for processing
                StepsProcessing stepsProcessing = new StepsProcessing();
                gitHubActionStep = stepsProcessing.ProcessStep(azurePipelinesJob.steps[0]);

                //Find all variables in this text block, we need this for a bit later
                VariablesProcessing vp = new VariablesProcessing(_verbose);
                List<string> variableList = vp.SearchForVariables(processedInput);

                //Create the GitHub YAML and apply some adjustments
                if (gitHubActionStep != null)
                {
                    //add the step into a github job so it renders correctly
                    GitHubActions.Job gitHubJob = new GitHubActions.Job
                    {
                        steps = new GitHubActions.Step[1] //create an array of size 1
                    };
                    //Load the step into the single item array
                    gitHubJob.steps[0] = gitHubActionStep;

                    //Finally, we can serialize the job back to yaml
                    yaml = GitHubActionsSerialization.SerializeJob(gitHubJob, variableList);
                }
            }

            //Load failed tasks and comments for processing
            List<string> allComments = new List<string>();
            if (gitHubActionStep != null)
            {
                allComments.Add(gitHubActionStep.step_message);
            }

            //Return the final conversion result, with the original (pipeline) yaml, processed (actions) yaml, and any comments
            return new ConversionResponse
            {
                pipelinesYaml = input,
                actionsYaml = yaml,
                comments = allComments
            };
        }

    }
}
