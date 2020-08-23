using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace AzurePipelinesToGitHubActionsConverter.Core.Conversion.Serialization
{
    public static class JSONSerialization
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
