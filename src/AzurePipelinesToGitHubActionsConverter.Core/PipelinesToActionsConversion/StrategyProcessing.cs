using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public class StrategyProcessing
    {
        private readonly bool _verbose;
        public StrategyProcessing(bool verbose)
        {
            _verbose = verbose;
        }

        public Strategy ProcessStrategyV2(JsonElement strategyJson)
        {
            if (strategyJson.ValueKind != JsonValueKind.Undefined)
            {
                JsonElement jsonElement = new JsonElement();
                AzurePipelines.Strategy newStrategy = new AzurePipelines.Strategy();
                if (strategyJson.TryGetProperty("parallel", out jsonElement) == true)
                {
                    newStrategy.parallel = strategyJson.GetProperty("parallel").ToString();
                    ConversionUtility.WriteLine("Note that Parallel doesn't currently have an equivalent in Actions: " + newStrategy.parallel, _verbose);
                }
                if (strategyJson.TryGetProperty("matrix", out jsonElement) == true)
                {
                    newStrategy.matrix = YamlSerialization.DeserializeYaml<Dictionary<string, Dictionary<string, string>>>(strategyJson.GetProperty("matrix").ToString());
                }
                if (strategyJson.TryGetProperty("maxParallel", out jsonElement) == true)
                {
                    newStrategy.maxParallel = strategyJson.GetProperty("maxParallel").ToString();
                }
                if (strategyJson.TryGetProperty("runOnce", out jsonElement) == true)
                {
                    newStrategy.runOnce = ProcessRunOnceStrategy(strategyJson.GetProperty("runOnce"));
                }
                if (strategyJson.TryGetProperty("canary", out jsonElement) == true)
                {
                    newStrategy.canary = ProcessCanaryStrategy(strategyJson.GetProperty("canary"));
                }
                if (strategyJson.TryGetProperty("rolling", out jsonElement) == true)
                {
                    newStrategy.rolling = ProcessRollingStrategy(strategyJson.GetProperty("rolling"));
                }
                return newStrategy;
            }
            else
            {
                return null;
            }
        }

        private RunOnce ProcessRunOnceStrategy(JsonElement strategyJson)
        {
            JsonElement jsonElement = new JsonElement();
            RunOnce runOnce = new RunOnce();
            if (strategyJson.TryGetProperty("preDeploy", out jsonElement) == true)
            {
                runOnce.preDeploy = ProcessDeploy(strategyJson.GetProperty("preDeploy"));
                ConversionUtility.WriteLine("Note that RunOnce>preDeploy isn't currently processed: " + runOnce.preDeploy.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("deploy", out jsonElement) == true)
            {
                runOnce.deploy = ProcessDeploy(strategyJson.GetProperty("deploy"));
            }
            if (strategyJson.TryGetProperty("routeTraffic", out jsonElement) == true)
            {
                runOnce.routeTraffic = ProcessDeploy(strategyJson.GetProperty("routeTraffic"));
                ConversionUtility.WriteLine("Note that RunOnce>preDeploy isn't currently processed: " + runOnce.routeTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("postRouteTraffic", out jsonElement) == true)
            {
                runOnce.postRouteTraffic = ProcessDeploy(strategyJson.GetProperty("postRouteTraffic"));
                ConversionUtility.WriteLine("Note that RunOnce>preDeploy isn't currently processed: " + runOnce.postRouteTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("on", out jsonElement) == true)
            {
                runOnce.on = ProcessOn(strategyJson.GetProperty("on"));
                ConversionUtility.WriteLine("Note that RunOnce>on isn't currently processed: " + runOnce.on.ToString(), _verbose);
            }
            return runOnce;
        }

        private Canary ProcessCanaryStrategy(JsonElement strategyJson)
        {
            JsonElement jsonElement = new JsonElement();
            Canary canary = new Canary();
            if (strategyJson.TryGetProperty("increments", out jsonElement) == true)
            {
                canary.increments = YamlSerialization.DeserializeYaml<int[]>(strategyJson.GetProperty("increments").ToString());
                ConversionUtility.WriteLine("Note that canary>increments isn't currently processed: " + canary.increments.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("preDeploy", out jsonElement) == true)
            {
                canary.preDeploy = ProcessDeploy(strategyJson.GetProperty("preDeploy"));
                ConversionUtility.WriteLine("Note that canary>preDeploy isn't currently processed: " + canary.preDeploy.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("deploy", out jsonElement) == true)
            {
                canary.deploy = ProcessDeploy(strategyJson.GetProperty("deploy"));
            }
            if (strategyJson.TryGetProperty("routeTraffic", out jsonElement) == true)
            {
                canary.routeTraffic = ProcessDeploy(strategyJson.GetProperty("routeTraffic"));
                ConversionUtility.WriteLine("Note that canary>routeTraffic isn't currently processed: " + canary.routeTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("postRouteTraffic", out jsonElement) == true)
            {
                canary.postRouteTraffic = ProcessDeploy(strategyJson.GetProperty("postRouteTraffic"));
                ConversionUtility.WriteLine("Note that canary>postRouteTraffic isn't currently processed: " + canary.postRouteTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("on", out jsonElement) == true)
            {
                canary.on = ProcessOn(strategyJson.GetProperty("on"));
                ConversionUtility.WriteLine("Note that canary>on isn't currently processed: " + canary.on.ToString(), _verbose);
            }
            return canary;
        }

        private Rolling ProcessRollingStrategy(JsonElement strategyJson)
        {
            JsonElement jsonElement = new JsonElement();
            Rolling rolling = new Rolling();
            if (strategyJson.TryGetProperty("maxParallel", out jsonElement) == true)
            {
                if (int.TryParse(strategyJson.GetProperty("maxParallel").ToString(), out int maxParallel) == true)
                {
                    rolling.maxParallel = maxParallel;
                    ConversionUtility.WriteLine("Note that rolling>maxParallel isn't currently processed: " + rolling.maxParallel.ToString(), _verbose);
                }
            }
            if (strategyJson.TryGetProperty("preDeploy", out jsonElement) == true)
            {
                rolling.preDeploy = ProcessDeploy(strategyJson.GetProperty("preDeploy"));
                ConversionUtility.WriteLine("Note that rolling>preDeploy isn't currently processed: " + rolling.preDeploy.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("deploy", out jsonElement) == true)
            {
                rolling.deploy = ProcessDeploy(strategyJson.GetProperty("deploy"));
            }
            if (strategyJson.TryGetProperty("routeTraffic", out jsonElement) == true)
            {
                rolling.routeTraffic = ProcessDeploy(strategyJson.GetProperty("routeTraffic"));
                ConversionUtility.WriteLine("Note that rolling>routeTraffic isn't currently processed: " + rolling.routeTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("postRouteTraffic", out jsonElement) == true)
            {
                rolling.postRouteTraffic = ProcessDeploy(strategyJson.GetProperty("postRouteTraffic"));
                ConversionUtility.WriteLine("Note that rolling>postRouteTraffic isn't currently processed: " + rolling.postRouteTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("on", out jsonElement) == true)
            {
                rolling.on = ProcessOn(strategyJson.GetProperty("on"));
                ConversionUtility.WriteLine("Note that rolling>on isn't currently processed: " + rolling.on.ToString(), _verbose);
            }

            return rolling;
        }

        private Deploy ProcessDeploy(JsonElement deployJson)
        {
            JsonElement jsonElement = new JsonElement();
            GeneralProcessing gp = new GeneralProcessing(_verbose);
            Deploy deploy = new Deploy();
            if (deployJson.TryGetProperty("pool", out jsonElement) == true)
            {
                deploy.pool = gp.ProcessPoolV2(deployJson.GetProperty("pool").ToString());
            }
            if (deployJson.TryGetProperty("steps", out jsonElement) == true)
            {
                //try
                //{
                deploy.steps = YamlSerialization.DeserializeYaml<AzurePipelines.Step[]>(deployJson.GetProperty("steps").ToString());
                //}
                //catch (Exception ex)
                //{
                //    ConversionUtility.WriteLine($"DeserializeYaml<AzurePipelines.Step[]>(ProcessDeploy[\"steps\"].ToString() swallowed an exception: " + ex.Message, _verbose);
                //}
            }

            return deploy;
        }

        private On ProcessOn(JsonElement onJson)
        {
            JsonElement jsonElement = new JsonElement();
            On on = new On();
            if (onJson.TryGetProperty("success", out jsonElement) == true)
            {
                on.success = ProcessDeploy(onJson.GetProperty("success"));
                ConversionUtility.WriteLine("Note that success isn't currently processed: " + on.success.ToString(), _verbose);
            }
            if (onJson.TryGetProperty("failure", out jsonElement) == true)
            {
                on.failure = ProcessDeploy(onJson.GetProperty("failure"));
                ConversionUtility.WriteLine("Note that failure isn't currently processed: " + on.failure.ToString(), _verbose);
            }
            return on;
        }

    }
}
