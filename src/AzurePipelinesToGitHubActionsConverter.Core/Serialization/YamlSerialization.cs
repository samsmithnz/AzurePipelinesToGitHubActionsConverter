using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.Serialization
{
    public static class YamlSerialization
    {
        //Read in a YAML file and convert it to a T object
        public static T DeserializeYaml<T>(string yaml)
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            T yamlObject = deserializer.Deserialize<T>(yaml);
            return yamlObject;
        }

        //Write a YAML file using the T object
        public static string SerializeYaml<T>(T obj)
        {
            //Convert the object into a YAML document
            ISerializer serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults) //New as of YamlDotNet 8.0.0: https://github.com/aaubry/YamlDotNet/wiki/Serialization.Serializer#configuredefaultvalueshandlingdefaultvalueshandling. This will not show null properties, e.g. "app-name: " will not display when the value is null, as the value is nullable
                .Build();
            string yaml = serializer.Serialize(obj);

            return yaml;
        }
    }
}