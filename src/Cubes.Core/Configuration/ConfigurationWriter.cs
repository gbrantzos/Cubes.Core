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
        private readonly ITypeResolver typeResolver;
        private readonly ICubesEnvironment cubesEnvironment;
        private readonly List<ISerializer> serializers;

        public ConfigurationWriter(ITypeResolver typeResolver,
            ICubesEnvironment cubesEnvironment,
            IEnumerable<ISerializer> serializers)
        {
            this.typeResolver     = typeResolver;
            this.cubesEnvironment = cubesEnvironment;
            this.serializers      = serializers.ToList();
        }

        public void Save(Type configType, object configInstance)
        {
            var storeAttribute = configType.GetAttribute<ConfigurationStoreAttribute>();
            if (storeAttribute == null)
                throw new ArgumentException($"No configuration storage attribute for type '{configType.Name}'.");

            var path = Path.Combine(cubesEnvironment.GetFolder(storeAttribute.CubesFolder), storeAttribute.FilePath);
            var format = Path.GetExtension(path);
            if (format.StartsWith("."))
                format = format.Substring(1);

            var serializer = serializers.Find(s => s.Format.Equals(format, StringComparison.CurrentCultureIgnoreCase));
            if (serializer == null)
                throw new ArgumentException($"No serializer defined for format '{format}'.");

            var toWrite = new ExpandoObject();
            toWrite.TryAdd(configType.Name, configInstance);

            var settingsRaw = serializer.Serialize(toWrite);
            File.WriteAllText(path, settingsRaw);
        }

        public void Save(string configTypeName, object configInstance)
        {
            var configType = typeResolver.GetByName(configTypeName);
            if (configTypeName == null)
                throw new ArgumentException($"Could not resolve type '{configTypeName}'.");

            Save(configType, configInstance);
        }
    }
}
