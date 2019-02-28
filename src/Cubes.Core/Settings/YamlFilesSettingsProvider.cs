using System;
using System.IO.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cubes.Core.Settings
{
    public class YamlFilesSettingsProvider : BaseFilesSettingsProvider
    {
        public YamlFilesSettingsProvider(string baseFolder) : this(baseFolder, new FileSystem()) { }
        public YamlFilesSettingsProvider(string baseFolder, IFileSystem fileSystem) : base(baseFolder, "yaml", fileSystem) { }

        protected override object ProviderSpecificDeserialize(Type settingsType, string fileName)
        {
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize(fs.File.ReadAllText(fileName), settingsType);
        }

        protected override string ProviderSpecificSerialize(object settingsObj)
        {
            var serializer = new SerializerBuilder().Build();
            return serializer.Serialize(settingsObj);
        }
    }
}