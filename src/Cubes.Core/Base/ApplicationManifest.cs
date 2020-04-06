using System.Collections.Generic;

namespace Cubes.Core.Base
{
    public class ApplicationManifest
    {
        public string Name { get; set; }
        public bool Active { get; set; }
        public string BasePath { get; set; }
        public IEnumerable<string> Assemblies { get; set; } = new HashSet<string>();
        public IEnumerable<string> ConfigFiles { get; set; } = new HashSet<string>();
        public IEnumerable<string> WebApps { get; set; } = new HashSet<string>();
    }
}
