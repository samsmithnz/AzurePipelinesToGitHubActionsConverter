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

        //From: https://stackoverflow.com/a/36125258/337421
        public static bool JsonCompare(this object obj, object another)
        {
            //if (ReferenceEquals(obj, another))
            //{
            //    return true;
            //}
            //if ((obj == null) || (another == null))
            //{
            //    return false;
            //}
            //if (obj.GetType() != another.GetType())
            //{
            //    return false;
            //}

            var objJson = JsonConvert.SerializeObject(obj);
            var anotherJson = JsonConvert.SerializeObject(another);

            return objJson == anotherJson;
        }
    }
}
