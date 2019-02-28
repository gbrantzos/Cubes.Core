using Newtonsoft.Json;
using System;
using System.IO.Abstractions;

namespace Cubes.Core.Settings
{
    /// <summary>
    /// Setting provider based on Json files
    /// </summary>
    public class JsonFilesSettingsProvider : BaseFileSettingsProvider
    {
        private readonly JsonSerializerSettings jsonSettings;

        public JsonFilesSettingsProvider(string baseFolder) : this(baseFolder, new FileSystem()) { }
        public JsonFilesSettingsProvider(string baseFolder, IFileSystem fs) : base(baseFolder, fs)
        {
            // Settings for specific provider, we should not share global settings
            jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
         }

        protected override object ProviderSpecificDeserialize(Type settingsType, string fileName)
            => JsonConvert.DeserializeObject(fs.File.ReadAllText(fileName), settingsType, jsonSettings);

        protected override string ProviderSpecificSerialize(object settingsObj)
            => JsonConvert.SerializeObject(settingsObj, Formatting.Indented, jsonSettings);
    }
}