using System;
using System.Collections.Generic;
using System.Text;

namespace Cubes.Core.Scheduling.ExecutionHistory
{
    public class LiteDbExecutionHistoryManager : IExecutionHistoryManager
    {
        public void Delete(string jobName, Retention retention)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExecutionHistoryDetails> Get(string jobName)
        {
            throw new NotImplementedException();
        }

        public ExecutionHistoryDetails GetLastExecution(string jobName)
        {
            throw new NotImplementedException();
        }

        public void Save(ExecutionHistoryDetails historyDetails)
        {
            throw new NotImplementedException();
        }
    }
}
