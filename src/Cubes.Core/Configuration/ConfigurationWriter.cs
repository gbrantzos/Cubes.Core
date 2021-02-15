using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Cubes.Core.Base;
using Cubes.Core.Utilities;

namespace Cubes.Core.Configuration
{
    public class ConfigurationWriter : IConfigurationWriter
    {
        private readonly ITypeResolver _typeResolver;
        private readonly ICubesEnvironment _cubesEnvironment;
        private readonly List<ISerializer> _serializers;

        public ConfigurationWriter(ITypeResolver typeResolver,
            ICubesEnvironment cubesEnvironment,
            IEnumerable<ISerializer> serializers)
        {
            _typeResolver     = typeResolver;
            _cubesEnvironment = cubesEnvironment;
            _serializers      = serializers.ToList();
        }

        public void Save(Type configType, object configInstance)
        {
            var storeAttribute = configType.GetAttribute<ConfigurationStoreAttribute>();
            if (storeAttribute == null)
                throw new ArgumentException($"No configuration storage attribute for type '{configType.Name}'.");

            var path = Path.Combine(_cubesEnvironment.GetFolder(storeAttribute.CubesFolder), storeAttribute.FilePath);
            var format = Path.GetExtension(path);
            if (format.StartsWith("."))
                format = format.Substring(1);

            var serializer = _serializers.Find(s => s.Format.Equals(format, StringComparison.CurrentCultureIgnoreCase));
            if (serializer == null)
                throw new ArgumentException($"No serializer defined for format '{format}'.");

            var toWrite = new ExpandoObject();
            toWrite.TryAdd(configType.Name, configInstance);

            var configRaw = serializer.Serialize(toWrite);
            File.WriteAllText(path, configRaw);
        }

        public void Save(string configTypeName, object configInstance)
        {
            var configType = _typeResolver.GetByName(configTypeName);
            if (configTypeName == null)
                throw new ArgumentException($"Could not resolve type '{configTypeName}'.");

            Save(configType, configInstance);
        }
    }
}
