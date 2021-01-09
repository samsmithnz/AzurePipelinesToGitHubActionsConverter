using AzurePipelinesToGitHubActionsConverter.Core.AzurePipelines;
using AzurePipelinesToGitHubActionsConverter.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace AzurePipelinesToGitHubActionsConverter.Core.PipelinesToActionsConversion
{
    public class GeneralProcessing
    {
        public string MatrixVariableName;
        private readonly bool _verbose;
        public GeneralProcessing(bool verbose)
        {
            _verbose = verbose;
        }

        public string ProcessNameV2(string nameYaml)
        {
            return nameYaml.Replace("name:", "").Replace(System.Environment.NewLine, "").Trim();
        }

        public string[] ProcessDependsOnV2(string dependsOnYaml)
        {
            string[] dependsOn = null;
            if (dependsOnYaml != null)
            {
                try
                {
                    string simpleDependsOn = YamlSerialization.DeserializeYaml<string>(dependsOnYaml);
                    dependsOn = new string[1];
                    dependsOn[0] = simpleDependsOn;
                }
                catch (Exception ex)
                {
                    ConversionUtility.WriteLine($"DeserializeYaml<string>(dependsOnYaml) swallowed an exception: " + ex.Message, _verbose);
                    dependsOn = YamlSerialization.DeserializeYaml<string[]>(dependsOnYaml);
                }
            }

            //Build the return results
            return dependsOn;
        }

        public AzurePipelines.Environment ProcessEnvironmentV2(string environmentYaml)
        {
            AzurePipelines.Environment environment = null;
            if (environmentYaml != null)
            {
                try
                {
                    environment = YamlSerialization.DeserializeYaml<AzurePipelines.Environment>(environmentYaml);
                }
                catch (Exception ex1)
                {
                    ConversionUtility.WriteLine($"DeserializeYaml<AzurePipelines.Environment>(environmentYaml) swallowed an exception: " + ex1.Message, _verbose);

                    try
                    {
                        //when the environment is just a simple string, e.g.  //environment: environmentName.resourceName
                        string simpleEnvironment = YamlSerialization.DeserializeYaml<string>(environmentYaml);
                        environment = new AzurePipelines.Environment
                        {
                            name = simpleEnvironment
                        };
                    }
                    catch (Exception ex2)
                    {
                        ConversionUtility.WriteLine($"Deserializing (simple) environment failed. Let's try a complex deserialization with JSON. Error: " + ex2.Message, _verbose);
                        JsonElement json = JsonSerialization.DeserializeStringToObject(environmentYaml);
                        if (json.GetProperty("name").ToString() != null)
                        {
                            environment = new AzurePipelines.Environment
                            {
                                name = json.GetProperty("name").ToString()
                            };
                        }
                        if (json.GetProperty("resourceName").ToString() != null)
                        {
                            environment.resourceName = json.GetProperty("resourceName").ToString();
                            ConversionUtility.WriteLine($"No conversion for resourceName at this time: " + environment.resourceName, _verbose);
                        }
                        if (json.GetProperty("resourceId").ToString() != null)
                        {
                            environment.resourceId = json.GetProperty("resourceId").ToString();
                            ConversionUtility.WriteLine($"No conversion for resourceId at this time: " + environment.resourceId, _verbose);
                        }
                        if (json.GetProperty("resourceType").ToString() != null)
                        {
                            environment.resourceType = json.GetProperty("resourceType").ToString();
                            ConversionUtility.WriteLine($"No conversion for resourceType at this time: " + environment.resourceType, _verbose);
                        }
                        if (json.GetProperty("tags").GetType() == Type.GetType("string"))
                        {
                            //Move the single string demands to an array
                            environment.tags = new string[1];
                            environment.tags[0] = json.GetProperty("tags").ToString();
                        }
                        //else
                        //{
                        //    ConversionUtility.WriteLine($"Manual deserialization with demands string swallowed an exception: " + ex2.Message, _verbose);
                        //}
                    }
                }
            }

            return environment;
        }

        public Resources ExtractResourcesV2(string resourcesYaml)
        {
            if (resourcesYaml != null)
            {
                try
                {
                    Resources resources = YamlSerialization.DeserializeYaml<Resources>(resourcesYaml);
                    return resources;
                }
                catch (Exception ex)
                {
                    ConversionUtility.WriteLine($"DeserializeYaml<Resources>(resourcesYaml) swallowed an exception: " + ex.Message, _verbose);
                }
            }
            return null;
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

        public Pool ProcessPoolV2(string poolYaml)
        {
            Pool pool = null;
            if (poolYaml != null)
            {

                //try
                //{
                //    //Most often, the pool will be in this structure
                //    pool = YamlSerialization.DeserializeYaml<Pool>(poolYaml);
                //}
                //catch (Exception ex)
                //{
                //    ConversionUtility.WriteLine($"DeserializeYaml<Pool>(poolYaml) swallowed an exception: " + ex.Message, _verbose);
                //    //If it's a simple pool string, and has no json in it, assign it to the name
                //    if (poolYaml.IndexOf("{") < 0)
                //    {
                //        pool = new Pool
                //        {
                //            name = poolYaml
                //        };
                //    }
                //    else
                //    {
                //otherwise, demands is probably a string, instead of string[], let's fix it
                JsonElement poolJson = JsonSerialization.DeserializeStringToObject(poolYaml);
                JsonElement jsonElement = new JsonElement();
                if (poolJson.TryGetProperty("demands", out jsonElement) == true)// & poolJson.GetProperty("demands").GetType() == Type.GetType("string"))
                {
                    string name = null;
                    if (poolJson.TryGetProperty("name", out jsonElement) == true)
                    {
                        name = poolJson.GetProperty("name").ToString();
                    }
                    string vmImage = null;
                    if (poolJson.TryGetProperty("vmImage", out jsonElement) == true)
                    {
                        vmImage = poolJson.GetProperty("vmImage").ToString();
                    }
                    string demands = null;
                    if (poolJson.TryGetProperty("demands", out jsonElement) == true)
                    {
                        demands = poolJson.GetProperty("demands").ToString();
                    }
                    pool = new Pool
                    {
                        name = name,
                        vmImage = vmImage
                    };
                    //Move the single string demands to an array
                    pool.demands = new string[1];
                    pool.demands[0] = demands;
                }
                //else
                //{
                //    ConversionUtility.WriteLine($"Manual deserialization with demands string swallowed an exception: " + ex.Message, _verbose);
                //}
                //}
                //}
            }
            return pool;
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
                //TODO: There is currently no conversion path for other strategies    
                //if (strategy.runOnce != null)
                //{
                //    ConversionUtility.WriteLine("TODO: " + strategy.runOnce, _verbose);
                //}
                //if (strategy.canary != null)
                //{
                //    ConversionUtility.WriteLine("TODO: " + strategy.canary, _verbose);
                //}
                //if (strategy.rolling != null)
                //{
                //    ConversionUtility.WriteLine("TODO: " + strategy.rolling, _verbose);
                //}
                return processedStrategy;
            }
            else
            {
                return null;
            }
        }

        public GitHubActions.Container ProcessContainer(Resources resources)
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
                GitHubActions.Container container = new GitHubActions.Container
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

                //Couple properties not used yet
                if (resources.containers[0].container != null)
                {
                    Debug.WriteLine("container property not used: " + resources.containers[0].container);
                }
                //if (resources.containers[0].endpoint != null)
                //{
                //    Debug.WriteLine("endpoint property not used: " + resources.containers[0].endpoint);
                //}

                return container;
            }
            else
            {
                return null;
            }
        }


    }
}
