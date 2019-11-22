using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Cubes.Core.Utilities
{
    public class CompressedFileProvider : IFileProvider, IDisposable
    {
        private readonly string rootFolder;
        private readonly string filePath;

        private ConcurrentDictionary<string, IFileInfo> cache;
        private MemoryStream inMemoryZip;
        private ZipArchive archive;
        private List<ZipArchiveEntry> entries;
        private FileSystemWatcher fileWatcher;
        private bool disposedValue = false;

        public CompressedFileProvider(string filePath,string rootFolder)
        {
            this.filePath   = filePath.ThrowIfEmpty(nameof(filePath));
            this.rootFolder = rootFolder.ThrowIfNull(nameof(rootFolder));

            Initialize();

            var actualPath = Path.GetFullPath(filePath);
            fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(actualPath), Path.GetFileName(actualPath));
            fileWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite;
            fileWatcher.Changed += (s, e) => { Initialize(); };
            fileWatcher.EnableRaisingEvents = true;
        }

        public CompressedFileProvider(string filePath) : this(filePath, String.Empty) { }

        private void Initialize()
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            inMemoryZip = new MemoryStream();
            fileStream.CopyTo(inMemoryZip);

            archive = new ZipArchive(inMemoryZip, ZipArchiveMode.Read);
            cache   = new ConcurrentDictionary<string, IFileInfo>();
            entries = archive.Entries.ToList();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath.StartsWith("/", StringComparison.Ordinal))
                subpath = subpath[1..];
            if (!String.IsNullOrEmpty(rootFolder))
                subpath = $"{rootFolder}/{subpath}";
            subpath = subpath.Replace('/', Path.DirectorySeparatorChar);

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

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    fileWatcher?.Dispose();
                    archive?.Dispose();
                    inMemoryZip?.Dispose();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() => Dispose(true);
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
