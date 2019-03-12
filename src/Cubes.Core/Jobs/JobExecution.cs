using System;

namespace Cubes.Core.Jobs
{
    public class JobExecution
    {
        public DateTime? ExecutedAt { get; set; }
        public DateTime? NextExecution { get; set; }
        public JobExecutionResult ExecutionResult { get; set; }
    }

    public class JobExecutionResult
    {
        public bool Failed { get; set; }
        public string Message { get; set; }
        public Exception ExceptionThrown { get; set; }
    }
}