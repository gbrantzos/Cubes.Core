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
        public IEnumerable<(JobDefinition Definition, JobExecution Execution)> JobDetails { get; set; }
    }
}