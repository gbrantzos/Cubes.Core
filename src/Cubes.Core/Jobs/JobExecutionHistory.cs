using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Cubes.Core.Jobs
{
    public class JobExecutionHistory : IJobExecutionHistory
    {
        private ConcurrentDictionary<string, JobExecution> lastExecution = new ConcurrentDictionary<string, JobExecution>();

        public void Add(JobExecution jobExecution)
        {
            lastExecution.AddOrUpdate(jobExecution.JobID,
                jobExecution,
                (key, value) => value = jobExecution);
            // TODO: Add to permenant storage
        }

        public IEnumerable<JobExecution> GetAll(string jobID)
        {
            // TODO: Get from permenant storage
            return new List<JobExecution> { GetLast(jobID) };
        }

        public JobExecution GetLast(string jobID)
        {
            if (lastExecution.TryGetValue(jobID, out JobExecution jobExecution))
                return jobExecution;
            else
                return null;
        }
    }
}