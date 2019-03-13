using System;
using Newtonsoft.Json;

namespace Cubes.Core.Utilities
{
    public class JsonSerializer : ISerializer
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.All
            };

        public object Deserialize(string objAsString, Type targetType)
            => JsonConvert.DeserializeObject(objAsString, targetType, settings);

        public string Serialize(object objToSerialize)
            => JsonConvert.SerializeObject(objToSerialize, settings);
    }
}