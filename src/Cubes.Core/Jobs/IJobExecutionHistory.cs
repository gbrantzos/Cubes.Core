using System.Collections.Generic;

namespace Cubes.Core.Jobs
{
    public interface IJobExecutionHistory
    {
         void Add(JobExecution jobExecution);
         JobExecution GetLast(string jobID);
         IEnumerable<JobExecution> GetAll(string jobID);
    }
}