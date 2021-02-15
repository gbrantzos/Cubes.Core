using System;
using Cubes.Core.Base;
using Cubes.Core.Utilities;

namespace Cubes.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ConfigurationStoreAttribute : Attribute
    {
        public CubesFolderKind CubesFolder { get; }
        public string FilePath { get; }

        public ConfigurationStoreAttribute(CubesFolderKind cubesFolder, string filePath)
        {
            CubesFolder = cubesFolder;
            FilePath    = filePath.ThrowIfEmpty(nameof(filePath));
        }

        public ConfigurationStoreAttribute(string filePath)
            : this(CubesFolderKind.Config, filePath) { }
    }
}
