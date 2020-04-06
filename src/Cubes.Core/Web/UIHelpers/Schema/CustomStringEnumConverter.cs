using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cubes.Core.Web.UIHelpers.Schema
{
    public class CustomStringEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var toReturn = value.ToString().ToLower();
            writer.WriteValue(toReturn);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //var enumString = (string)reader.Value;

            // return Enum.Parse(typeof(ErrorCode), enumString, true);
            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}