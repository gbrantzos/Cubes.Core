using System;
using System.IO.Abstractions;
using YamlDotNet.Serialization;

namespace Cubes.Core.Settings
{
    public class YamlFilesSettingsProvider : BaseFilesSettingsProvider
    {
        private static IDeserializer Deserializer { get; } = new DeserializerBuilder().Build();
        private static ISerializer Serializer { get; } = new SerializerBuilder().Build();

        public YamlFilesSettingsProvider(string baseFolder) : this(baseFolder, new FileSystem()) { }
        public YamlFilesSettingsProvider(string baseFolder, IFileSystem fileSystem) : base(baseFolder, "yaml", fileSystem) { }

        protected override object ProviderSpecificDeserialize(Type settingsType, string fileName)
        {
            var deserializer = YamlFilesSettingsProvider.Deserializer;
            return deserializer.Deserialize(fs.File.ReadAllText(fileName), settingsType);
        }

        protected override string ProviderSpecificSerialize(object settingsObj)
        {
            var serializer = YamlFilesSettingsProvider.Serializer;
            return serializer.Serialize(settingsObj);
        }
    }
}