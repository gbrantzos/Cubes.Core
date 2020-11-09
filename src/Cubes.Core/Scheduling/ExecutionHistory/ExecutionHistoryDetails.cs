using System;
using System.Collections.Generic;

namespace Cubes.Core.Scheduling.ExecutionHistory
{
    public class ExecutionHistoryDetails
    {
        public Guid ID { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime ExecutedAt { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool ExecutionOnDemand { get; set; }
        public string JobName { get; set; }
        public string JobType { get; set; }
        public Dictionary<string, object> JobParameters { get; set; }
        public string JobInstance { get; set; }
        public bool ExecutionFailed { get; set; }
        public string ExecutionMessage { get; set; }
        public Exception ExceptionThrown { get; set; }
    }
}
