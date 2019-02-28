using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Cubes.Core.Settings
{
    public sealed class FileWatcherExtended
    {
        private ConcurrentDictionary<string, long> lastWrite = new ConcurrentDictionary<string, long>();
        private FileSystemWatcher watcher;

        public FileWatcherExtended(string basePath, string fileFilter)
        {
            // Prepare watcher
            watcher = new FileSystemWatcher();
            watcher.Path = basePath;
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                    | NotifyFilters.LastWrite
                                    | NotifyFilters.FileName;
            watcher.Filter = fileFilter;
            watcher.IncludeSubdirectories = true;

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += (s, e) =>
            {
                if (!lastWrite.TryRemove(e.FullPath, out long ignore))
                {
                    Task.Delay(50).Wait();
                    lastWrite.TryRemove(e.FullPath, out ignore);
                }
            };

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // For debugging reasons only
            // Console.WriteLine($"{e.Name} - {e.ChangeType}");
            var key = e.FullPath;
            var lastWriteTime = File.GetLastWriteTime(e.FullPath);
            lastWrite.AddOrUpdate(key,
                lastWriteTime.Ticks,
                (k, v) => lastWriteTime.Ticks);
        }

        // Get last modified date for file
        public DateTime LastModified(string file)
        {
            if (lastWrite.TryGetValue(file, out var ticks))
                return new DateTime(ticks);
            return DateTime.MinValue;
        }
    }
}