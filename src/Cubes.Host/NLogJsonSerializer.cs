using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NLog.Common;

namespace Cubes.Host
{
    // Based on the following:
    // https://github.com/NLog/NLog/wiki/How-to-use-structured-logging#i-like-to-use-jsonnet-for-creating-json
    public class NLogJsonSerializer : NLog.IJsonConverter
    {
        private readonly JsonSerializerSettings _settings;
        private readonly JsonSerializer _jsonSerializer;

        public NLogJsonSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            _jsonSerializer = JsonSerializer.CreateDefault(_settings);
            _settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        /// <summary>Serialization of an object into JSON format.</summary>
        /// <param name="value">The object to serialize to JSON.</param>
        /// <param name="builder">Output destination.</param>
        /// <returns>Serialize succeeded (true/false)</returns>
        public bool SerializeObject(object value, StringBuilder builder)
        {
            try
            {
                using var sw = new StringWriter(builder, CultureInfo.InvariantCulture);
                using var jsonWriter = new JsonTextWriter(sw);

                jsonWriter.Formatting = _jsonSerializer.Formatting;
                _jsonSerializer.Serialize(jsonWriter, value, null);
            }
            catch (Exception e)
            {
                InternalLogger.Error(e, "[NLog] Error using custom JSON serialization");
                return false;
            }
            return true;
        }
    }
}
