using Newtonsoft.Json;

namespace AnimalSerialization.Tests.Conversion
{
    public static class FarmSerialization
    {
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Serialize<T>(T item)
        {
            return JsonConvert.SerializeObject(item);
        }
    }
}
