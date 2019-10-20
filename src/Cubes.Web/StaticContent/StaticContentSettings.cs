using System.Collections.Generic;

namespace Cubes.Web.StaticContent
{
    public class StaticContentSettings
    {
        public List<Content> Content { get; set; } = new List<Content>();

        public static StaticContentSettings Create()
        {
            var toReturn = new StaticContentSettings();
            toReturn.Content.Add(new StaticContent.Content
            {
                DefaultFile           = "index.html",
                RequestPath           = "/sample",
                FileSystemPath        = "sampleContent",
                PathIsAbsolute        = false,
                ServeUnknownFileTypes = true,
                CustomContentTypes    = new Dictionary<string, string> { { "json", "application/json" } }
            });
            return toReturn;
        }
    }

    public class Content
    {
        public string RequestPath { get; set; }
        public string FileSystemPath { get; set; }
        public bool PathIsAbsolute { get; set; }
        public string DefaultFile { get; set; } = "index.html";
        public bool ServeUnknownFileTypes { get; set; }
        public Dictionary<string, string> CustomContentTypes { get; set; }
    }
}
