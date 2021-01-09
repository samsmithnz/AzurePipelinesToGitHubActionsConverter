using System.Text.Json;
using System.IO;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.Serialization
{
    public static class JsonSerialization
    {
        public static JsonElement DeserializeStringToJsonElement(string yaml)
        {
            StringReader sr = new StringReader(yaml);
            Deserializer deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(sr);
            string processedYaml = JsonSerializer.Serialize(yamlObject);
            return JsonSerializer.Deserialize<JsonElement>(processedYaml);
        }

        public static T DeserializeStringToObject<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        //From: https://stackoverflow.com/a/36125258/337421
        public static bool JsonCompare(this object obj, object another)
        {
            string objJson = JsonSerializer.Serialize(obj);
            string anotherJson = JsonSerializer.Serialize(another);

            return objJson == anotherJson;
        }
    }
}
