using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Autofac;
using Cubes.Core.DataAccess;
using Cubes.Core.Email;
using Cubes.Core.Scheduling;
using Cubes.Core.Web.StaticContent;
using Figgle;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace Cubes.Core.Base
{
    public class CubesEnvironment : ICubesEnvironment
    {
        private static readonly List<(string Filename, Func<object> CreateDefaultObject)> configurationFiles
            = new List<(string, Func<object>)>
        {
            (CubesConstants.Files_DataAccess,    DataAccessSettings.Create),
            (CubesConstants.Files_Scheduling,    SchedulerSettings.Create),
            (CubesConstants.Files_SmtpSettings,  SmtpSettingsProfiles.Create),
            (CubesConstants.Files_StaticContent, StaticContentSettings.Create)
        };

        private readonly static string Platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? "win" : "unix";
        private readonly string rootFolder;
        private readonly IEnumerable<ApplicationManifest> applicationManifests;
        private readonly ILogger logger;
        private readonly IFileSystem fileSystem;

        private readonly CubesEnvironmentInfo environmentInformation;
        private readonly List<AssemblyDetails> loadedAssemblies;
        private readonly List<ApplicationManifest> loadedApplications;
        private readonly List<IApplication> applicationInstances;
        private readonly List<string> swaggerXmlFiles;

        /// <summary>
        /// Constructor that accepts <see cref="IFileSystem"/> to support unit testing.
        /// </summary>
        /// <param name="rootFolder">Cubes root folder.</param>
        /// <param name="applications">Application manifests defined using environment variables or command line parameters.</param>
        /// <param name="logger">The logger to use</param>
        /// <param name="fileSystem"></param>
        public CubesEnvironment(string rootFolder,
            IEnumerable<ApplicationManifest> applications,
            ILogger logger,
            IFileSystem fileSystem)
        {
            this.rootFolder             = rootFolder;
            this.logger                 = logger;
            this.fileSystem             = fileSystem;

            this.environmentInformation = new CubesEnvironmentInfo(rootFolder);
            this.applicationManifests   = applications;

            this.applicationInstances   = new List<IApplication>();
            this.loadedAssemblies       = new List<AssemblyDetails>();
            this.loadedApplications     = new List<ApplicationManifest>();
            this.swaggerXmlFiles        = new List<string>();
            CheckRootFolderExistance(rootFolder);

            // Cubes information
            var figgle = FiggleFonts.Slant.Render(" Cubes v5");
            var buildInfo = $"Git Commit #{environmentInformation.GitHash}, build time {environmentInformation.BuildDateTime}";
            var version = $"{environmentInformation.BuildVersion}, {environmentInformation.Mode}";
            var message = $"Starting Cubes, version {version} build [{buildInfo}]{Environment.NewLine}{figgle}";

            logger.LogInformation(new String('-', 100));
            logger.LogInformation(message);
            logger.LogInformation($"Cubes root folder: {rootFolder}");
        }

        /// <summary>
        /// Create a CubesEnvironment.
        /// </summary>
        /// <param name="rootFolder">Cubes root folder.</param>
        /// <param name="applications">Application manifests defined using environment variables or command line parameters.</param>
        /// <param name="logger">The logger to use</param>
        public CubesEnvironment(string rootFolder, IEnumerable<ApplicationManifest> applications, ILogger logger)
            : this(rootFolder, applications, logger, new FileSystem()) { }

        #region ICubesEnvironment implementation
        public string GetBinariesFolder()
            => fileSystem.Path.GetDirectoryName(typeof(CubesEnvironment).Assembly.Location);

        public string GetFolder(CubesFolderKind folderKind)
            => folderKind == CubesFolderKind.Root ? rootFolder : fileSystem.Path.Combine(rootFolder, folderKind.ToString());

        public CubesEnvironmentInfo GetEnvironmentInformation() => this.environmentInformation;

        public IEnumerable<AssemblyDetails> GetLoadedAssemblies() => this.loadedAssemblies.AsReadOnly();

        public IEnumerable<ApplicationManifest> GetLoadedeApplications() => this.loadedApplications.AsReadOnly();

        public IEnumerable<IApplication> GetApplicationInstances() => this.applicationInstances.AsReadOnly();

        public IEnumerable<string> GetSwaggerXmlFiles() => this.swaggerXmlFiles.AsReadOnly();
        #endregion

        // The following method is used for environment - server setup only.
        // There is no need to be included on ICubesEnvironment interface.
        public void PrepareHost()
        {
            // Ensure needed folders exist
            CreateFolders();

            // Load assemblies and applications
            LoadApplications();

            // Gather Swagger files
            CollectSwaggerXmlFiles();

            // Create settings files
            CreateSettingsFile();
        }

        // TODO We shall keep this until we can provide a centralized IConfiguration with write capabilities!
        private void CreateSettingsFile()
        {
            var serializer = new SerializerBuilder().Build();
            foreach (var config in configurationFiles)
            {
                var filePath = this.GetFileOnPath(CubesFolderKind.Settings, config.Filename);
                if (!fileSystem.File.Exists(filePath))
                {
                    var toWrite  = new ExpandoObject();
                    var instance = config.CreateDefaultObject();
                    toWrite.TryAdd(instance.GetType().Name, instance);
                    fileSystem.File.WriteAllText(filePath, serializer.Serialize(toWrite));
                }
            }
        }

        // Create all needed folders
        private void CreateFolders()
        {
            var enumValues = Enum
                            .GetValues(typeof(CubesFolderKind))
                            .Cast<CubesFolderKind>()
                            .Except(new CubesFolderKind[] { CubesFolderKind.Root })
                            .Select(e => e.ToString());
            foreach (var value in enumValues)
                fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(rootFolder, value));
        }

        // Gather application assemblies to be loaded, including libraries folder
        private IEnumerable<ApplicationManifest> DiscoverApplications()
        {
            var discoveredApplications = new List<ApplicationManifest>();

            // Add default application
            var defaultApplication = new ApplicationManifest
            {
                Name = "Default",
                Active = true,
                Assemblies = GetCommonAssemblies()
            };
            discoveredApplications.Add(defaultApplication);

            // Process applications to load
            foreach (var application in applicationManifests)
            {
                if (application.Active)
                {
                    var applicationAssemblies = new List<string>();
                    foreach (var assemblyPath in application.Assemblies)
                    {
                        var temp = assemblyPath;

                        // Check for platform filter
                        if (temp.StartsWith("{os:"))
                        {
                            if (!temp.StartsWith($"{{os:{CubesEnvironment.Platform}}}"))
                                continue;
                            temp = temp.Substring(assemblyPath.IndexOf('}') + 1);
                        }

                        // Make assembly paths absolute
                        var actualPath = fileSystem.File.Exists(temp) ?
                            temp :
                            fileSystem.Path.Combine(application.BasePath, temp);
                        applicationAssemblies.Add(actualPath);
                    }

                    var toLoad = new ApplicationManifest
                    {
                        Name = application.Name,
                        Active = true,
                        BasePath = application.BasePath,
                        Assemblies = applicationAssemblies
                    };
                    discoveredApplications.Add(toLoad);
                }
            }
            return discoveredApplications;
        }

        // Load applications, instantiate instances
        private void LoadApplications()
        {
            // Get applications to load details
            var toLoad = DiscoverApplications();

            // Load all assemblies defined
            var allAssemblies = toLoad
                .SelectMany(app => app.Assemblies)
                .ToArray();
            LoadAssemblies(allAssemblies);

            // Instantiate application instances
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var loadedAssembliesNames = this.loadedAssemblies.Select(i => i.AssemblyName).ToList();
            var applicationTypes = domainAssemblies
                .Where(l => loadedAssembliesNames.Contains(l.FullName))
                .SelectMany(asm => asm.GetTypes())
                .Where(t => typeof(IApplication).IsAssignableFrom(t))
                .ToList();
            foreach (var applicationType in applicationTypes)
            {
                var instance = Activator.CreateInstance(applicationType) as IApplication;
                this.applicationInstances.Add(instance);
                logger.LogInformation("Instantiated application \"{application}\", from {path}.",
                    applicationType.Name,
                    applicationType.Assembly.Location);
            }

            // Expose loaded applications details
            this.loadedApplications.AddRange(toLoad);
        }

        // Get common assemblies, respect platform sub-folders
        private IEnumerable<string> GetCommonAssemblies()
        {
            var toReturn = new List<string>();

            // Load common assemblies
            var libraries = fileSystem.Directory.GetFiles(this.GetFolder(CubesFolderKind.Libraries));
            toReturn.AddRange(libraries);

            // Load common assemblies for current platform
            var platfromPath = fileSystem.Path.Combine(this.GetFolder(CubesFolderKind.Libraries), CubesEnvironment.Platform);
            if (fileSystem.Directory.Exists(platfromPath))
            {
                var platformLibraries = fileSystem.Directory.GetFiles(platfromPath);
                toReturn.AddRange(platformLibraries);
            }
            return toReturn;
        }

        // Load assemblies
        private void LoadAssemblies(string[] assemblies)
        {
            foreach (var file in assemblies)
            {
                try
                {
                    // Check if we have already loaded such an assembly
                    var asmName = AssemblyName.GetAssemblyName(file).Name;
                    var existing = this.loadedAssemblies.FirstOrDefault(asm => asm.AssemblyName == asmName);
                    if (existing != null)
                    {
                        logger.LogError("Assembly with name {name} is already loaded: {path}", asmName, existing.Path);
                        continue;
                    }

                    // Load assembly
                    var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                    logger.LogInformation($"Loaded assembly: {asm.FullName}");

                    this.loadedAssemblies.Add(new AssemblyDetails
                    {
                        Filename        = fileSystem.Path.GetFileName(file),
                        Path            = file,
                        AssemblyName    = asm.FullName,
                        AssemblyVersion = asm.GetName().Version.ToString()
                    });
                }
                catch (Exception x)
                {
                    logger.LogError(x, $"Could not load assembly {file}");
                }
            }
        }

        // Make sure root folder exists
        private void CheckRootFolderExistance(string rootFolder)
        {
            if (!this.fileSystem.Directory.Exists(rootFolder))
            {
                try
                {
                    this.fileSystem.Directory.CreateDirectory(rootFolder);
                }
                catch (Exception x)
                {
                    throw new ArgumentException($"Root folder must be writable by cubes process: \"{rootFolder}\"!", x);
                }
            }
        }

        // Gather Swagger files
        private void CollectSwaggerXmlFiles()
        {
            var assemblyPath = fileSystem.Path.GetDirectoryName(this.GetType().Assembly.Location);
            this.swaggerXmlFiles.Add(fileSystem.Path.Combine(assemblyPath, "Cubes.Core.xml"));
            foreach (var application in this.GetApplicationInstances())
                this.swaggerXmlFiles.AddRange(application.GetSwaggerXmlFiles());
        }
    }
}