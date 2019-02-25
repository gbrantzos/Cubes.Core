namespace Cubes.Core.Environment
{
    public interface ICubesEnvironment
    {
        string GetFolder(FolderKind folderKind);
        CubesEnvironmentInformation GetEnvironmentInformation();
    }

    public enum FolderKind
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
            => cubesEnvironment.GetFolder(FolderKind.Root);
        public static string GetAppsFolder(this ICubesEnvironment cubesEnvironment)
            => cubesEnvironment.GetFolder(FolderKind.Apps);
        public static string GetSettingsFolder(this ICubesEnvironment cubesEnvironment)
            => cubesEnvironment.GetFolder(FolderKind.Settings);
        public static string GetStorageFolder(this ICubesEnvironment cubesEnvironment)
            => cubesEnvironment.GetFolder(FolderKind.Storage);
    }
}