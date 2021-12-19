using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using System.Collections.Generic;
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
                JsonElement jsonElement;
                AzurePipelines.Strategy newStrategy = new AzurePipelines.Strategy();
                if (strategyJson.TryGetProperty("parallel", out jsonElement))
                {
                    newStrategy.parallel = jsonElement.ToString();
                    ConversionUtility.WriteLine("Note that Parallel doesn't currently have an equivalent in Actions: " + newStrategy.parallel, _verbose);
                }
                if (strategyJson.TryGetProperty("matrix", out jsonElement))
                {
                    newStrategy.matrix = YamlSerialization.DeserializeYaml<Dictionary<string, Dictionary<string, string>>>(jsonElement.ToString());
                }
                if (strategyJson.TryGetProperty("maxParallel", out jsonElement))
                {
                    newStrategy.maxParallel = jsonElement.ToString();
                }
                if (strategyJson.TryGetProperty("runOnce", out jsonElement))
                {
                    newStrategy.runOnce = ProcessRunOnceStrategy(jsonElement);
                }
                if (strategyJson.TryGetProperty("canary", out jsonElement))
                {
                    newStrategy.canary = ProcessCanaryStrategy(jsonElement);
                }
                if (strategyJson.TryGetProperty("rolling", out jsonElement))
                {
                    newStrategy.rolling = ProcessRollingStrategy(jsonElement);
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
            JsonElement jsonElement;
            RunOnce runOnce = new RunOnce();
            if (strategyJson.TryGetProperty("preDeploy", out jsonElement))
            {
                runOnce.preDeploy = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that RunOnce>preDeploy isn't currently processed: " + runOnce.preDeploy.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("deploy", out jsonElement))
            {
                runOnce.deploy = ProcessDeploy(jsonElement);
            }
            if (strategyJson.TryGetProperty("routeTraffic", out jsonElement))
            {
                runOnce.routeTraffic = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that RunOnce>preDeploy isn't currently processed: " + runOnce.routeTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("postRouteTraffic", out jsonElement))
            {
                runOnce.postRouteTraffic = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that RunOnce>preDeploy isn't currently processed: " + runOnce.postRouteTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("on", out jsonElement))
            {
                runOnce.on = ProcessOn(jsonElement);
                ConversionUtility.WriteLine("Note that RunOnce>on isn't currently processed: " + runOnce.on.ToString(), _verbose);
            }
            return runOnce;
        }

        private Canary ProcessCanaryStrategy(JsonElement strategyJson)
        {
            JsonElement jsonElement;
            Canary canary = new Canary();
            if (strategyJson.TryGetProperty("increments", out jsonElement))
            {
                canary.increments = YamlSerialization.DeserializeYaml<int[]>(jsonElement.ToString());
                ConversionUtility.WriteLine("Note that canary>increments isn't currently processed: " + canary.increments.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("preDeploy", out jsonElement))
            {
                canary.preDeploy = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that canary>preDeploy isn't currently processed: " + canary.preDeploy.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("deploy", out jsonElement))
            {
                canary.deploy = ProcessDeploy(jsonElement);
            }
            if (strategyJson.TryGetProperty("routeTraffic", out jsonElement))
            {
                canary.routeTraffic = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that canary>routeTraffic isn't currently processed: " + canary.routeTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("postRouteTraffic", out jsonElement))
            {
                canary.postRouteTraffic = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that canary>postRouteTraffic isn't currently processed: " + canary.postRouteTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("on", out jsonElement))
            {
                canary.on = ProcessOn(jsonElement);
                ConversionUtility.WriteLine("Note that canary>on isn't currently processed: " + canary.on.ToString(), _verbose);
            }
            return canary;
        }

        private Rolling ProcessRollingStrategy(JsonElement strategyJson)
        {
            JsonElement jsonElement;
            Rolling rolling = new Rolling();
            if (strategyJson.TryGetProperty("maxParallel", out jsonElement) && int.TryParse(jsonElement.ToString(), out int maxParallel))
            {
                rolling.maxParallel = maxParallel;
                ConversionUtility.WriteLine("Note that rolling>maxParallel isn't currently processed: " + rolling.maxParallel.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("preDeploy", out jsonElement))
            {
                rolling.preDeploy = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that rolling>preDeploy isn't currently processed: " + rolling.preDeploy.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("deploy", out jsonElement))
            {
                rolling.deploy = ProcessDeploy(jsonElement);
            }
            if (strategyJson.TryGetProperty("routeTraffic", out jsonElement))
            {
                rolling.routeTraffic = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that rolling>routeTraffic isn't currently processed: " + rolling.routeTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("postRouteTraffic", out jsonElement))
            {
                rolling.postRouteTraffic = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that rolling>postRouteTraffic isn't currently processed: " + rolling.postRouteTraffic.ToString(), _verbose);
            }
            if (strategyJson.TryGetProperty("on", out jsonElement))
            {
                rolling.on = ProcessOn(jsonElement);
                ConversionUtility.WriteLine("Note that rolling>on isn't currently processed: " + rolling.on.ToString(), _verbose);
            }

            return rolling;
        }

        private Deploy ProcessDeploy(JsonElement deployJson)
        {
            JsonElement jsonElement;
            GeneralProcessing gp = new GeneralProcessing(_verbose);
            Deploy deploy = new Deploy();
            if (deployJson.TryGetProperty("pool", out jsonElement))
            {
                deploy.pool = gp.ProcessPoolV2(jsonElement.ToString());
            }
            if (deployJson.TryGetProperty("steps", out jsonElement))
            {
                deploy.steps = YamlSerialization.DeserializeYaml<AzurePipelines.Step[]>(jsonElement.ToString());
            }

            return deploy;
        }

        private On ProcessOn(JsonElement onJson)
        {
            JsonElement jsonElement;
            On on = new On();
            if (onJson.TryGetProperty("success", out jsonElement))
            {
                on.success = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that success isn't currently processed: " + on.success.ToString(), _verbose);
            }
            if (onJson.TryGetProperty("failure", out jsonElement))
            {
                on.failure = ProcessDeploy(jsonElement);
                ConversionUtility.WriteLine("Note that failure isn't currently processed: " + on.failure.ToString(), _verbose);
            }
            return on;
        }

    }
}
