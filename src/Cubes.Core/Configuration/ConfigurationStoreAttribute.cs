using System;
using Cubes.Core.Base;
using Cubes.Core.Utilities;

namespace Cubes.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ConfigurationStoreAttribute : Attribute
    {
        public CubesFolderKind CubesFolder { get; set; }
        public string FilePath { get; set; }
        public string ElementPath { get; set; } = String.Empty;

        public ConfigurationStoreAttribute(CubesFolderKind cubesFolder, string filePath, string elementPath)
        {
            CubesFolder = cubesFolder;
            FilePath    = filePath.ThrowIfEmpty(nameof(filePath));
            ElementPath = elementPath;
        }
        public ConfigurationStoreAttribute(CubesFolderKind cubesFolder, string filePath)
            : this(cubesFolder, filePath, String.Empty) { }
        public ConfigurationStoreAttribute(string filePath)
            : this(CubesFolderKind.Settings, filePath, String.Empty) { }

    }
}
