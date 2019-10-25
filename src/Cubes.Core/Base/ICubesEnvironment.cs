using System;
using System.Collections.Generic;
using System.IO;
using Autofac;

namespace Cubes.Core.Base
{
    public interface ICubesEnvironment
    {
        /// <summary>
        /// Get Cubes specific folder.
        /// </summary>
        /// <param name="folderKind"></param>
        /// <returns></returns>
        string GetFolder(CubesFolderKind folderKind);

        /// <summary>
        /// Get Cubes binaries folder
        /// </summary>
        /// <returns></returns>
        string GetBinariesFolder();

        /// <summary>
        /// Cubes environment information.
        /// </summary>
        /// <returns><see cref="CubesEnvironmentInfo"/></returns>
        CubesEnvironmentInfo GetEnvironmentInformation();

        /// <summary>
        /// All assemblies loaded during host startup.
        /// </summary>
        /// <returns><see cref="IEnumerable{LoadedAssembly}"/></returns>
        IEnumerable<LoadedAssembly> GetLoadedAssemblies();

        /// <summary>
        /// All applications found during host startup.
        /// </summary>
        /// <returns><see cref="IEnumerable{ApplicationInfo}"/></returns>
        IEnumerable<ApplicationInfo> GetApplications();

        /// <summary>
        /// All applications that are activated.
        /// </summary>
        /// <returns><see cref="IEnumerable{IApplication}"/></returns>
        IEnumerable<IApplication> GetActivatedApplications();

        /// <summary>
        /// Add an XML file for Swagger documentation.
        /// </summary>
        /// <param name="xmlFile">XML file path</param>
        void RegisterSwaggerXmlFile(string xmlFile);

        /// <summary>
        /// Get all Swagger registered XML files.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSwaggerFiles();
    }

    public class LoadedAssembly
    {
        public string Filename        { get; set; }
        public string Path            { get; set; }
        public string AssemblyName    { get; set; }
        public string AssemblyVersion { get; set; }
    }

    public class ApplicationInfo
    {
        public string Name                    { get; set; } = String.Empty;
        public bool Active                    { get; set; } = true;
        public string Path                    { get; set; } = String.Empty;
        public string UIPath                  { get; set; } = String.Empty;
        public ICollection<string> Assemblies { get; set; }
    }

    public enum CubesFolderKind
    {
        Root,
        Apps,
        Logs,
        Settings,
        StaticContent,
        Storage,
        Temp
    }

    public static class CubesEnvironmentExtentions
    {
        public static string GetRootFolder(this ICubesEnvironment cubesEnvironment)
            => cubesEnvironment.GetFolder(CubesFolderKind.Root);
        public static string GetAppsFolder(this ICubesEnvironment cubesEnvironment)
            => cubesEnvironment.GetFolder(CubesFolderKind.Apps);
        public static string GetSettingsFolder(this ICubesEnvironment cubesEnvironment)
            => cubesEnvironment.GetFolder(CubesFolderKind.Settings);
        public static string GetStorageFolder(this ICubesEnvironment cubesEnvironment)
            => cubesEnvironment.GetFolder(CubesFolderKind.Storage);

        public static string GetFileOnPath(this ICubesEnvironment cubesEnvironment,
            CubesFolderKind folderKind,
            string fileName)
            => Path.Combine(cubesEnvironment.GetFolder(folderKind), fileName);
    }
}