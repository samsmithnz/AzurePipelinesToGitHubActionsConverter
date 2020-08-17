using YamlDotNet.Serialization;

namespace AnimalSerialization.Tests.Conversion
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class FarmSerialization
    {
        public static T Deserialize<T>(string yaml)
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<T>(yaml);
        }

        public static string Serialize<T>(T obj)
        {
            ISerializer serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults) //New as of YamlDotNet 8.0.0: https://github.com/aaubry/YamlDotNet/wiki/Serialization.Serializer#configuredefaultvalueshandlingdefaultvalueshandling. This will not show null properties, e.g. "app-name: " will not display when the value is null, as the value is nullable
                .Build();
            return serializer.Serialize(obj);
        }
    }
}
