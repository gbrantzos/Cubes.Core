using Cubes.Core.Jobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly List<LoadedAssembly> loadedAssemblies;
        private readonly List<ApplicationInfo> applications;

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
            this.loadedAssemblies       = new List<LoadedAssembly>();
            this.applications           = new List<ApplicationInfo>();

            logger.LogInformation($"Starting Cubes, version {environmentInformation.Version}, {environmentInformation.Mode} build...");
            logger.LogInformation($"Cubes root folder: {rootFolder}");
        }

        public CubesEnvironment(string rootFolder, ILogger logger) : this(rootFolder, logger, new FileSystem()) { }

        #region ICubesEnvironment implementation
        public string GetFolder(CubesFolderKind folderKind)
            => folderKind == CubesFolderKind.Root ? rootFolder : fileSystem.Path.Combine(rootFolder, folderKind.ToString());

        public CubesEnvironmentInformation GetEnvironmentInformation() => this.environmentInformation;

        public IEnumerable<LoadedAssembly> GetLoadedAssemblies() => this.loadedAssemblies;

        public IEnumerable<ApplicationInfo> GetApplications() => this.applications;

        public void Start(IServiceProvider serviceProvider)
        {
            var jobScheduler = serviceProvider.GetService<IJobScheduler>();
            jobScheduler.StartScheduler();
        }
        #endregion

        // The following methods are use for environment - server setup
        // There is no need to be included on ICubesEnvironment interface
        public void PrepareEnvironment()
        {
            var enumValues = Enum
                .GetValues(typeof(CubesFolderKind))
                .Cast<CubesFolderKind>()
                .Except(new CubesFolderKind[] { CubesFolderKind.Root });
            foreach (var value in enumValues)
                fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(rootFolder, value.ToString()));
        }

        public void LoadApplications()
        {
            var appsFolders = Directory
                .GetDirectories(this.GetAppsFolder())
                .OrderBy(directory => directory.Equals(CubesConstants.Folders_Common, StringComparison.CurrentCultureIgnoreCase) ? 0 : 1)
                .ThenBy(directory => directory)
                .ToList();
            foreach (var folder in appsFolders)
            {
                var appInfo = GetApplicationInfo(folder);
                if (appInfo != null)
                    this.applications.Add(appInfo);

                // Load assemblies
                LoadAssemblies(folder);

            }



        }

        private ApplicationInfo GetApplicationInfo(string folder)
        {
            // TODO Check if a ApplicationInfo.yaml file exists
            return new ApplicationInfo();
        }

        private void LoadAssemblies(string folder)
        {
            var files = Directory.GetFiles(folder, "*.dll");
            foreach (var file in files)
            {
                try
                {
                    var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                    logger.LogInformation($"Loaded assembly: {asm.FullName}");

                    loadedAssemblies.Add(new LoadedAssembly
                    {
                        File = Path.GetFileName(file),
                        AssemblyName = asm.GetName().Name,
                        AssemblyVersion = asm.GetName().Version.ToString()
                    });
                }
                catch (Exception x)
                {
                    logger.LogError(x, $"Could not load assembly {file}");
                }
            }
        }
    }
}