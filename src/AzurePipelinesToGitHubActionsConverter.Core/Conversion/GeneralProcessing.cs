using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization;
using AzurePipelinesToGitHubActionsConverter.Core.Extensions;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public class GeneralProcessing
    {
        public List<string> VariableList;
        public string MatrixVariableName;
        private readonly bool _verbose;
        public GeneralProcessing(bool verbose)
        {
            VariableList = new List<string>();
            _verbose = verbose;
        }

        public string ProcessName(string nameYaml)
        {
            if (nameYaml != null)
            {
                return nameYaml.Replace("name:", "").Replace(System.Environment.NewLine, "").Trim();
            }
            else
            {
                return null;
            }
        }

        public GitHubActions.Trigger ProcessTriggerPRAndSchedules(string triggerYaml, string prYaml, string schedulesYaml)
        {
            triggerYaml = ConversionUtility.ProcessAndCleanElement(triggerYaml, "trigger:", "- ");
            prYaml = ConversionUtility.ProcessAndCleanElement(prYaml, "pr:", "- ");
            schedulesYaml = ConversionUtility.RemoveFirstLine(schedulesYaml);

            //Convert the simple triggers to complex, and then serialize once.
            AzurePipelines.Trigger trigger = null;
            if (triggerYaml != null)
            {
                try
                {
                    string[] simpleTrigger = GenericObjectSerialization.DeserializeYaml<string[]>(triggerYaml);
                    trigger = new AzurePipelines.Trigger
                    {
                        branches = new IncludeExclude
                        {
                            include = simpleTrigger
                        }
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DeserializeYaml<string[]>(triggerYaml) swallowed an exception, to be reviewed...");
                    trigger = GenericObjectSerialization.DeserializeYaml<AzurePipelines.Trigger>(triggerYaml);
                }
            }
            AzurePipelines.Trigger pr = null;
            if (prYaml != null)
            {
                try
                {
                    string[] simplepr = GenericObjectSerialization.DeserializeYaml<string[]>(prYaml);
                    pr = new AzurePipelines.Trigger
                    {
                        branches = new IncludeExclude
                        {
                            include = simplepr
                        }
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DeserializeYaml<string[]>(prYaml) swallowed an exception, to be reviewed...");
                    pr = GenericObjectSerialization.DeserializeYaml<AzurePipelines.Trigger>(prYaml);
                }
            }
            Schedule[] schedules = null;
            if (schedulesYaml != null)
            {
                try
                {
                    schedules = GenericObjectSerialization.DeserializeYaml<Schedule[]>(schedulesYaml);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DeserializeYaml<Schedule[]>(schedulesYaml) swallowed an exception, to be reviewed...");
                }
            }

            GitHubActions.Trigger push = ProcessComplexTrigger(trigger);
            GitHubActions.Trigger pullRequest = ProcessPullRequest(pr);
            string[] schedule = ProcessSchedules(schedules);

            if (push != null || pullRequest != null || schedule != null)
            {
                return new GitHubActions.Trigger
                {
                    push = push?.push,
                    pull_request = pullRequest?.pull_request,
                    schedule = schedule
                };
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, string> ProcessParametersAndVariables(string parametersYaml, string variablesYaml)
        {
            List<Variable> parameters = null;
            if (parametersYaml != null)
            {
                parametersYaml = ConversionUtility.RemoveFirstLine(parametersYaml);
                try
                {
                    Dictionary<string, string> simpleParameters = GenericObjectSerialization.DeserializeYaml<Dictionary<string, string>>(parametersYaml);
                    parameters = new List<AzurePipelines.Variable>();
                    foreach (KeyValuePair<string, string> item in simpleParameters)
                    {
                        parameters.Add(new Variable
                        {
                            name = item.Key,
                            value = item.Value
                        });
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DeserializeYaml<Dictionary<string, string>>(parametersYaml) swallowed an exception, to be reviewed...");
                    parameters = GenericObjectSerialization.DeserializeYaml<List<Variable>>(parametersYaml);
                }
            }

            List<Variable> variables = null;
            if (variablesYaml != null)
            {
                variablesYaml = ConversionUtility.RemoveFirstLine(variablesYaml);
                try
                {
                    Dictionary<string, string> simpleVariables = GenericObjectSerialization.DeserializeYaml<Dictionary<string, string>>(variablesYaml);
                    variables = new List<AzurePipelines.Variable>();
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
                    Debug.WriteLine($"DeserializeYaml<Dictionary<string, string>>(variablesYaml) swallowed an exception, to be reviewed...");
                    parameters = GenericObjectSerialization.DeserializeYaml<List<Variable>>(variablesYaml);
                }
            }


            Dictionary<string, string> env = new Dictionary<string, string>();
            Dictionary<string, string> processedParameters = ProcessComplexVariables2(parameters);
            Dictionary<string, string> processedVariables = ProcessComplexVariables2(variables);
            foreach (KeyValuePair<string, string> item in processedParameters)
            {
                if (env.ContainsKey(item.Key) == false)
                {
                    env.Add(item.Key, item.Value);
                }
            }
            foreach (KeyValuePair<string, string> item in processedVariables)
            {
                if (env.ContainsKey(item.Key) == false)
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

        //Process a simple trigger, e.g. "Trigger: [master, develop]"
        public GitHubActions.Trigger ProcessSimpleTrigger(string[] trigger)
        {
            AzurePipelines.Trigger newTrigger = new AzurePipelines.Trigger
            {
                branches = new IncludeExclude
                {
                    include = trigger
                }
            };
            return ProcessComplexTrigger(newTrigger);
        }

        //Process a complex trigger, using the Trigger object
        public GitHubActions.Trigger ProcessComplexTrigger(AzurePipelines.Trigger trigger)
        {
            if (trigger == null)
            {
                return null;
            }
            //Note: as of 18-Oct, you receive an error if you try to post both a "branches" and a "ignore-branches", or a "paths and a ignore-paths". You can only have one or the other...
            TriggerDetail push = new TriggerDetail();
            //process branches
            if (trigger.branches != null)
            {
                if (trigger.branches.include != null)
                {
                    push.branches = trigger.branches.include;
                }
                else if (trigger.branches.exclude != null)
                {
                    push.branches_ignore = trigger.branches.exclude;
                }
            }
            //process paths
            if (trigger.paths != null)
            {
                if (trigger.paths.include != null)
                {
                    push.paths = trigger.paths.include;
                }
                if (trigger.paths.exclude != null)
                {
                    push.paths_ignore = trigger.paths.exclude;
                }
            }
            //process tags
            if (trigger.tags != null)
            {
                if (trigger.tags.include != null)
                {
                    push.tags = trigger.tags.include;
                }
                if (trigger.tags.exclude != null)
                {
                    push.tags_ignore = trigger.tags.exclude;
                }
            }

            return new GitHubActions.Trigger
            {
                push = push
            };

        }

        //process the pull request
        public GitHubActions.Trigger ProcessPullRequest(AzurePipelines.Trigger pr)
        {
            if (pr == null)
            {
                return null;
            }
            TriggerDetail pullRequest = new TriggerDetail();
            //process branches
            if (pr.branches != null)
            {
                if (pr.branches.include != null)
                {
                    pullRequest.branches = pr.branches.include;
                }
                else if (pr.branches.exclude != null)
                {
                    pullRequest.branches_ignore = pr.branches.exclude;
                }
            }
            //process paths
            if (pr.paths != null)
            {
                if (pr.paths.include != null)
                {
                    pullRequest.paths = pr.paths.include;
                }
                if (pr.paths.exclude != null)
                {
                    pullRequest.paths_ignore = pr.paths.exclude;
                }
            }
            //process tags
            if (pr.tags != null)
            {
                if (pr.tags.include != null)
                {
                    pullRequest.tags = pr.tags.include;
                }
                if (pr.tags.exclude != null)
                {
                    pullRequest.tags_ignore = pr.tags.exclude;
                }
            }

            return new GitHubActions.Trigger
            {
                pull_request = pullRequest
            };
        }

        //process the schedule
        public string[] ProcessSchedules(AzurePipelines.Schedule[] schedules)
        {
            if (schedules == null)
            {
                return null;
            }
            string[] newSchedules = new string[schedules.Length];
            for (int i = 0; i < schedules.Length; i++)
            {
                newSchedules[i] = "cron: '" + schedules[i].cron + "'";
            }

            return newSchedules;
        }

        //process the build pool/agent
        public string ProcessPool(Pool pool)
        {
            string newPool = null;
            if (pool != null)
            {
                if (pool.vmImage != null)
                {
                    newPool = pool.vmImage;
                }
                else if (pool.name != null)
                {
                    newPool = pool.name;
                }
            }
            return newPool;
        }

        //process the conditions
        public string ProcessCondition(string condition)
        {
            return ConditionsProcessing.TranslateConditions(condition);
        }

        //process the strategy matrix
        public GitHubActions.Strategy ProcessStrategy(AzurePipelines.Strategy strategy)
        {
            //Azure DevOps
            //strategy:
            //  matrix:
            //    linux:
            //      imageName: ubuntu - 16.04
            //    mac:
            //      imageName: macos-10.13
            //    windows:
            //      imageName: vs2017-win2016
            //jobs:
            //- job: Build
            //  pool: 
            //    vmImage: $(imageName)

            //GitHub Actions
            //runs-on: ${{ matrix.imageName }}
            //strategy:
            //  matrix:
            //    imageName: [ubuntu-16.04, macos-10.13, vs2017-win2016]

            if (strategy != null)
            {
                GitHubActions.Strategy processedStrategy = null;

                if (strategy.matrix != null)
                {
                    if (processedStrategy == null)
                    {
                        processedStrategy = new GitHubActions.Strategy();
                    }
                    string[] matrix = new string[strategy.matrix.Count];
                    KeyValuePair<string, Dictionary<string, string>> matrixVariable = strategy.matrix.First();
                    MatrixVariableName = matrixVariable.Value.Keys.First();
                    //VariableList.Add("$(" + _matrixVariableName + ")");
                    int i = 0;
                    foreach (KeyValuePair<string, Dictionary<string, string>> entry in strategy.matrix)
                    {
                        matrix[i] = strategy.matrix[entry.Key][MatrixVariableName];
                        i++;
                    }
                    processedStrategy.matrix = new Dictionary<string, string[]>
                    {
                        { MatrixVariableName, matrix }
                    };
                }
                if (strategy.parallel != null)
                {
                    ConversionUtility.WriteLine("This variable is not needed in actions: " + strategy.parallel, _verbose);
                }
                if (strategy.maxParallel != null)
                {
                    if (processedStrategy == null)
                    {
                        processedStrategy = new GitHubActions.Strategy();
                    }
                    processedStrategy.max_parallel = strategy.maxParallel;
                }
                if (strategy.runOnce != null)
                {
                    //TODO: Process other strategies
                    ConversionUtility.WriteLine("TODO: " + strategy.runOnce, _verbose);
                }
                return processedStrategy;
            }
            else
            {
                return null;
            }
        }

        public Container ProcessContainer(Resources resources)
        {
            //FROM
            //resources:
            //  containers:
            //  - container: string  # identifier (A-Z, a-z, 0-9, and underscore)
            //    image: string  # container image name
            //    options: string  # arguments to pass to container at startup
            //    endpoint: string  # reference to a service connection for the private registry
            //    env: { string: string }  # list of environment variables to add
            //    ports: [ string ] # ports to expose on the container
            //    volumes: [ string ] # volumes to mount on the container

            //TO
            //jobs:
            //  my_job:
            //    container:
            //      image: node:10.16-jessie
            //      env:
            //        NODE_ENV: development
            //      ports:
            //        - 80
            //      volumes:
            //        - my_docker_volume:/volume_mount
            //      options: --cpus 1

            if (resources != null && resources.containers != null && resources.containers.Length > 0)
            {
                Container container = new Container
                {
                    //All containers have at least the image name
                    image = resources.containers[0].image
                };

                //Optionally, these next 4 properties could also exist
                if (resources.containers[0].env != null)
                {
                    container.env = resources.containers[0].env;
                }
                if (resources.containers[0].ports != null)
                {
                    container.ports = resources.containers[0].ports;
                }
                if (resources.containers[0].volumes != null)
                {
                    container.volumes = resources.containers[0].volumes;
                }
                if (resources.containers[0].options != null)
                {
                    container.options = resources.containers[0].options;
                }
                return container;
            }
            else
            {
                return null;
            }
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

        public Dictionary<string, string> ProcessComplexVariables2(List<AzurePipelines.Variable> variables)
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

        //process all (complex) variables
        public Dictionary<string, string> ProcessComplexVariables(AzurePipelines.Variable[] variables)
        {
            Dictionary<string, string> processedVariables = new Dictionary<string, string>();
            if (variables != null)
            {
                //update variables from the $(variableName) format to ${{variableName}} format, by piping them into a list for replacement later.
                for (int i = 0; i < variables.Length; i++)
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


        //process the steps
        public GitHubActions.Step[] ProcessSteps(AzurePipelines.Step[] steps, bool addCheckoutStep = true)
        {
            StepsProcessing stepsProcessing = new StepsProcessing();

            GitHubActions.Step[] newSteps = null;
            if (steps != null)
            {
                //Start by scanning all of the steps, to see if we need to insert additional tasks
                int stepAdjustment = 0;
                bool addJavaSetupStep = false;
                bool addGradleSetupStep = false;
                bool addAzureLoginStep = false;
                bool addMSSetupStep = false;
                string javaVersion = null;

                //If the code needs a Checkout step, add it first
                if (addCheckoutStep == true)
                {
                    stepAdjustment++; // we are inserting a step and need to start moving steps 1 place into the array
                }

                //Loop through the steps to see if we need other tasks inserted in for specific circumstances
                foreach (AzurePipelines.Step step in steps)
                {
                    if (step.task != null)
                    {
                        switch (step.task.ToUpper()) //Set to upper case to handle case sensitivity comparisons e.g. NPM hangles Npm, NPM, or npm. 
                        {
                            //If we have an Java based step, we will need to add a Java setup step
                            case "ANT@1":
                            case "MAVEN@3":
                                if (addJavaSetupStep == false)
                                {
                                    addJavaSetupStep = true;
                                    stepAdjustment++;
                                    javaVersion = stepsProcessing.GetStepInput(step, "jdkVersionOption");
                                }
                                break;

                            //Needs a the Java step and an additional Gradle step
                            case "GRADLE@2":

                                if (addJavaSetupStep == false)
                                {
                                    addJavaSetupStep = true;
                                    stepAdjustment++;
                                    //Create the java step, as it doesn't exist
                                    javaVersion = "1.8";
                                }
                                if (addGradleSetupStep == false)
                                {
                                    addGradleSetupStep = true;
                                    stepAdjustment++;
                                }
                                break;

                            //If we have an Azure step, we will need to add a Azure login step
                            case "AZUREAPPSERVICEMANAGE@0":
                            case "AZURERESOURCEGROUPDEPLOYMENT@2":
                            case "AZURERMWEBAPPDEPLOYMENT@3":
                                if (addAzureLoginStep == false)
                                {
                                    addAzureLoginStep = true;
                                    stepAdjustment++;
                                }
                                break;

                            case "VSBUILD@1":
                                if (addMSSetupStep == false)
                                {
                                    addMSSetupStep = true;
                                    stepAdjustment++;
                                }
                                break;
                        }
                    }
                }

                //Re-size the newSteps array with adjustments as needed
                newSteps = new GitHubActions.Step[steps.Length + stepAdjustment];

                int adjustmentsUsed = 0;

                //Add the steps array
                if (addCheckoutStep == true)
                {
                    //Add the check out step to get the code
                    newSteps[adjustmentsUsed] = stepsProcessing.CreateCheckoutStep();
                    adjustmentsUsed++;
                }
                if (addJavaSetupStep == true)
                {
                    //Add the JavaSetup step to the code
                    if (javaVersion != null)
                    {
                        newSteps[adjustmentsUsed] = stepsProcessing.CreateSetupJavaStep(javaVersion);
                        adjustmentsUsed++;
                    }
                }
                if (addGradleSetupStep == true)
                {
                    //Add the Gradle setup step to the code
                    newSteps[adjustmentsUsed] = stepsProcessing.CreateSetupGradleStep();
                    adjustmentsUsed++;
                }
                if (addAzureLoginStep == true)
                {
                    //Add the Azure login step to the code
                    newSteps[adjustmentsUsed] = stepsProcessing.CreateAzureLoginStep();
                    adjustmentsUsed++;
                }
                if (addMSSetupStep == true)
                {
                    //Add the Azure login step to the code
                    newSteps[adjustmentsUsed] = stepsProcessing.CreateMSBuildSetupStep();
                    //adjustmentsUsed++;
                }

                //Translate the other steps
                for (int i = stepAdjustment; i < steps.Length + stepAdjustment; i++)
                {
                    newSteps[i] = stepsProcessing.ProcessStep(steps[i - stepAdjustment]);
                }
            }

            return newSteps;
        }

    }
}
