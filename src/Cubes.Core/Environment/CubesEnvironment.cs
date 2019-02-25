using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Cubes.Core.Environment
{
    public class CubesEnvironment : ICubesEnvironment
    {
        private readonly string rootFolder;
        private readonly CubesEnvironmentInformation environmentInformation;

        public CubesEnvironment(string rootFolder)
        {
            this.rootFolder = rootFolder;
            this.environmentInformation = new CubesEnvironmentInformation(rootFolder);
        }
        public CubesEnvironment() : this(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) { }

        #region ICubesEnvironment implementation
        public string GetFolder(FolderKind folderKind)
            => folderKind == FolderKind.Root ? rootFolder : Path.Combine(rootFolder, folderKind.ToString());

        public CubesEnvironmentInformation GetEnvironmentInformation() => this.environmentInformation;
        #endregion

        public void PrepareEnvironmentFolders()
        {
            var enumValues = Enum
                .GetValues(typeof(FolderKind))
                .Cast<FolderKind>()
                .Except(new FolderKind[] { FolderKind.Root });
            foreach (var value in enumValues)
                Directory.CreateDirectory(Path.Combine(rootFolder, value.ToString()));
        }

        public void LoadAppsAssemblies()
        {
            var files = Directory.GetFiles(this.GetAppsFolder(), "*.dll");
            foreach (var file in files)
            {
                var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                Console.Write(asm.GetExportedTypes().Count());
            }
        }

        public void EnsureDefaultLoggersForNLog()
        {

        }
    }
}