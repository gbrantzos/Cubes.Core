using Cubes.Core.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO.Abstractions;
using System.Linq;

namespace Cubes.Core.Settings
{
    /// <summary>
    /// Setting provider based on Json files
    /// </summary>
    public class JsonFilesSettingsProvider : ISettingsProvider
    {
        private readonly string baseFolder;
        private readonly JsonSerializerSettings jsonSettings;
        private ConcurrentDictionary<string, Tuple<DateTime, object>> cache;
        private FileWatcherExtended fwe;
        private IFileSystem fs;

        public JsonFilesSettingsProvider(string baseFolder) : this(baseFolder, new FileSystem()) { }
        public JsonFilesSettingsProvider(string baseFolder, IFileSystem fileSystem)
        {
            this.fs = fileSystem;
            this.baseFolder = baseFolder ?? throw new ArgumentException("Settings base folder cannot be null");
            fs.Directory.CreateDirectory(baseFolder);

            cache = new ConcurrentDictionary<string, Tuple<DateTime, object>>();

            // Nasty, but needed for testing
            // FileWatcher is is not covered by IO.Abstractions
            if (fs is FileSystem)
                fwe = new FileWatcherExtended(baseFolder, "*.json");

            // Settings for specific provider, we should not share global settings
            jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };
        }

        #region ISettingsProvider implementation
        public object Load(Type settingsType, string key)
        {
            var returnValue = Activator.CreateInstance(settingsType);
            var configFile = fs.Path.Combine(baseFolder, CreateFileName(settingsType, key));
            if (!fs.File.Exists(configFile))
                Save(settingsType, returnValue, key);
            returnValue = Convert.ChangeType(GetValue(settingsType, configFile), settingsType);
            return returnValue;
        }
        public TSettings Load<TSettings>(string key) where TSettings : class, new()
            => Load(typeof(TSettings), key) as TSettings ;

        public void Save(Type settingsType, object settingsObj, string key)
        {
            var configFile = fs.Path.Combine(baseFolder, CreateFileName(settingsType, key));
            fs.FileInfo.FromFileName(configFile).Directory.Create();
            fs.File.WriteAllText(configFile, JsonConvert.SerializeObject(settingsObj, Formatting.Indented, jsonSettings));
            cache.TryRemove(configFile, out var tmp);
        }
        public void Save<TSettings>(TSettings settingsObj, string key) where TSettings : class, new()
            => Save(typeof(TSettings), settingsObj, key);
        #endregion

        #region Helpers
        private string CreateFileName(Type settingsType, string key)
        {
            // Get name parts
            var fileName = settingsType.Name;

            if (settingsType.IsGenericType && settingsType.GetInterfaces().Contains(typeof(IEnumerable)))
                fileName = $"{settingsType.GenericTypeArguments.First().Name}List";

            if (!String.IsNullOrEmpty(key))
                fileName += "." + key;

            fileName += ".json";

            // Check for settings path
            var prefix = settingsType.IsGenericType && settingsType.GetInterfaces().Contains(typeof(IEnumerable)) ?
                 settingsType.GenericTypeArguments.First().GetAttribute<SettingsPrefixAttribute>() :
                 settingsType.GetAttribute<SettingsPrefixAttribute>();
            if (prefix != null)
                fileName = $"{prefix.Prefix}.{fileName}";

            return fileName;
        }

        private object GetValue(Type settingsType, string fileName)
        {
            // Check in cache
            if (fwe !=null && cache.TryGetValue(fileName, out Tuple<DateTime, object> tmp))
            {
                // Get file's last modification
                var lastModified = fwe.LastModified(fileName);

                // If cache is older, keep it
                if (tmp.Item1 >= lastModified && tmp.Item2.GetType().Equals(settingsType))
                {
                    // For debugging reasons only
                    // Console.WriteLine("Cache hit!");
                    return tmp.Item2;
                }
            }

            var settings = JsonConvert.DeserializeObject(fs.File.ReadAllText(fileName), settingsType, jsonSettings);
            var cacheEntry = new Tuple<DateTime, object>(DateTime.Now, settings);
            cache.AddOrUpdate(fileName, cacheEntry, (k, v) => cacheEntry);
            return settings;
        }
        #endregion
    }
}