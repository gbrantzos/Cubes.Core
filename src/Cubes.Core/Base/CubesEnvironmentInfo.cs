using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;

namespace Cubes.Core.Base
{
    public class CubesEnvironmentInfo
    {
        public class BuildInfo
        {
            public DateTime BuildAt { get; set; }
            public string Branch { get; set; }
            public string Commit { get; set; }
        }

        // Properties
        public DateTime LiveSince            { get; }
        public BuildInfo BuildInformation    { get; }
        public string   Version              { get; }
        public string   BuildVersion         { get; }
        public string   FullVersion          { get; }
        public string   Hostname             { get; }
        public string   RootFolder           { get; }
        public bool     IsDebug              { get; } = true;
        public string   Mode => IsDebug ? "DEBUG" : "RELEASE";

        // Constructor
        public CubesEnvironmentInfo(string rootFolder)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi      = FileVersionInfo.GetVersionInfo(assembly.Location);

            RootFolder   = rootFolder;
            Hostname     = Dns.GetHostName();
            LiveSince    = DateTime.Now;
            Version      = fvi.FileVersion;
#if DEBUG
            IsDebug      = true;
#else
            IsDebug      = false;
#endif
            FullVersion = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
            BuildVersion = FullVersion;
            if (BuildVersion.Contains("-"))
                BuildVersion = BuildVersion.Substring(0, BuildVersion.IndexOf('-'));

            using var buildDate = assembly.GetManifestResourceStream("Cubes.Core.BuildInfo.txt");
            using var reader    = new StreamReader(buildDate);
            string value        = reader.ReadToEnd().Trim();
            BuildInformation    = GetBuildInfo(value);
        }

        public static BuildInfo GetBuildInfo(string text)
        {
            var infoParts = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return new BuildInfo
            {
                BuildAt = DateTime.ParseExact(infoParts[0], "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                Branch = infoParts[1].Substring(infoParts[1].IndexOf(":") + 1).Trim(),
                Commit = infoParts[2].Substring(infoParts[2].IndexOf(":") + 1).Trim()
            };
        }
    }
}
