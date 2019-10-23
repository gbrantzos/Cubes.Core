using System;
using System.Collections.Generic;

namespace Cubes.Core.Base
{
    public enum ContextSourceEnum
    {
        None,
        HttpRequest,
        Scheduler
    }

    public class Context
    {
        public string ID                       { get; }
        public ContextSourceEnum ContextSource { get; }
        public string SourceInfo               { get; }
        public DateTime StartedAt              { get; }

        public Dictionary<string, object> Data { get; }

        public Context(string id, ContextSourceEnum contextSource, string sourceInfo)
        {
            this.ID            = id;
            this.ContextSource = contextSource;
            this.SourceInfo    = sourceInfo;

            this.StartedAt     = DateTime.UtcNow;
            this.Data          = new Dictionary<string, object>();
        }
    }
}
