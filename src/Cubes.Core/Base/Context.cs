using System;
using System.Collections.Generic;

namespace Cubes.Core.Base
{
    public class Context
    {
        public string ID                       { get; }
        public string SourceInfo               { get; }
        public DateTime StartedAt              { get; }

        public Dictionary<string, object> Data { get; }

        public Context(string id, string sourceInfo)
        {
            this.ID            = id;
            this.SourceInfo    = sourceInfo;
            this.StartedAt     = DateTime.UtcNow;
            this.Data          = new Dictionary<string, object>();
        }

        public Context(string sourceInfo) : this(Guid.NewGuid().ToString("N"), sourceInfo) { }
    }
}
