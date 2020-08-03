using System;
using System.Diagnostics;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization
{
    public static class GenericObjectSerialization
    {
        //Read in a YAML file and convert it to a T object
        public static T DeserializeYaml<T>(string yaml)
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            T yamlObject = deserializer.Deserialize<T>(yaml);

            //Commented out the new solution, as it doesn't process failed/invalid documents.
            //If we leave this in, then errors are returned to the call stack root. 
            //If we capture the error, we will capture a lot of false negatives,
            // as we have to try a few YAML combinations depending on the trigger and pool

            //T yamlObject = default;
            //IDeserializer deserializer = new DeserializerBuilder().Build();
            //try
            //{
            //    yamlObject = deserializer.Deserialize<T>(yaml);
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex);
            //    //hide any exception for the moment
            //}
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