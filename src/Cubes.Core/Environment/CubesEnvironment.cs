using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Autofac;
using Figgle;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace Cubes.Core.Environment
{
    public class CubesEnvironment : ICubesEnvironment
    {
        private static readonly HashSet<string> configurationFiles = new HashSet<string>
        {
            CubesConstants.Files_DataAccess,
            CubesConstants.Files_Scheduling,
            CubesConstants.Files_StaticContent
        };

        private readonly string rootFolder;
        private readonly ILogger logger;
        private readonly CubesEnvironmentInfo environmentInformation;
        private readonly IFileSystem fileSystem;
        private readonly List<LoadedAssembly> loadedAssemblies;
        private readonly List<ApplicationInfo> definedApplications;
        private readonly List<IApplication> activatedApplications;
        private readonly List<string> swaggerFiles;

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
            this.environmentInformation = new CubesEnvironmentInfo(rootFolder);
            this.loadedAssemblies       = new List<LoadedAssembly>();
            this.definedApplications    = new List<ApplicationInfo>();
            this.activatedApplications  = new List<IApplication>();

            var assemblyPath            = fileSystem.Path.GetDirectoryName(this.GetType().Assembly.Location);
            this.swaggerFiles           = new List<string>
            {
                fileSystem.Path.Combine(assemblyPath, "Cubes.Core.xml"),
                fileSystem.Path.Combine(assemblyPath, "Cubes.Web.xml")
            };

            var figgle  = FiggleFonts.Slant.Render("Cubes  v5");
            var version = $"{environmentInformation.Version}, {environmentInformation.Mode}";
            var message = $"Starting Cubes, version {version} build...{System.Environment.NewLine}{figgle}";
            logger.LogInformation(message);
            logger.LogInformation($"Cubes root folder: {rootFolder}");
        }

        public CubesEnvironment(string rootFolder, ILogger logger) : this(rootFolder, logger, new FileSystem()) { }

        #region ICubesEnvironment implementation
        public string GetFolder(CubesFolderKind folderKind)
            => folderKind == CubesFolderKind.Root ? rootFolder : fileSystem.Path.Combine(rootFolder, folderKind.ToString());

        public CubesEnvironmentInfo GetEnvironmentInformation() => this.environmentInformation;

        public IEnumerable<LoadedAssembly> GetLoadedAssemblies() => this.loadedAssemblies;

        public IEnumerable<ApplicationInfo> GetApplications() => this.definedApplications;

        public IEnumerable<IApplication> GetActivatedApplications() => this.activatedApplications;

        public void RegisterSwaggerXmlFile(string xmlFile)
        {
            if (!String.IsNullOrEmpty(xmlFile))
                this.swaggerFiles.Add(xmlFile);
        }

        public IEnumerable<string> GetSwaggerFiles() => this.swaggerFiles;
        #endregion

        // The following method is used for environment - server setup only.
        // There is no need to be included on ICubesEnvironment interface.
        public void PrepareHost()
        {
            // Ensure needed folders exist
            CreateFolders();

            // Load assemblies and applications
            LoadApplications();

            // Create settings files
            CreateSettingsFile();
        }

        private void CreateSettingsFile()
        {
            foreach (var file in configurationFiles)
            {
                var filePath = this.GetFileOnPath(CubesFolderKind.Settings, file);
                if (!File.Exists(filePath))
                    File.Create(filePath);
            }
        }

        private void CreateFolders()
        {
            var enumValues = Enum
                            .GetValues(typeof(CubesFolderKind))
                            .Cast<CubesFolderKind>()
                            .Except(new CubesFolderKind[] { CubesFolderKind.Root });
            foreach (var value in enumValues)
                fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(rootFolder, value.ToString()));
        }

        private void LoadApplications()
        {
            var applications = new List<ApplicationInfo>();

            // Check if applications file exists
            var applicationsFile = this.GetFileOnPath(CubesFolderKind.Root, CubesConstants.Files_Applications);
            if (fileSystem.File.Exists(applicationsFile))
            {
                var fileContents = File.ReadAllText(applicationsFile);
                var deserializer = new Deserializer();
                applications = deserializer.Deserialize<List<ApplicationInfo>>(fileContents);
            }

            // Load common assemblies
            var commonPath = fileSystem.Path.Combine(this.GetAppsFolder(), CubesConstants.Folders_Common);
            if (fileSystem.Directory.Exists(commonPath))
                LoadAssemblies(fileSystem.Directory.GetFiles(commonPath, "*.dll"));

            // Get applications from Applications Folder
            var appsFolders = fileSystem.Directory
                .GetDirectories(this.GetAppsFolder())
                .Where(directory => !directory.Equals(commonPath))
                .ToList();
            foreach (var folder in appsFolders)
            {
                var appInfo = GetApplicationInfo(folder);
                if (appInfo != null)
                {
                    var existing = this
                        .definedApplications
                        .FirstOrDefault(a => a.Name.Equals(appInfo.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (existing == null)
                        this.definedApplications.Add(appInfo);
                    else
                        logger.LogError("Application already defined: {application}", appInfo.Name);
                }
            }
            this.definedApplications.AddRange(applications);

            // Load all applications assemblies
            var allAssemblies = this.definedApplications
                .Where(app => app.Active)
                .SelectMany(app => app.Assemblies.Select(a => fileSystem.Path.Combine(app.Path, a)))
                .ToArray();
            LoadAssemblies(allAssemblies);

            // Create an instance of each application
            foreach (var app in this.definedApplications.Where(a => a.Active).ToList())
            {
                foreach (var asm in app.Assemblies)
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(Path.Combine(app.Path, asm)).Name;
                        var loadedAsm = AppDomain
                            .CurrentDomain
                            .GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name == assemblyName);
                        if (loadedAsm != null)
                        {
                            var applicationType = loadedAsm
                                .GetTypes()
                                .FirstOrDefault(t => typeof(IApplication).IsAssignableFrom(t));
                            if (applicationType != null)
                            {
                                var existing = this
                                    .activatedApplications
                                    .FirstOrDefault(app => app.GetType().Name == applicationType.Name);
                                if (existing != null)
                                {
                                    logger.LogError("Already instantiated application of type: {applicationType}", applicationType.Name);
                                    logger.LogError("Please check application setup!");
                                    continue;
                                }
                                var instance = Activator.CreateInstance(applicationType) as IApplication;
                                this.activatedApplications.Add(instance);
                                logger.LogInformation("Application instantiated: {application}", app.Name);

                                // Add Swagger file
                                this.RegisterSwaggerXmlFile(instance.SwaggerXmlFile);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to instantiate application {application}", app.Name);
                    }
                }
            }
        }

        private ApplicationInfo GetApplicationInfo(string folder)
        {
            var application = new ApplicationInfo();
            var infoFile = fileSystem.Path.Combine(folder, CubesConstants.Files_Application);
            if (fileSystem.File.Exists(infoFile))
            {
                var fileContents   = fileSystem.File.ReadAllText(infoFile);
                var deserializer   = new Deserializer();
                application        = deserializer.Deserialize<ApplicationInfo>(fileContents);
            }
            else
            {
                application.Active = true;
                application.Name   = new DirectoryInfo(folder).Name;
            }
            application.Path = folder;

            if (application.Active && (application.Assemblies == null || application.Assemblies.Count == 0))
                application.Assemblies = fileSystem.Directory.GetFiles(folder, "*.dll");

            return application;
        }

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

                    loadedAssemblies.Add(new LoadedAssembly
                    {
                        Filename            = fileSystem.Path.GetFileName(file),
                        Path        = file,
                        AssemblyName    = asm.GetName().Name,
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