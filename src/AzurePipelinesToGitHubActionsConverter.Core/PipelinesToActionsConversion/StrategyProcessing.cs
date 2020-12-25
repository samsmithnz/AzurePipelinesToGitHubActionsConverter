using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public class StrategyProcessing
    {
        private readonly bool _verbose;
        public StrategyProcessing(bool verbose)
        {
            _verbose = verbose;
        }

        public Strategy ProcessStrategyV2(JToken strategyJson)
        {
            if (strategyJson != null)
            {
                AzurePipelines.Strategy newStrategy = new AzurePipelines.Strategy();
                if (strategyJson["parallel"] != null)
                {
                    newStrategy.parallel = strategyJson["parallel"].ToString();
                    ConversionUtility.WriteLine("Note that Parallel doesn't currently have an equivalent in Actions: " + newStrategy.parallel, _verbose);
                }
                if (strategyJson["matrix"] != null)
                {
                    newStrategy.matrix = YamlSerialization.DeserializeYaml<Dictionary<string, Dictionary<string, string>>>(strategyJson["matrix"].ToString());
                }
                if (strategyJson["maxParallel"] != null)
                {
                    newStrategy.maxParallel = strategyJson["maxParallel"].ToString();
                }
                if (strategyJson["runOnce"] != null)
                {
                    newStrategy.runOnce = ProcessRunOnceStrategy(strategyJson["runOnce"]);
                }
                if (strategyJson["canary"] != null)
                {
                    newStrategy.canary = ProcessCanaryStrategy(strategyJson["canary"]);
                }
                if (strategyJson["rolling"] != null)
                {
                    newStrategy.rolling = ProcessRollingStrategy(strategyJson["rolling"]);
                }
                return newStrategy;
            }
            else
            {
                return null;
            }
        }

        private RunOnce ProcessRunOnceStrategy(JToken strategyJson)
        {
            RunOnce runOnce = new RunOnce();
            if (strategyJson["preDeploy"] != null)
            {
                runOnce.preDeploy = ProcessDeploy(strategyJson["preDeploy"]);
                ConversionUtility.WriteLine("Note that RunOnce>preDeploy isn't currently processed: " + runOnce.preDeploy.ToString(), _verbose);
            }
            if (strategyJson["deploy"] != null)
            {
                runOnce.deploy = ProcessDeploy(strategyJson["deploy"]);
            }
            if (strategyJson["routeTraffic"] != null)
            {
                runOnce.routeTraffic = ProcessDeploy(strategyJson["routeTraffic"]);
                ConversionUtility.WriteLine("Note that RunOnce>preDeploy isn't currently processed: " + runOnce.routeTraffic.ToString(), _verbose);
            }
            if (strategyJson["postRouteTraffic"] != null)
            {
                runOnce.postRouteTraffic = ProcessDeploy(strategyJson["postRouteTraffic"]);
                ConversionUtility.WriteLine("Note that RunOnce>preDeploy isn't currently processed: " + runOnce.postRouteTraffic.ToString(), _verbose);
            }
            if (strategyJson["on"] != null)
            {
                runOnce.on = ProcessOn(strategyJson["on"]);
                ConversionUtility.WriteLine("Note that RunOnce>on isn't currently processed: " + runOnce.on.ToString(), _verbose);
            }
            return runOnce;
        }

        private Canary ProcessCanaryStrategy(JToken strategyJson)
        {
            Canary canary = new Canary();
            if (strategyJson["increments"] != null)
            {
                canary.increments = YamlSerialization.DeserializeYaml<int[]>(strategyJson["increments"].ToString());
                ConversionUtility.WriteLine("Note that canary>increments isn't currently processed: " + canary.increments.ToString(), _verbose);
            }
            if (strategyJson["preDeploy"] != null)
            {
                canary.preDeploy = ProcessDeploy(strategyJson["preDeploy"]);
                ConversionUtility.WriteLine("Note that canary>preDeploy isn't currently processed: " + canary.preDeploy.ToString(), _verbose);
            }
            if (strategyJson["deploy"] != null)
            {
                canary.deploy = ProcessDeploy(strategyJson["deploy"]);
            }
            if (strategyJson["routeTraffic"] != null)
            {
                canary.routeTraffic = ProcessDeploy(strategyJson["routeTraffic"]);
                ConversionUtility.WriteLine("Note that canary>routeTraffic isn't currently processed: " + canary.routeTraffic.ToString(), _verbose);
            }
            if (strategyJson["postRouteTraffic"] != null)
            {
                canary.postRouteTraffic = ProcessDeploy(strategyJson["postRouteTraffic"]);
                ConversionUtility.WriteLine("Note that canary>postRouteTraffic isn't currently processed: " + canary.postRouteTraffic.ToString(), _verbose);
            }
            if (strategyJson["on"] != null)
            {
                canary.on = ProcessOn(strategyJson["on"]);
                ConversionUtility.WriteLine("Note that canary>on isn't currently processed: " + canary.on.ToString(), _verbose);
            }
            return canary;
        }

        private Rolling ProcessRollingStrategy(JToken strategyJson)
        {
            Rolling rolling = new Rolling();
            if (strategyJson["maxParallel"] != null)
            {
                rolling.maxParallel = (int)(strategyJson["maxParallel"]);
                ConversionUtility.WriteLine("Note that rolling>maxParallel isn't currently processed: " + rolling.maxParallel.ToString(), _verbose);
            }
            if (strategyJson["preDeploy"] != null)
            {
                rolling.preDeploy = ProcessDeploy(strategyJson["preDeploy"]);
                ConversionUtility.WriteLine("Note that rolling>preDeploy isn't currently processed: " + rolling.preDeploy.ToString(), _verbose);
            }
            if (strategyJson["deploy"] != null)
            {
                rolling.deploy = ProcessDeploy(strategyJson["deploy"]);
            }
            if (strategyJson["routeTraffic"] != null)
            {
                rolling.routeTraffic = ProcessDeploy(strategyJson["routeTraffic"]);
                ConversionUtility.WriteLine("Note that rolling>routeTraffic isn't currently processed: " + rolling.routeTraffic.ToString(), _verbose);
            }
            if (strategyJson["postRouteTraffic"] != null)
            {
                rolling.postRouteTraffic = ProcessDeploy(strategyJson["postRouteTraffic"]);
                ConversionUtility.WriteLine("Note that rolling>postRouteTraffic isn't currently processed: " + rolling.postRouteTraffic.ToString(), _verbose);
            }
            if (strategyJson["on"] != null)
            {
                rolling.on = ProcessOn(strategyJson["on"]);
                ConversionUtility.WriteLine("Note that rolling>on isn't currently processed: " + rolling.on.ToString(), _verbose);
            }

            return rolling;
        }

        private Deploy ProcessDeploy(JToken deployJson)
        {
            GeneralProcessing gp = new GeneralProcessing(_verbose);
            Deploy deploy = new Deploy();
            if (deployJson["pool"] != null)
            {
                deploy.pool = gp.ProcessPoolV2(deployJson["pool"].ToString());
            }
            if (deployJson["steps"] != null)
            {
                //try
                //{
                deploy.steps = YamlSerialization.DeserializeYaml<AzurePipelines.Step[]>(deployJson["steps"].ToString());
                //}
                //catch (Exception ex)
                //{
                //    ConversionUtility.WriteLine($"DeserializeYaml<AzurePipelines.Step[]>(ProcessDeploy[\"steps\"].ToString() swallowed an exception: " + ex.Message, _verbose);
                //}
            }

            return deploy;
        }

        private On ProcessOn(JToken onJson)
        {
            On on = new On();
            if (onJson["success"] != null)
            {
                on.success = ProcessDeploy(onJson["success"]);
                ConversionUtility.WriteLine("Note that success isn't currently processed: " + on.success.ToString(), _verbose);
            }
            if (onJson["failure"] != null)
            {
                on.failure = ProcessDeploy(onJson["failure"]);
                ConversionUtility.WriteLine("Note that failure isn't currently processed: " + on.failure.ToString(), _verbose);
            }
            return on;
        }

    }
}
