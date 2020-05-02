using System.Collections.Generic;

namespace Cubes.Core.Base
{
    public class ApplicationManifest
    {
        /// <summary>
        /// Name of the application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if application should be loaded.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The manifest file path. Can be used to define relative paths inside manifest file.
        /// </summary>
        public string ManifestPath { get; set; }

        /// <summary>
        /// Assemblies list to load.
        /// </summary>
        public IEnumerable<string> Assemblies { get; set; } = new HashSet<string>();

        /// <summary>
        /// Configuration files (NOT USED).
        /// </summary>
        public IEnumerable<string> ConfigFiles { get; set; } = new HashSet<string>();

        /// <summary>
        /// Web apps to activate (NOT USED).
        /// </summary>
        public IEnumerable<string> WebApps { get; set; } = new HashSet<string>();
    }
}
