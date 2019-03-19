using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Cubes.Core.Environment;
using Cubes.Core.Settings;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Commands.Middleware.ExecutionHistory
{
    public class CommandExecutionHistory : ICommandExecutionHistory, IDisposable
    {
        private readonly LiteDatabase liteDb;
        private readonly ICubesEnvironment environment;
        private readonly ISettingsProvider settingsProvider;
        private readonly ILogger<CommandExecutionHistory> logger;
        private Timer timer;

        public CommandExecutionHistory(ICubesEnvironment environment, ISettingsProvider settingsProvider, ILogger<CommandExecutionHistory> logger)
        {
            var path = Path.Combine(environment.GetStorageFolder(), "Core.CommandExecutionHistory.db");
            var connectionString = $"Filename={path};Mode=Exclusive";

            this.environment      = environment;
            this.settingsProvider = settingsProvider;
            this.logger           = logger;
            this.liteDb           = new LiteDatabase(connectionString);

            // Setup clean up timer
            var settings = settingsProvider.Load<CommandExecutionHistorySettings>();
            var interval = 1000 * settings.MonitoringIntervalSecs;
            if (interval > 0)
                this.timer = new Timer((t) => ClearHistory(), null, interval, interval);
        }

        #region interface implementation
        public void Add(CommandExecution commandExecution)
        {
            var col = liteDb.GetCollection<CommandExecution>();
            col.Insert(commandExecution);
            col.EnsureIndex(c => c.CommandType);
            col.EnsureIndex(c => c.ExecutedAt);
        }

        public int Delete(string commandType, HistoryRetentionOptions options)
        {
            int entriesDeleted = 0;
            var col = liteDb.GetCollection<CommandExecution>();
            if (options.KeepLastDays.HasValue && options.KeepLastDays.Value >= 0)
            {
                var untilDate = DateTime.Now.Date.AddDays(-1 * options.KeepLastDays.Value);
                if (untilDate > DateTime.Now.Date) untilDate = DateTime.Now.Date;
                entriesDeleted = col.Delete(x => x.CommandType.Equals(commandType, StringComparison.CurrentCultureIgnoreCase) && x.ExecutedAt < untilDate);

                return entriesDeleted;
            }

            if (options.KeepLastTimes.HasValue && options.KeepLastTimes.Value >= 0)
            {
                var toDelete = col
                    .Find(x => x.CommandType.Equals(commandType, StringComparison.CurrentCultureIgnoreCase))
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

        public IEnumerable<CommandExecution> Get(string commandType)
        {
            var col = liteDb.GetCollection<CommandExecution>();
            return col.Find(c => c.CommandType.Equals(commandType, StringComparison.CurrentCultureIgnoreCase));
        }
        #endregion

        #region Helpers
        private void ClearHistory()
        {
            var settings = settingsProvider.Load<CommandExecutionHistorySettings>();
            if (this.timer != null)
                timer.Change(Timeout.Infinite, Timeout.Infinite);

            logger.LogDebug("House keeping ...");
            ClearHistoryInternal(settings);

            var interval = 1000 * settings.MonitoringIntervalSecs;
            if (interval > 0)
                timer.Change(interval, interval);
        }

        private void ClearHistoryInternal(CommandExecutionHistorySettings settings)
        {
            var options = settings.RetentionOptions;
            if (options == null)
                return;
            var col = liteDb.GetCollection<CommandExecution>();
            if (options.KeepLastDays.HasValue && options.KeepLastDays.Value >= 0)
            {
                var untilDate = DateTime.Now.Date.AddDays(-1 * options.KeepLastDays.Value);
                if (untilDate > DateTime.Now.Date) untilDate = DateTime.Now.Date;
                col.Delete(x => x.ExecutedAt < untilDate);

                return;
            }

            if (options.KeepLastTimes.HasValue && options.KeepLastTimes.Value >= 0)
            {
                var allCommandTypes = GetAvailableCommands();
                foreach (var commandType in allCommandTypes)
                {
                    var toDelete = col
                        .Find(x => x.CommandType.Equals(commandType, StringComparison.CurrentCultureIgnoreCase))
                        .OrderByDescending(x => x.ExecutedAt)
                        .Skip(options.KeepLastTimes.Value)
                        .Select(x => x.ID)
                        .ToList();
                    foreach (var item in toDelete)
                        col.Delete(new BsonValue(item));
                }
            }
        }

        private IEnumerable<string> GetAvailableCommands()
        {
            var commandType = typeof(ICommand<>);
            var commandTypes = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsCommand())
                .Select(t => t.FullName)
                .ToList();
            return commandTypes;
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ClearHistory();

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
