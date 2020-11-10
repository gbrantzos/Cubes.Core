using Cubes.Core.Base;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cubes.Core.Scheduling.ExecutionHistory
{
    public class LiteDbExecutionHistoryManager : IExecutionHistoryManager
    {
        private readonly string _liteDbPath;

        public LiteDbExecutionHistoryManager(ICubesEnvironment cubesEnvironment)
            => _liteDbPath = Path.Combine(cubesEnvironment.GetStorageFolder(), CubesConstants.ExecutionHistory_File);

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

        public IEnumerable<ExecutionHistoryDetails> GetLastExecutions(string[] jobNames)
        {
            throw new NotImplementedException();
        }

        public void Save(ExecutionHistoryDetails historyDetails)
        {
            using var db = new LiteDatabase(_liteDbPath);
            var collection = db.GetCollection<ExecutionHistoryDetails>();
            var existing = collection
                .Find(i => i.ID == historyDetails.ID)
                .FirstOrDefault();
            if (existing != null)
                collection.Delete(existing.ID);

            collection.Insert(historyDetails);
            collection.EnsureIndex(det => det.JobName, false);
        }
    }
}
