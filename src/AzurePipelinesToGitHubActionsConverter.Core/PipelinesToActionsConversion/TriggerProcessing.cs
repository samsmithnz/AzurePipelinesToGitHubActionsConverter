using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using GitHubActionsDotNet.Models;
using System;
using GitHubActions = GitHubActionsDotNet.Models;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public class TriggerProcessing
    {
        private readonly bool _verbose;
        public TriggerProcessing(bool verbose)
        {
            _verbose = verbose;
        }

        //Process a complex trigger, using the Trigger object
        private GitHubActions.Trigger ProcessComplexTrigger(AzurePipelines.Trigger trigger)
        {
            //Note: as of ~18-Oct-2020, GitHub Actions was updated, and you now you receive an error if you try to use both a "branches" and a "ignore-branches", or a "paths and a ignore-paths". You can only have one or the other.
            TriggerDetail push = new TriggerDetail();
            if (trigger != null)
            {
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
                if (trigger.batch != null)
                {
                    ConversionUtility.WriteLine($"This trigger contains a batch property, that Actions does not currently have a conversion path for", _verbose);
                }
                if (trigger.autoCancel != null)
                {
                    ConversionUtility.WriteLine($"This trigger contains a autoCancel property, that Actions does not currently have a conversion path for", _verbose);
                }
            }

            return new GitHubActions.Trigger
            {
                push = push
            };

        }

        public GitHubActions.Trigger ProcessTriggerV2(string triggerYaml)
        {
            AzurePipelines.Trigger trigger = null;
            if (triggerYaml != null)
            {
                try
                {
                    string[] simpleTrigger = YamlSerialization.DeserializeYaml<string[]>(triggerYaml);
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
                    ConversionUtility.WriteLine($"DeserializeYaml<string[]>(triggerYaml) swallowed an exception: " + ex.Message, _verbose);
                    trigger = YamlSerialization.DeserializeYaml<AzurePipelines.Trigger>(triggerYaml);
                }
            }

            //Convert the pieces to GitHub
            GitHubActions.Trigger push = ProcessComplexTrigger(trigger);

            //Build the return results
            return push;
            //return new GitHubActions.Trigger
            //{
            //    push = push?.push
            //};

        }

        //process the pull request
        public GitHubActions.Trigger ProcessPullRequest(AzurePipelines.Trigger pr)
        {
            TriggerDetail pullRequest = new TriggerDetail();
            if (pr != null)
            {
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
            }

            return new GitHubActions.Trigger
            {
                pull_request = pullRequest
            };
        }

        public GitHubActions.Trigger ProcessPullRequestV2(string pullRequestYaml)
        {
            AzurePipelines.Trigger trigger = null;
            if (pullRequestYaml != null)
            {
                try
                {
                    string[] simpleTrigger = YamlSerialization.DeserializeYaml<string[]>(pullRequestYaml);
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
                    ConversionUtility.WriteLine($"DeserializeYaml<string[]>(pullRequestYaml) swallowed an exception: " + ex.Message, _verbose);
                    trigger = YamlSerialization.DeserializeYaml<AzurePipelines.Trigger>(pullRequestYaml);
                }
            }

            //Convert the pieces to GitHub
            GitHubActions.Trigger pr = ProcessPullRequest(trigger);

            //Build the return results
            return new GitHubActions.Trigger
            {
                pull_request = pr.pull_request
            };

        }

        //process the schedule
        public string[] ProcessSchedules(AzurePipelines.Schedule[] schedules)
        {
            //if (schedules == null)
            //{
            //    return null;
            //}
            string[] newSchedules = new string[schedules.Length];
            for (int i = 0; i < schedules.Length; i++)
            {
                newSchedules[i] = "cron: '" + schedules[i].cron + "'";
            }

            return newSchedules;
        }

        public GitHubActions.Trigger ProcessSchedulesV2(string schedulesYaml)
        {
            Schedule[] schedules = null;
            if (schedulesYaml != null)
            {
                schedules = YamlSerialization.DeserializeYaml<Schedule[]>(schedulesYaml);
            }

            //Convert the pieces to GitHub
            string[] schedule = ProcessSchedules(schedules);

            //Build the return results
            GitHubActions.Trigger newSchedule = null;
            if (schedule != null)
            {
                newSchedule = new GitHubActions.Trigger
                {
                    schedule = schedule
                };
            }
            return newSchedule;
        }

    }
}
