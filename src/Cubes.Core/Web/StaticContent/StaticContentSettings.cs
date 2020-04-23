using System.Collections.Generic;
using Cubes.Core.Base;
using Cubes.Core.Configuration;

namespace Cubes.Core.Web.StaticContent
{
    [ConfigurationStore(CubesConstants.Files_StaticContent)]
    public class StaticContentSettings
    {
        public List<Content> Content { get; set; } = new List<Content>();

        public static StaticContentSettings Create()
        {
            var toReturn = new StaticContentSettings();
            toReturn.Content.Add(new Content
            {
                DefaultFile           = "index.html",
                RequestPath           = "sample",
                FileSystemPath        = "sampleContent",
                Active                = false,
                ServeUnknownFileTypes = true,
                CustomContentTypes    = new Dictionary<string, string> { { "json", "application/json" } }
            });
            return toReturn;
        }
    }

    public class Content
    {
        public string RequestPath                            { get; set; }
        public bool Active                                   { get; set; }
        public string FileSystemPath                         { get; set; }
        public string DefaultFile                            { get; set; } = "index.html";
        public bool ServeUnknownFileTypes                    { get; set; }
        public Dictionary<string, string> CustomContentTypes { get; set; }
    }
}
