using System;
using System.Net;
using System.Reflection;

namespace Cubes.Core.Environment
{
    public class CubesEnvironmentInfo
    {
        // Properties
        public DateTime LiveSince     { get; }
        public string   Version       { get; }
        public bool     IsDebug       { get; } = true;
        public string   Hostname      { get; }
        public string   RootFolder    { get; }
        public string   Mode => IsDebug ? "DEBUG" : "RELEASE";

        // Constructor
        public CubesEnvironmentInfo(string rootFolder)
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