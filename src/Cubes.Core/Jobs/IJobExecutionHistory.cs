using System;
using System.Collections.Generic;

namespace Cubes.Core.Jobs
{
    public interface IJobExecutionHistory
    {
         void Add(JobExecution jobExecution);
         JobExecution GetLast(string jobID);
         IEnumerable<JobExecution> GetAll(string jobID);
         int ClearHistory(string jobID, HistoryRetentionOptions options);
    }

    public class HistoryRetentionOptions
    {
        public int? KeepLastDays { get; set; } = 7;
        public int? KeepLastTimes { get; set; } = -1;
    }
}