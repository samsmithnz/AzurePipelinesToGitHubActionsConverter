using System.Text.Json;
using System.IO;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.Serialization
{
    public static class JsonSerialization
    {
        public static JsonElement DeserializeStringToJsonElement(string yaml)
        {
            //This looks inefficient, but helps to ensure the Yaml is clean and well formed
            //first we convert the yaml to a object 
            StringReader sr = new StringReader(yaml);
            Deserializer deserializer = new Deserializer();
            object yamlObject = deserializer.Deserialize(sr);
            //then convert the object back to yaml again to format the yaml
            string processedYaml = JsonSerializer.Serialize(yamlObject);
            //Finally we can return a JsonElement object from the processed Yaml. 
            return JsonSerializer.Deserialize<JsonElement>(processedYaml);
        }

        //From: https://stackoverflow.com/a/36125258/337421
        public static bool JsonCompare(this object obj, object another)
        {
            //Serialize two objects as strings
            string objJson = JsonSerializer.Serialize(obj);
            string anotherJson = JsonSerializer.Serialize(another);
            //Compare the strings
            return objJson == anotherJson;
        }
    }
}
