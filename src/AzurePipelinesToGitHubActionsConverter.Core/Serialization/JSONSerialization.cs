using System.Text.Json;
using System.IO;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.Serialization
{
    public static class JsonSerialization
    {
        public static JsonElement DeserializeStringToObject(string yaml)
        {
            //StringWriter sw = new StringWriter();
            StringReader sr = new StringReader(yaml);
            Deserializer deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(sr);
            //JsonSerializer serializer = new JsonSerializer();
            //serializer.Serialize(sw, yamlObject);
            //return JsonConvert.DeserializeObject<JObject>(sw.ToString());
            string processedYaml = JsonSerializer.Serialize(yamlObject);
            return JsonSerializer.Deserialize<JsonElement>(processedYaml);
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
