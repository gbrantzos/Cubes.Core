using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace Cubes.Core.Environment
{
    public class CubesEnvironmentInfo
    {
        // Properties
        public DateTime LiveSince     { get; }
        public string   Version       { get; }
        public string   FileVersion   { get; }
        public bool     IsDebug       { get; } = true;
        public string   Hostname      { get; }
        public string   RootFolder    { get; }
        public string   Mode => IsDebug ? "DEBUG" : "RELEASE";

        // Constructor
        public CubesEnvironmentInfo(string rootFolder)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi      = FileVersionInfo.GetVersionInfo(assembly.Location);

            RootFolder  = rootFolder;
            Hostname    = Dns.GetHostName();
            LiveSince   = DateTime.Now;
            Version     = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .ToString();
            FileVersion = fvi.FileVersion;
            #if DEBUG
            IsDebug = true;
            #else
            IsDebug = false;
            #endif
        }
    }
}
