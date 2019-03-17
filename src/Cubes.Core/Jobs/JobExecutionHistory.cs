using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cubes.Core.Environment;
using Cubes.Core.Settings;
using LiteDB;

namespace Cubes.Core.Jobs
{
    public class JobExecutionHistory : IJobExecutionHistory, IDisposable
    {
        private readonly LiteDatabase liteDb;
        public JobExecutionHistory(ICubesEnvironment environment, ISettingsProvider settingsProvider)
        {
            this.environment = environment;
            this.settingsProvider = settingsProvider;
            var path = Path.Combine(environment.GetStorageFolder(), "Core.JobExecutionHistory.db");
            var connectionString = $"Filename={path};Mode=Exclusive";
            this.liteDb = new LiteDatabase(connectionString);
        }

        private ConcurrentDictionary<string, JobExecution> lastExecution = new ConcurrentDictionary<string, JobExecution>();
        private readonly ICubesEnvironment environment;
        private readonly ISettingsProvider settingsProvider;

        public void Add(JobExecution jobExecution)
        {
            lastExecution.AddOrUpdate(jobExecution.JobID,
                jobExecution,
                (key, value) => value = jobExecution);

            var col = liteDb.GetCollection<JobExecution>();
            col.Insert(jobExecution);
            col.EnsureIndex(j => j.JobID);
        }

        public IEnumerable<JobExecution> GetAll(string jobID)
        {
            var col = liteDb.GetCollection<JobExecution>();
            return col.Find(j => j.JobID == jobID);
        }

        public JobExecution GetLast(string jobID)
        {
            if (lastExecution.TryGetValue(jobID, out JobExecution jobExecution))
                return jobExecution;
            else
                return null;
        }

        public int ClearHistory(string jobID, HistoryRetentionOptions options)
        {
            int entriesDeleted = 0;
            var col = liteDb.GetCollection<JobExecution>();
            if (options.KeepUntil.HasValue)
            {
                var untilDate = options.KeepUntil.Value;
                if (untilDate > DateTime.Now.Date) untilDate = DateTime.Now.Date;
                entriesDeleted = col.Delete(x => x.JobID == jobID && x.ExecutedAt < untilDate);
            }
            else
            {
                var toDelete = col
                    .Find(x => x.JobID == jobID)
                    .OrderByDescending(x => x.ExecutedAt)
                    .Skip(options.KeepLastTimes)
                    .Select(x => x.ID)
                    .ToList();
                foreach (var item in toDelete)
                    col.Delete(new BsonValue(item));
                entriesDeleted = toDelete.Count;
            }
            return entriesDeleted;
        }

        private void RemoveOrphans()
        {
            var col = liteDb.GetCollection<JobExecution>();
            var settings = settingsProvider.Load<JobSchedulerSettings>();
            var existing = settings
                .Jobs
                .Select(i => i.ID)
                .Distinct()
                .ToList();
            var orphans = col
                .FindAll()
                .Select(i => i.JobID)
                .Distinct()
                .Where(i => !existing.Contains(i))
                .ToList();

            foreach (var item in orphans)
                ClearHistory(item, new HistoryRetentionOptions { KeepLastTimes = 0 });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    RemoveOrphans();
                    liteDb?.Shrink(); 
                    liteDb?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}