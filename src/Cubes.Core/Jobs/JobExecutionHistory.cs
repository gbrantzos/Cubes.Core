using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Cubes.Core.Environment;
using Cubes.Core.Settings;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Jobs
{
    public class JobExecutionHistory : IJobExecutionHistory, IDisposable
    {
        private readonly LiteDatabase liteDb;
        private readonly ILogger<JobExecutionHistory> logger;
        private Timer timer;

        public JobExecutionHistory(ICubesEnvironment environment, ISettingsProvider settingsProvider, ILogger<JobExecutionHistory> logger)
        {
            var path = Path.Combine(environment.GetStorageFolder(), "Core.JobExecutionHistory.db");
            var connectionString = $"Filename={path};Mode=Exclusive";

            this.logger           = logger;
            this.environment      = environment;
            this.settingsProvider = settingsProvider;
            this.liteDb           = new LiteDatabase(connectionString);

            // Setup clean up timer
            var settings = settingsProvider.Load<JobSchedulerSettings>();
            var interval = 1000 * settings.MonitoringIntervalSecs;
            if (interval > 0)
                this.timer = new Timer((t) => ClearHistoryForAllJobs(), null, interval, interval);
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
            col.EnsureIndex(j => j.ExecutedAt);
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
            if (options.KeepLastDays.HasValue && options.KeepLastDays.Value >= 0)
            {
                var untilDate = DateTime.Now.Date.AddDays(-1 * options.KeepLastDays.Value);
                if (untilDate > DateTime.Now.Date) untilDate = DateTime.Now.Date;
                entriesDeleted = col.Delete(x => x.JobID == jobID && x.ExecutedAt < untilDate);

                return entriesDeleted;
            }

            if (options.KeepLastTimes.HasValue && options.KeepLastTimes.Value >= 0)
            {
                var toDelete = col
                    .Find(x => x.JobID == jobID)
                    .OrderByDescending(x => x.ExecutedAt)
                    .Skip(options.KeepLastTimes.Value)
                    .Select(x => x.ID)
                    .ToList();
                foreach (var item in toDelete)
                    col.Delete(new BsonValue(item));
                entriesDeleted = toDelete.Count;
            }
            return entriesDeleted;
        }

        private void ClearHistoryForAllJobs()
        {
            var settings = settingsProvider.Load<JobSchedulerSettings>();
            if (this.timer != null)
                timer.Change(Timeout.Infinite, Timeout.Infinite);

            logger.LogDebug("House keeping ...");
            ClearHistoryForAllJobsInternal(settings);

            var interval = 1000 * settings.MonitoringIntervalSecs;
            if (interval > 0)
                timer.Change(interval, interval);
        }

        private void ClearHistoryForAllJobsInternal(JobSchedulerSettings settings)
        {
            var options = settings.RetentionOptions;
            if (options == null)
                return;
            var col = liteDb.GetCollection<JobExecution>();
            if (options.KeepLastDays.HasValue && options.KeepLastDays.Value >= 0)
            {
                var untilDate = DateTime.Now.Date.AddDays(-1 * options.KeepLastDays.Value);
                if (untilDate > DateTime.Now.Date) untilDate = DateTime.Now.Date;
                col.Delete(x => x.ExecutedAt < untilDate);

                return;
            }

            if (options.KeepLastTimes.HasValue && options.KeepLastTimes.Value >= 0)
            {
                var allIDs = settings
                    .Jobs
                    .Select(i => i.ID)
                    .Distinct()
                    .ToList();
                foreach (var jobID in allIDs)
                {
                    var toDelete = col
                        .Find(x => x.JobID == jobID)
                        .OrderByDescending(x => x.ExecutedAt)
                        .Skip(options.KeepLastTimes.Value)
                        .Select(x => x.ID)
                        .ToList();
                    foreach (var item in toDelete)
                        col.Delete(new BsonValue(item));
                }
            }
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
                    ClearHistoryForAllJobs();

                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        liteDb?.Shrink();
                    liteDb?.Dispose();

                    timer = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}