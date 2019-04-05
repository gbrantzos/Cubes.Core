using System.Collections.Generic;
using Cubes.Core.Settings;

namespace Cubes.Web.StaticContent
{
    [SettingsPrefix("Core")]
    public class StaticContentSettings
    {
        public List<Content> Content { get; set; } = new List<Content>();
    }

    public class Content
    {
        public string RequestPath { get; set; }
        public string FileSystemPath { get; set; }
        public bool PathIsAbsolute { get; set; }
        public string DefaultFile { get; set; } = "index.html";
        public bool ServeUnknownFileTypes { get; set; }
    }
}
