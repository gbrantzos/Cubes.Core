using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace Cubes.Core.Base
{
    public static class CubesEnvironmentHelpers
    {
        private static readonly string BinariesPath = Path.GetDirectoryName(typeof(CubesEnvironment).Assembly.Location);
        private static readonly char[] Separators = new char[] { ',', ';' };

        /// <summary>
        /// Get Cubes root folder from command line or environment variable.
        /// </summary>
        /// <param name="args">Process command line arguments</param>
        /// <returns></returns>
        public static string GetRootFolder(string[] args)
        {
            var argsList = args == null ? Enumerable.Empty<string>().ToList() : args.ToList();
            var toReturn = String.Empty;

            var rootFromVariable = Environment.GetEnvironmentVariable("CUBES_ROOT");
            if (!String.IsNullOrEmpty(rootFromVariable))
                toReturn = rootFromVariable;

            if (args.Length > 0)
            {
                int argIndex = argsList.FindIndex(arg => arg.Equals("--root", StringComparison.OrdinalIgnoreCase));
                if (argIndex != -1 && (argIndex + 1) < args.Length)
                    toReturn = argsList[argIndex + 1];
            }
            toReturn = ReplaceVariables(toReturn);

            return String.IsNullOrEmpty(toReturn) ?
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) : toReturn;
        }

        /// <summary>
        /// Get Cubes application manifests from command line or environment variable.
        /// </summary>
        /// <param name="rootFolder">The Cubes root folder</param>
        /// <param name="args">Process command line arguments</param>
        /// <returns></returns>
        public static IEnumerable<ApplicationManifest> GetApplications(string rootFolder, string[] args)
        {
            var argsList = args == null ? Enumerable.Empty<string>().ToList() : args.ToList();
            var toProcess = new List<string>();
            var toReturn = new List<ApplicationManifest>();

            var appsFromVariable = Environment.GetEnvironmentVariable("CUBES_APPLICATION");
            if (!String.IsNullOrEmpty(appsFromVariable))
                toProcess.AddRange(appsFromVariable.Split(Separators, StringSplitOptions.RemoveEmptyEntries));

            if (args.Length > 0)
            {
                int argIndex = 0;
                do
                {
                    argIndex = argsList.FindIndex(argIndex, arg => arg.Equals("--application", StringComparison.OrdinalIgnoreCase));
                    if (argIndex != -1 && (argIndex + 1) < args.Length)
                    {
                        argIndex++;
                        toProcess.Add(argsList[argIndex]);
                    }
                }
                while (argIndex != -1);
            }

            // Make paths to process absolute
            var absolutePaths = new List<string>();
            foreach (var p in toProcess)
            {
                var path = ReplaceVariables(p);
                var actualPath = File.Exists(path) ?
                    path :
                    Path.Combine(rootFolder, path);
                absolutePaths.Add(Path.GetFullPath(actualPath));
            }

            // Process manifest files
            var deserializer = new DeserializerBuilder().Build();
            foreach (var path in absolutePaths)
            {
                if (!File.Exists(path))
                    throw new ArgumentException($"Manifest file not found: {path}");

                var content           = File.ReadAllText(path);
                var manifest          = deserializer.Deserialize<ApplicationManifest>(content);
                manifest.ManifestPath = Path.GetDirectoryName(path);

                toReturn.Add(manifest);
            }

            return toReturn;
        }

        /// <summary>
        /// Get the path where CubesManagement.zip is located, to server Cubes Management web application.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetAdminPath(string[] args)
        {
            var argsList = args == null ? Enumerable.Empty<string>().ToList() : args.ToList();
            var toReturn = String.Empty;

            var adminFromVariable = Environment.GetEnvironmentVariable("CUBES_ADMIN");
            if (!String.IsNullOrEmpty(adminFromVariable))
                toReturn = adminFromVariable;

            if (args.Length > 0)
            {
                int argIndex = argsList.FindIndex(arg => arg.Equals("--admin", StringComparison.OrdinalIgnoreCase));
                if (argIndex != -1 && (argIndex + 1) < args.Length)
                    toReturn = argsList[argIndex + 1];
            }
            toReturn = ReplaceVariables(toReturn);

            string fallBack = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "CubesManagement.zip");
            return Path.GetFullPath(String.IsNullOrEmpty(toReturn) ? fallBack : toReturn);
        }

        private static string ReplaceVariables(string toReturn)
        {
            var index = toReturn.IndexOf("${BINARIES}", StringComparison.OrdinalIgnoreCase);
            return index >= 0 ? toReturn.Replace("${BINARIES}", BinariesPath) : toReturn;
        }
    }
}
