using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.Serialization
{
    public static class JsonSerialization
    {
        public static JObject DeserializeStringToObject(string yaml)
        {
            StringWriter sw = new StringWriter();
            StringReader sr = new StringReader(yaml);
            Deserializer deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(sr);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(sw, yamlObject);
            return JsonConvert.DeserializeObject<JObject>(sw.ToString());
        }
    }
}
