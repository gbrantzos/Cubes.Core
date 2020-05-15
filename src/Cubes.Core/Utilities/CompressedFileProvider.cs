using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Cubes.Core.Utilities
{
    public class CompressedFileProvider : IFileProvider, IDisposable
    {
        private readonly string rootFolder;
        private readonly string filePath;

        private readonly ConcurrentDictionary<string, IFileInfo> cache;
        private readonly FileSystemWatcher fileWatcher;
        private readonly PhysicalFileProvider physicalFileProvider;
        private char pathSeparator = '/';
        private MemoryStream inMemoryZip;
        private ZipArchive archive;
        private List<ZipArchiveEntry> entries;
        private int readTries = 0;
        private bool disposedValue = false;

        public CompressedFileProvider(string filePath,string rootFolder)
        {
            var debouncer = new Debouncer(TimeSpan.FromSeconds(2));
            this.filePath   = filePath.ThrowIfEmpty(nameof(filePath));
            this.rootFolder = rootFolder.ThrowIfNull(nameof(rootFolder));
            this.cache = new ConcurrentDictionary<string, IFileInfo>();

            ProcessZip();

            var actualPath = Path.GetFullPath(filePath);
            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = Path.GetDirectoryName(actualPath);
            fileWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
            fileWatcher.Filter = Path.GetFileName(actualPath);
            fileWatcher.Changed += (s, e) => debouncer.Debounce(() => ProcessZip());
            fileWatcher.Created += (s, e) => debouncer.Debounce(() => ProcessZip());
            fileWatcher.Renamed += (s, e) => debouncer.Debounce(() => ProcessZip());
            fileWatcher.EnableRaisingEvents = true;

            physicalFileProvider = new PhysicalFileProvider(fileWatcher.Path);
        }

        public CompressedFileProvider(string filePath) : this(filePath, String.Empty) { }

        private void ProcessZip()
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                inMemoryZip = new MemoryStream();
                fileStream.CopyTo(inMemoryZip);
            }
            catch (IOException ex)
            {
                // Cannot access file? Possibly someone else is working with it. Retry...
                readTries++;
                if (readTries >= 3)
                    throw new Exception($"Could not precess changes on file {filePath}", ex);
                Thread.Sleep(1000);
                ProcessZip();
            }

            // Cleanup...
            archive?.Dispose();
            cache.Clear();

            readTries = 0;
            archive = new ZipArchive(inMemoryZip, ZipArchiveMode.Read);
            entries = archive.Entries.ToList();

            var temp = entries
                .First(e => e.FullName.Contains('/') || e.FullName.Contains('\\'))
                .FullName;
            if (temp.Contains('/'))
                pathSeparator = '/';
            if (temp.Contains('\\'))
                pathSeparator = '\\';
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath.StartsWith("/", StringComparison.Ordinal))
                subpath = subpath[1..];
            if (!String.IsNullOrEmpty(rootFolder))
                subpath = $"{rootFolder}/{subpath}";
            subpath = subpath.Replace('/', pathSeparator);

            if (!cache.TryGetValue(subpath, out var fi))
            {
                var entry = entries
                    .Find(e => e.FullName.Equals(subpath, StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                    return new NotFoundFileInfo(subpath);

                fi = new CompressedFileInfo(entry, DateTime.UtcNow);
                cache.AddOrUpdate(subpath, fi, (k, v) => v = fi);
            }
            return fi;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var contents = entries
                .Select(e => new CompressedFileInfo(e, DateTime.UtcNow))
                .ToList();
            return new CompressedDirectoryContents(contents);
        }

        public IChangeToken Watch(string filter) => physicalFileProvider.Watch(filter);

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    physicalFileProvider?.Dispose();
                    fileWatcher?.Dispose();
                    archive?.Dispose();
                    inMemoryZip?.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() { Dispose(true); }
        #endregion
    }

    public class CompressedFileInfo : IFileInfo
    {
        private readonly ZipArchiveEntry archiveEntry;
        private readonly DateTime lastModified;

        public CompressedFileInfo(ZipArchiveEntry archiveEntry, DateTime lastModified)
        {
            this.lastModified = lastModified;
            this.archiveEntry = archiveEntry;
        }

        public Stream CreateReadStream() => archiveEntry.Open();

        public bool Exists => true;

        public long Length => archiveEntry?.Length ?? 0;

        public string PhysicalPath => null;

        public string Name => archiveEntry.Name;

        public DateTimeOffset LastModified => lastModified;

        public bool IsDirectory => false;
    }

    public class CompressedDirectoryContents : IDirectoryContents
    {
        private IEnumerable<IFileInfo> entries;

        public CompressedDirectoryContents(IEnumerable<IFileInfo> entries) => this.entries = entries;

        public bool Exists => true;

        public IEnumerator<IFileInfo> GetEnumerator() => entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => entries.GetEnumerator();
    }
}
