using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public class PipelineProcessing<TTriggers, TVariables>
    {
        public List<string> VariableList;
        public string MatrixVariableName;
        private bool _verbose;

        public PipelineProcessing(bool verbose) => _verbose = verbose;

        void WriteLine(string message)
        {
            if (_verbose)
                Console.WriteLine(message);
        }

        /// <summary>
        /// Process an Azure DevOps Pipeline, converting it to a GitHub Action
        /// </summary>
        /// <param name="azurePipeline">Azure DevOps Pipeline object</param>
        /// <param name="simpleTrigger">When the YAML has a simple trigger, (String[]). Can be null</param>
        /// <param name="complexTrigger">When the YAML has a complex trigger. Can be null</param>
        /// <returns>GitHub Actions object</returns>
        public GitHubActionsRoot ProcessPipeline(AzurePipelinesRoot<TTriggers, TVariables> azurePipeline,
            string[] simpleTrigger, AzurePipelines.Trigger complexTrigger,
            Dictionary<string, string> simpleVariables, AzurePipelines.Variable[] complexVariables)
        {
            VariableList = new List<string>();
            GitHubActionsRoot gitHubActions = new GitHubActionsRoot();
            //Name
            if (azurePipeline.name != null)
            {
                gitHubActions.name = azurePipeline.name;
            }

            //Container
            if (azurePipeline.container != null)
            {
                gitHubActions.messages.Add("TODO: Container conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39");
            }

            //Triggers for pushs 
            if (azurePipeline.trigger != null)
            {
                if (complexTrigger != null)
                {
                    gitHubActions.on = ProcessComplexTrigger(complexTrigger);
                }
                else if (simpleTrigger != null)
                {
                    gitHubActions.on = ProcessSimpleTrigger(simpleTrigger);
                }
            }

            //Triggers for pull requests
            if (azurePipeline.pr != null)
            {
                GitHubActions.Trigger pr = ProcessPullRequest(azurePipeline.pr);
                if (gitHubActions.on == null)
                {
                    gitHubActions.on = pr;
                }
                else
                {
                    gitHubActions.on.pull_request = pr.pull_request;
                }
            }

            //Container
            if (azurePipeline.pool != null && azurePipeline.pool.demands != null)
            {
                gitHubActions.messages.Add("Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet");
            }

            //schedules
            if (azurePipeline.schedules != null)
            {
                string[] schedules = ProcessSchedules(azurePipeline.schedules);
                if (gitHubActions.on == null)
                {
                    gitHubActions.on = new GitHubActions.Trigger();
                }
                gitHubActions.on.schedule = schedules;
            }

            //Resources
            if (azurePipeline.resources != null)
            {
                //Note: Containers is in the jobs - this note should be removed once pipeliens and repositories is moved too

                //TODO: Add code for pipelines
                if (azurePipeline.resources.pipelines != null)
                {
                    gitHubActions.messages.Add("TODO: Resource pipelines conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/8");
                    if (azurePipeline.resources.pipelines.Length > 0)
                    {
                        if (azurePipeline.resources.pipelines[0].pipeline != null)
                        {
                            WriteLine("pipeline: " + azurePipeline.resources.pipelines[0].pipeline);
                        }
                        if (azurePipeline.resources.pipelines[0].project != null)
                        {
                            WriteLine("project: " + azurePipeline.resources.pipelines[0].project);
                        }
                        if (azurePipeline.resources.pipelines[0].source != null)
                        {
                            WriteLine("source: " + azurePipeline.resources.pipelines[0].source);
                        }
                        if (azurePipeline.resources.pipelines[0].branch != null)
                        {
                            WriteLine("branch: " + azurePipeline.resources.pipelines[0].branch);
                        }
                        if (azurePipeline.resources.pipelines[0].version != null)
                        {
                            WriteLine("version: " + azurePipeline.resources.pipelines[0].version);
                        }
                        if (azurePipeline.resources.pipelines[0].trigger != null)
                        {
                            if (azurePipeline.resources.pipelines[0].trigger.autoCancel)
                            {
                                WriteLine("autoCancel: " + azurePipeline.resources.pipelines[0].trigger.autoCancel);
                            }
                            if (azurePipeline.resources.pipelines[0].trigger.batch)
                            {
                                WriteLine("batch: " + azurePipeline.resources.pipelines[0].trigger.batch);
                            }
                        }

                    }
                }

                //TODO: Add code for repositories
                if (azurePipeline.resources.repositories != null)
                {
                    gitHubActions.messages.Add("TODO: Resource repositories conversion not yet done: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/8");

                    if (azurePipeline.resources.repositories.Length > 0)
                    {
                        if (azurePipeline.resources.repositories[0].repository != null)
                        {
                            WriteLine("repository: " + azurePipeline.resources.repositories[0].repository);
                        }
                        if (azurePipeline.resources.repositories[0].type != null)
                        {
                            WriteLine("type: " + azurePipeline.resources.repositories[0].type);
                        }
                        if (azurePipeline.resources.repositories[0].name != null)
                        {
                            WriteLine("name: " + azurePipeline.resources.repositories[0].name);
                        }
                        if (azurePipeline.resources.repositories[0]._ref != null)
                        {
                            WriteLine("ref: " + azurePipeline.resources.repositories[0]._ref);
                        }
                        if (azurePipeline.resources.repositories[0].endpoint != null)
                        {
                            WriteLine("endpoint: " + azurePipeline.resources.repositories[0].endpoint);
                        }
                        if (azurePipeline.resources.repositories[0].connection != null)
                        {
                            WriteLine("connection: " + azurePipeline.resources.repositories[0].connection);
                        }
                        if (azurePipeline.resources.repositories[0].source != null)
                        {
                            WriteLine("source: " + azurePipeline.resources.repositories[0].source);
                        }
                    }
                }
            }

            //Stages (Note: stages are not yet present in actions, we are merging them into one giant list of jobs, appending the stage name to jobs to keep names unique)
            if (azurePipeline.stages != null)
            {
                //Count the number of jobs and initialize the jobs array with that number
                int jobCounter = 0;
                foreach (Stage stage in azurePipeline.stages)
                {
                    if (stage.jobs != null)
                    {
                        jobCounter += stage.jobs.Length;
                    }
                }
                azurePipeline.jobs = new AzurePipelines.Job[jobCounter];
                //We are going to take each stage and assign it a set of jobs
                int currentIndex = 0;
                foreach (Stage stage in azurePipeline.stages)
                {
                    if (stage.jobs != null)
                    {
                        int j = 0;
                        for (int i = 0; i < stage.jobs.Length; i++)
                        {
                            //Get the job name
                            string jobName = stage.jobs[j].job;
                            if (jobName == null && stage.jobs[i].deployment != null)
                            {
                                jobName = stage.jobs[i].deployment;
                            }
                            if (jobName == null && stage.jobs[j].template != null)
                            {
                                jobName = "Template";
                            }
                            if (jobName == null)
                            {
                                jobName = "job" + currentIndex.ToString();
                            }
                            //Rename the job, using the stage name as prefix, so that we keep the job names unique
                            stage.jobs[j].job = stage.stage + "_Stage_" + jobName;
                            Console.WriteLine("This variable is not needed in actions: " + stage.displayName);
                            azurePipeline.jobs[currentIndex] = stage.jobs[j];
                            azurePipeline.jobs[currentIndex].condition = stage.condition;
                            j++;
                            currentIndex++;
                        }
                    }
                }
            }

            //Jobs (when no stages are defined)
            if (azurePipeline.jobs != null)
            {
                //If there is a parent strategy, and no child strategy, load in the parent
                //This is not perfect...
                if (azurePipeline.strategy != null)
                {
                    foreach (AzurePipelines.Job item in azurePipeline.jobs)
                    {
                        if (item.strategy == null)
                        {
                            item.strategy = azurePipeline.strategy;
                        }
                    }
                }
                gitHubActions.jobs = ProcessJobs(azurePipeline.jobs, azurePipeline.resources);
                if (gitHubActions.jobs.Count == 0)
                {
                    gitHubActions.messages.Add("Note that although having no jobs is valid YAML, it is not a valid GitHub Action.");
                }
            }

            //Pool + Steps (When there are no jobs defined)
            if ((azurePipeline.pool != null && azurePipeline.jobs == null) || (azurePipeline.steps != null && azurePipeline.steps.Length > 0))
            {
                //Steps only have one job, so we just create it here
                gitHubActions.jobs = new Dictionary<string, GitHubActions.Job>
                {
                    {
                        "build",
                        new GitHubActions.Job
                        {
                            runs_on = ProcessPool(azurePipeline.pool),
                            strategy = ProcessStrategy(azurePipeline.strategy),
                            container = ProcessContainer(azurePipeline.resources),
                            //resources = ProcessResources(azurePipeline.resources),
                            steps = ProcessSteps(azurePipeline.steps)
                        }
                    }
                };
            }

            //Variables
            if (azurePipeline.variables != null)
            {
                if (complexVariables != null)
                {
                    gitHubActions.env = ProcessComplexVariables(complexVariables);
                }
                else if (simpleVariables != null)
                {
                    gitHubActions.env = ProcessSimpleVariables(simpleVariables);
                }
            }
            else if (azurePipeline.parameters != null)
            {
                //For now, convert the parameters to variables
                gitHubActions.env = ProcessSimpleVariables(azurePipeline.parameters);
            }

            return gitHubActions;
        }

        //Process a simple trigger, e.g. "Trigger: [master, develop]"
        private GitHubActions.Trigger ProcessSimpleTrigger(string[] trigger)
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
        private GitHubActions.Trigger ProcessComplexTrigger(AzurePipelines.Trigger trigger)
        {
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
        private GitHubActions.Trigger ProcessPullRequest(AzurePipelines.Trigger pr)
        {
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
        private string[] ProcessSchedules(AzurePipelines.Schedule[] schedules)
        {
            string[] newSchedules = new string[schedules.Length];
            for (int i = 0; i < schedules.Length; i++)
            {
                newSchedules[i] = "cron: '" + schedules[i].cron + "'";
            }

            return newSchedules;
        }

        //process the build pool/agent
        private string ProcessPool(Pool pool)
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
        private string ProcessCondition(string condition)
        {
            return ConditionsProcessing.TranslateConditions(condition);
        }

        //process the strategy matrix
        private GitHubActions.Strategy ProcessStrategy(AzurePipelines.Strategy strategy)
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
                    WriteLine("This variable is not needed in actions: " + strategy.parallel);
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
                    WriteLine("TODO: " + strategy.runOnce);
                }
                return processedStrategy;
            }
            else
            {
                return null;
            }
        }

        private Container ProcessContainer(Resources resources)
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
        private Dictionary<string, string> ProcessSimpleVariables(Dictionary<string, string> variables)
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

        //process all (complex) variables
        private Dictionary<string, string> ProcessComplexVariables(AzurePipelines.Variable[] variables)
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
                    }
                    //groups
                    if (variables[i].group != null)
                    {
                        if (!processedVariables.ContainsKey("group"))
                            processedVariables.Add("group", variables[i].group);
                        else
                            WriteLine("group: only 1 variable group is supported at present");
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

        //process the jobs
        private Dictionary<string, GitHubActions.Job> ProcessJobs(AzurePipelines.Job[] jobs, Resources resources)
        {
            //A dictonary is perfect here, as the job_id (a string), must be unique in the action
            Dictionary<string, GitHubActions.Job> newJobs = null;
            if (jobs != null)
            {
                newJobs = new Dictionary<string, GitHubActions.Job>();
                for (int i = 0; i < jobs.Length; i++)
                {
                    string jobName = jobs[i].job;
                    if (jobName == null && jobs[i].deployment != null)
                    {
                        jobName = jobs[i].deployment;
                    }
                    else if (jobName == null && jobs[i].template != null)
                    {
                        jobName = "job_" + (i + 1).ToString() + "_template";
                    }
                    newJobs.Add(jobName, ProcessIndividualJob(jobs[i], resources));
                }
            }
            return newJobs;
        }

        private GitHubActions.Job ProcessIndividualJob(AzurePipelines.Job job, AzurePipelines.Resources resources)
        {
            GitHubActions.Job newJob = new GitHubActions.Job
            {
                name = job.displayName,
                needs = job.dependsOn,
                _if = ProcessCondition(job.condition),
                runs_on = ProcessPool(job.pool),
                strategy = ProcessStrategy(job.strategy),
                container = ProcessContainer(resources),
                env = ProcessSimpleVariables(job.variables),
                timeout_minutes = job.timeoutInMinutes,
                steps = ProcessSteps(job.steps)
            };

            if (newJob.steps == null & job.template != null)
            {
                //Initialize the array with no items
                job.steps = new AzurePipelines.Step[0];
                //Process the steps, adding the default checkout step
                newJob.steps = ProcessSteps(job.steps, true);
                //TODO: Find a way to allow GitHub jobs to reference another job as a template
                newJob.job_message += "Note: Azure DevOps template does not have an equivalent in GitHub Actions yet";
            }
            else if (newJob.steps == null && job.strategy?.runOnce?.deploy?.steps != null)
            {
                //Initialize the array with no items
                job.steps = new AzurePipelines.Step[0];
                //Process the steps, adding the default checkout step
                newJob.steps = ProcessSteps(job.strategy?.runOnce?.deploy?.steps, false);
                //TODO: Find a way to allow GitHub jobs to reference another job as a template
                newJob.job_message += "Note: Azure DevOps strategy>runOnce>deploy does not have an equivalent in GitHub Actions yet";
            }
            if (job.environment != null)
            {
                newJob.job_message += "Note: Azure DevOps job environment does not have an equivalent in GitHub Actions yet";
            }

            if (newJob._if != null)
            {
                if (newJob.job_message != null)
                {
                    newJob.job_message += System.Environment.NewLine;
                }
            }
            if (job.continueOnError)
            {
                newJob.continue_on_error = job.continueOnError;
            }

            return newJob;
        }

        //process the steps
        private GitHubActions.Step[] ProcessSteps(AzurePipelines.Step[] steps, bool addCheckoutStep = true)
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
                        switch (step.task)
                        {
                            //If we have an Java based step, we will need to add a Java setup step
                            case "Ant@1":
                            case "Maven@3":
                                if (addJavaSetupStep == false)
                                {
                                    addJavaSetupStep = true;
                                    stepAdjustment++;
                                    javaVersion = stepsProcessing.GetStepInput(step, "jdkVersionOption");
                                }
                                break;

                            //Needs a the Java step and an additional Gradle step
                            case "Gradle@2":

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
                            case "AzureAppServiceManage@0":
                            case "AzureResourceGroupDeployment@2":
                            case "AzureRmWebAppDeployment@3":
                                if (addAzureLoginStep == false)
                                {
                                    addAzureLoginStep = true;
                                    stepAdjustment++;
                                }
                                break;

                            case "VSBuild@1":
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
