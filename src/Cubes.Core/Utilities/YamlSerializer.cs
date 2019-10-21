using System;
using YamlDotNet.Serialization;

namespace Cubes.Core.Utilities
{
    public class YamlSerializer : ISerializer
    {
        private static YamlDotNet.Serialization.ISerializer Serializer { get; } = new SerializerBuilder()
            .EmitDefaults()
            .Build();
        private static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        public string Serialize(object objToSerialize)
            => Serializer.Serialize(objToSerialize);

        public object Deserialize(string objAsString, Type targetType)
            => Deserializer.Deserialize(objAsString, targetType);
    }
}
