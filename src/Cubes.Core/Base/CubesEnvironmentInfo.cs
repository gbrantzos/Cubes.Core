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
        // Properties
        public DateTime LiveSince     { get; }
        public string   Version       { get; }
        public string   BuildVersion  { get; }
        public DateTime BuildDateTime { get; set; }
        public string   GitHash       { get; } = "DEVELOPMENT";
        public bool     IsDebug       { get; } = true;
        public string   Hostname      { get; }
        public string   RootFolder    { get; }
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
            BuildVersion = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            if (BuildVersion.Contains("-"))
            {
                GitHash = BuildVersion.Substring(BuildVersion.IndexOf('-') + 1);
                BuildVersion = BuildVersion.Substring(0, BuildVersion.IndexOf('-'));
            }

            using var buildDate = assembly.GetManifestResourceStream("Cubes.Core.BuildDate.txt");
            using var reader    = new StreamReader(buildDate);
            string value        = reader.ReadToEnd().Trim();
            BuildDateTime       = DateTime.ParseExact(value, "yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
