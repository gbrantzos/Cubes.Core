using System;
using System.Linq;
using System.IO;
using System.Reflection;
using Cubes.Core.Utilities;

namespace Cubes.Core.Base
{
    public static class EmbeddedResourceManager
    {
        public static string GetText(Assembly assembly, string nameSpace, string name)
        {
            var resourceName = $"{nameSpace}.{name}";
            if (!assembly.GetManifestResourceNames().Contains(resourceName))
                throw new ArgumentException($"Resource '{resourceName}' does not exist!");

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd().Trim();
        }
    }
}
