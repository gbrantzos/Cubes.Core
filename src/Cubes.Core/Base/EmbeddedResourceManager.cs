using System.IO;
using System.Reflection;

namespace Cubes.Core.Base
{
    public static class EmbeddedResourceManager
    {
        public static string GetText(Assembly assembly, string nameSpace, string name)
        {
            using var stream = assembly.GetManifestResourceStream($"{nameSpace}.{name}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd().Trim();
        }
    }
}
