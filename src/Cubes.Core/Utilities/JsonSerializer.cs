using System;
using Cubes.Core.Base;
using Newtonsoft.Json;

namespace Cubes.Core.Utilities
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings settings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include
            };

        public object Deserialize(string objAsString, Type targetType)
            => JsonConvert.DeserializeObject(objAsString, targetType, settings);

        public string Serialize(object objToSerialize)
            => JsonConvert.SerializeObject(objToSerialize, settings);

        public string Format => CubesConstants.Serializer_JSON;
    }
}
