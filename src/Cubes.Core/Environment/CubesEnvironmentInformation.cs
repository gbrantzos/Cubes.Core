using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace Cubes.Core.Environment
{
    public class CubesEnvironmentInformation
    {
        // Properties
        public DateTime LiveSince { get; }
        public string Version { get; }
        public bool IsDebug { get; } = true;
        public string Mode { get { return IsDebug ? "DEBUG" : "RELEASE"; } }
        public string Hostname { get; }
        public string RootFolder { get; }

        // Constructor
        public CubesEnvironmentInformation(string rootFolder)
        {
            RootFolder  = rootFolder;
            Hostname    = Dns.GetHostName();
            LiveSince   = DateTime.Now;
            Version     = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
            #if DEBUG
            IsDebug = true;
            #else
            IsDebug = false;
            #endif
        }
    }
}