using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cubes.Core.Base;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Scheduling.ExecutionHistory
{
    public class LiteDbExecutionHistoryManager : IExecutionHistoryManager, IDisposable
    {
        private readonly int _cleanupIntervalSeconds = 60 * 5;
        private readonly Timer _cleanup;
        private readonly Retention _defaultRetention;
        private readonly LiteDatabase _liteDb;
        private readonly ILogger<LiteDbExecutionHistoryManager> _logger;
        private bool _disposedValue;

        public LiteDbExecutionHistoryManager(ICubesEnvironment cubesEnvironment,
            IConfiguration configuration,
            ILogger<LiteDbExecutionHistoryManager> logger)
        {
            var retentionString = configuration.GetValue(CubesConstants.Config_JobsHistoryRetention, "LastWeek");
            _defaultRetention = Retention.FromString(retentionString);

            var path = Path.Combine(cubesEnvironment.GetStorageFolder(), CubesConstants.ExecutionHistory_File);
            _liteDb = new LiteDatabase(path);
            _cleanup = new Timer(CleanupCallback, null, 3 * 1000, _cleanupIntervalSeconds * 1000);

            _logger = logger;
            _logger.LogInformation($"Execution history manager with retention {_defaultRetention}");
        }

        private void CleanupCallback(object state)
        {
            _logger.LogDebug("Cleanup jobs execution history...");
            var collection = _liteDb.GetCollection<ExecutionHistoryDetails>();

            if (_defaultRetention.Policy == Retention.RetentionPolicy.Days)
            {
                var lastDay = DateTime.Now.AddDays(_defaultRetention.Value * -1);
                collection.DeleteMany(d => d.ExecutedAt.Date <= lastDay);
            }

            if (_defaultRetention.Policy == Retention.RetentionPolicy.Executions)
            {
                var toDelete = collection
                    .Query()
                    .ToList()
                    .GroupBy(d => d.JobName)
                    .SelectMany(g => g.Skip(_defaultRetention.Value).Select(d => d.ID))
                    .ToList();
                collection.DeleteMany(d => toDelete.Contains(d.ID));
            }
            _logger.LogDebug("Cleanup finished!");
        }

        public void Delete(string jobName, Retention retention)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ExecutionHistoryDetails> Get(string jobName)
        {
            var collection = _liteDb.GetCollection<ExecutionHistoryDetails>();

            return collection
                .Query()
                .Where(d => jobName.Equals(d.JobName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public IEnumerable<ExecutionHistoryDetails> GetLastExecutions(IEnumerable<string> jobNames)
        {
            var collection = _liteDb.GetCollection<ExecutionHistoryDetails>();

            return collection
                .Query()
                .Where(d => jobNames.Contains(d.JobName))
                .ToList()
                .GroupBy(d => d.JobName)
                .Select(g => g.OrderByDescending(d => d.ExecutedAt).First())
                .ToList();
        }

        public void Save(ExecutionHistoryDetails historyDetails)
        {
            var collection = _liteDb.GetCollection<ExecutionHistoryDetails>();
            var existing = collection
                .Find(i => i.ID == historyDetails.ID)
                .FirstOrDefault();
            if (existing != null)
                collection.Delete(existing.ID);

            collection.Insert(historyDetails);
            collection.EnsureIndex(det => det.JobName, false);
        }

        #region IDisposable implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cleanup?.Dispose();
                    _liteDb?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
