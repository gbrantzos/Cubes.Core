using System;
using System.Collections.Generic;
using System.IO;

namespace Cubes.Core.Environment
{
    public interface ICubesEnvironment
    {
        string GetFolder(CubesFolderKind folderKind);
        CubesEnvironmentInformation GetEnvironmentInformation();
        IEnumerable<CubesLoadedApp> GetLoadedApps();

        void Start(IServiceProvider serviceProvider);
    }

    public class CubesLoadedApp
    {
        public string File { get; set; }
        public string AssemblyName { get; set; }
        public string AssemblyVersion { get; set; }

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