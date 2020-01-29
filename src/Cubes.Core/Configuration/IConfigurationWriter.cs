using System;

namespace Cubes.Core.Configuration
{
    public interface IConfigurationWriter
    {
        void Save(Type configType, object configInstance);
        void Save(string configTypeName, object configInstance);
    }

    public static class ConfigurationWriterExtensions
    {
        public static void Save<T>(this IConfigurationWriter writer, T configInstance)
            => writer.Save(typeof(T), configInstance);
    }
}
