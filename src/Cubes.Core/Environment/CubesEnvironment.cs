using Microsoft.Extensions.Logging;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.Loader;

namespace Cubes.Core.Environment
{
    public class CubesEnvironment : ICubesEnvironment
    {
        private readonly string rootFolder;
        private readonly ILogger logger;
        private readonly CubesEnvironmentInformation environmentInformation;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Constructor that accepts <see cref="IFileSystem"/> to support unit testing.
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="logger"></param>
        /// <param name="fileSystem"></param>
        public CubesEnvironment(string rootFolder, ILogger logger, IFileSystem fileSystem)
        {
            this.rootFolder             = rootFolder;
            this.logger                 = logger;
            this.fileSystem             = fileSystem;
            this.environmentInformation = new CubesEnvironmentInformation(rootFolder);

            logger.LogInformation($"Starting Cubes, version {environmentInformation.Version}, {environmentInformation.Mode} build...");
            logger.LogInformation($"Cubes root folder: {rootFolder}");
        }

        public CubesEnvironment(string rootFolder, ILogger logger) : this(rootFolder, logger, new FileSystem()) { }

        #region ICubesEnvironment implementation
        public string GetFolder(CubesFolderKind folderKind)
            => folderKind == CubesFolderKind.Root ? rootFolder : fileSystem.Path.Combine(rootFolder, folderKind.ToString());

        public CubesEnvironmentInformation GetEnvironmentInformation() => this.environmentInformation;
        #endregion

        // The following methods are use for environment - server setup
        // There is no need to be included on ICubesEnvironment interface
        public void PrepareEnvironmentFolders()
        {
            var enumValues = Enum
                .GetValues(typeof(CubesFolderKind))
                .Cast<CubesFolderKind>()
                .Except(new CubesFolderKind[] { CubesFolderKind.Root });
            foreach (var value in enumValues)
                fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(rootFolder, value.ToString()));
        }

        public void LoadAppsAssemblies()
        {
            var files = fileSystem.Directory.GetFiles(this.GetAppsFolder(), "*.dll");
            foreach (var file in files)
            {
                try
                {
                    var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                }
                catch (Exception x)
                {
                    logger.LogError(x, $"Could not load assembly {file}");
                }
            }
        }
    }
}