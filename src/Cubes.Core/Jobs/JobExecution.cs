using System;

namespace Cubes.Core.Jobs
{
    public class JobExecution
    {
        public string JobID { get; set; }
        public DateTime? ExecutedAt { get; set; }
        public TimeSpan Duration { get; set; }
        public JobExecutionResult ExecutionResult { get; set; }
    }

    public class JobExecutionResult
    {
        public bool Failed { get => ExceptionThrown != null; }
        public string Message { get; set; }
        public Exception ExceptionThrown { get; set; }
    }
}