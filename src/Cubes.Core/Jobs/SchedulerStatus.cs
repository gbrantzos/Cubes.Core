using System;
using System.Collections.Generic;

namespace Cubes.Core.Jobs
{
    public enum SchedulerState
    {
        Started,
        Stopped
    }

    public class SchedulerStatus
    {
        public SchedulerState State { get; set; }
        public DateTime ServerTime { get; set; }
        public IEnumerable<JobStatus> Jobs { get; set; }
    }

    public class JobStatus
    {
        public JobDefinition Definition { get; set; }
        public JobExecution LastExecution { get; set; }
        public DateTime? NextExecutionAt { get; set; }
    }
}