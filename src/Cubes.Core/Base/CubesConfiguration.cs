using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Cubes.Core.Base
{
    /// <summary>
    /// Exposes Cubes environment and settings information through <see cref="IConfiguration"/>.
    /// </summary>
    public class CubesConfiguration
    {
        public string Version                      { get; set; }
        public string RootFolder                   { get; set; }
        public string ContentFolder                { get; set; }
        public string LibrariesFolder              { get; set; }
        public string LogsFolder                   { get; set; }
        public string SettingsFolder               { get; set; }
        public string StorageFolder                { get; set; }
        public string TempFolder                   { get; set; }
        public string WebRootFolder                { get; set; }
        public string BinariesFolder               { get; set; }
        public IEnumerable<string> SwaggerXmlFiles { get; set; }
    }

    public static class CubesConfigurationExtensions
    {
        public static IConfigurationBuilder AddCubesConfigurationProvider(this IConfigurationBuilder configuration,
            ICubesEnvironment cubes)
        {
            var section = CubesConstants.Configuration_Section;
            var cubesConfig = new Dictionary<string, string>
                {
                    { $"{section}:Version"        , cubes.GetEnvironmentInformation().FullVersion },
                    { $"{section}:RootFolder"     , cubes.GetRootFolder() },
                    { $"{section}:ContentFolder"  , cubes.GetFolder(CubesFolderKind.Content) },
                    { $"{section}:LibrariesFolder", cubes.GetFolder(CubesFolderKind.Libraries) },
                    { $"{section}:LogsFolder"     , cubes.GetFolder(CubesFolderKind.Logs) },
                    { $"{section}:SettingsFolder" , cubes.GetSettingsFolder() },
                    { $"{section}:StorageFolder"  , cubes.GetStorageFolder() },
                    { $"{section}:TempFolder"     , cubes.GetFolder(CubesFolderKind.Temp) },
                    { $"{section}:WebRootFolder"  , cubes.GetFolder(CubesFolderKind.WebRoot) },
                    { $"{section}:BinariesFolder" , cubes.GetBinariesFolder() },
                };
            var swaggerFiles = cubes
                .GetSwaggerXmlFiles()
                .Select((f, i) => new KeyValuePair<string, string>($"{CubesConstants.Configuration_Section}:SwaggerXMLFiles:{i}", f))
                .ToList();
            foreach (var item in swaggerFiles)
                cubesConfig.Add(item.Key, item.Value);

            configuration.AddInMemoryCollection(cubesConfig);
            return configuration;
        }

        public static CubesConfiguration GetCubesConfiguration(this IConfiguration configuration)
            => configuration.GetSection(CubesConstants.Configuration_Section).Get<CubesConfiguration>();
    }
}
