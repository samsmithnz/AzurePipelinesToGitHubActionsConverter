using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.GitHubActions;
using System;
using System.Collections.Generic;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion
{
    public class PipelineProcessing<TTriggers, TVariables>
    {
        public List<string> VariableList;
        public string MatrixVariableName;
        private readonly bool _verbose;

        public PipelineProcessing(bool verbose)
        {
            _verbose = verbose;
        }

        void WriteLine(string message)
        {
            if (_verbose)
            {
                Console.WriteLine(message);
            }
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
            GeneralProcessing generalProcessing = new GeneralProcessing(_verbose);
            GitHubActionsRoot gitHubActions = new GitHubActionsRoot();


            //Name
            if (azurePipeline.name != null)
            {
                gitHubActions.name = azurePipeline.name;
            }

            //Container
            if (azurePipeline.container != null)
            {
                gitHubActions.messages.Add("TODO: Container conversion not yet done, we need help - our container skills are woeful: https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter/issues/39");
            }

            //Triggers for pushs 
            if (azurePipeline.trigger != null)
            {
                if (complexTrigger != null)
                {
                    gitHubActions.on = generalProcessing.ProcessComplexTrigger(complexTrigger);
                }
                else if (simpleTrigger != null)
                {
                    gitHubActions.on = generalProcessing.ProcessSimpleTrigger(simpleTrigger);
                }
            }

            //Triggers for pull requests
            if (azurePipeline.pr != null)
            {
                GitHubActions.Trigger pr = generalProcessing.ProcessPullRequest(azurePipeline.pr);
                if (gitHubActions.on == null)
                {
                    gitHubActions.on = pr;
                }
                else
                {
                    gitHubActions.on.pull_request = pr.pull_request;
                }
            }

            //pool/demands
            if (azurePipeline.pool != null && azurePipeline.pool.demands != null)
            {
                gitHubActions.messages.Add("Note: GitHub Actions does not have a 'demands' command on 'runs-on' yet");
            }

            //schedules
            if (azurePipeline.schedules != null)
            {
                string[] schedules = generalProcessing.ProcessSchedules(azurePipeline.schedules);
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
                            //Move over the variables, the stage variables will need to be applied to each job
                            if (stage.variables != null && stage.variables.Count > 0)
                            {
                                azurePipeline.jobs[currentIndex].variables = new Dictionary<string, string>();
                                foreach (KeyValuePair<string, string> stageVariable in stage.variables)
                                {
                                    azurePipeline.jobs[currentIndex].variables.Add(stageVariable.Key, stageVariable.Value);
                                }
                            }
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
                            runs_on = generalProcessing.ProcessPool(azurePipeline.pool),
                            strategy = generalProcessing.ProcessStrategy(azurePipeline.strategy),
                            container = generalProcessing.ProcessContainer(azurePipeline.resources),
                            //resources = ProcessResources(azurePipeline.resources),
                            steps = generalProcessing.ProcessSteps(azurePipeline.steps)
                        }
                    }
                };
                MatrixVariableName = generalProcessing.MatrixVariableName;
            }

            //Variables
            if (azurePipeline.variables != null)
            {
                if (complexVariables != null)
                {
                    gitHubActions.env = generalProcessing.ProcessComplexVariables(complexVariables);
                    VariableList.AddRange(generalProcessing.VariableList);
                }
                else if (simpleVariables != null)
                {
                    gitHubActions.env = generalProcessing.ProcessSimpleVariables(simpleVariables);
                    VariableList.AddRange(generalProcessing.VariableList);
                }
            }
            else if (azurePipeline.parameters != null)
            {
                //For now, convert the parameters to variables
                gitHubActions.env = generalProcessing.ProcessSimpleVariables(azurePipeline.parameters);
            }

            return gitHubActions;
        }



        //process the jobs
        private Dictionary<string, GitHubActions.Job> ProcessJobs(AzurePipelines.Job[] jobs, Resources resources)
        {
            //A dictonary is perfect here, as the job_id (a string), must be unique in the action
            Dictionary<string, GitHubActions.Job> newJobs = null;
            if (jobs != null)
            {
                JobProcessing jobProcessing = new JobProcessing(_verbose);
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
                    newJobs.Add(jobName, jobProcessing.ProcessJob(jobs[i], resources));
                    MatrixVariableName = jobProcessing.MatrixVariableName;
                    VariableList.AddRange(jobProcessing.VariableList);
                }
            }
            return newJobs;
        }

    }
}
