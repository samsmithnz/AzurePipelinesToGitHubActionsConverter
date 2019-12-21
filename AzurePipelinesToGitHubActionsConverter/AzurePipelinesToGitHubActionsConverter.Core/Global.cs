using System;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core
{
    public static class Global
    {

        public static string GetHeaderComment()
        {
            return "# converted to GitHub Actions by https://github.com/samsmithnz/AzurePipelinesToGitHubActionsConverter on " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm:sstt");
        }

        public static string GetLineComment()
        {
            return "# WARNING: This line is unknown and may not have been migrated correctly";
        }

        public static string GenerateSpaces(int number)
        {
            return new String(' ', number);
        }

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
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull) //New as of YamlDotNet 8.0.0: https://github.com/aaubry/YamlDotNet/wiki/Serialization.Serializer#configuredefaultvalueshandlingdefaultvalueshandling
                .Build();
            string yaml = serializer.Serialize(obj);

            return yaml;
        }

    }
}
