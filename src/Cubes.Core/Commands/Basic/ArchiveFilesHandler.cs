using System.Collections.Generic;
using System.IO.Compression;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cubes.Core.Base;

namespace Cubes.Core.Commands.Basic
{
    public class ArchiveFilesHandler : RequestHandler<ArchiveFiles, ArchiveFilesResult>
    {
        private readonly ICubesEnvironment cubesEnvironment;

        public ArchiveFilesHandler(ICubesEnvironment cubesEnvironment)
            => this.cubesEnvironment = cubesEnvironment;

        protected override Task<ArchiveFilesResult> HandleInternal(ArchiveFiles request, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(request.SourcePath))
                throw new ArgumentException($"Path '{request.SourcePath}' does not exist!");

            var filesFound = GetFilesForArchive(request);
            var archiveName = PrepareName(request.ArchiveName);
            var tempFile = Path.Combine(cubesEnvironment.GetFolder(CubesFolderKind.Temp), archiveName);
            if (File.Exists(tempFile))
                File.Delete(tempFile);

            using (var zipFile = ZipFile.Open(tempFile, ZipArchiveMode.Create))
            {
                foreach (var file in filesFound)
                {
                    var relativePath = Path.GetRelativePath(request.SourcePath, file);
                    zipFile.CreateEntryFromFile(file, relativePath);
                }
            }

            var targetFiles = new List<string>();
            foreach (var target in request.Targets)
            {
                CleanupArchives(target, request);

                var targetFile = Path.Combine(target, archiveName);
                if (File.Exists(targetFile))
                    File.Delete(targetFile);
                File.Copy(tempFile, targetFile);
                targetFiles.Add(targetFile);
            }
            File.Delete(tempFile);

            MessageToReturn = $"Created {targetFiles.Count} archive files";
            return Task.FromResult(new ArchiveFilesResult { ArchiveFiles = targetFiles.ToArray() });
        }

        private void CleanupArchives(string target, ArchiveFiles request)
        {
            var existingArchives = GetExistingArchives(target, request);
            while (existingArchives.Length >= request.FilesToKeep)
            {
                File.Delete(Path.Combine(target, existingArchives[0]));
                existingArchives = existingArchives[1..existingArchives.Length];
            }
        }

        private string[] GetExistingArchives(string path, ArchiveFiles request)
        {
            if (request.FilesToKeep <= 0 && !request.ArchiveName.Contains("{"))
                return Array.Empty<string>();

            var fileFilter = request.ArchiveName;
            while (true)
            {
                var startIndex = fileFilter.IndexOf("{");
                if (startIndex < 0) break;
                var endIndex = fileFilter.IndexOf("}", startIndex + 1);
                fileFilter = fileFilter[0..startIndex] + "*" + fileFilter.Substring(endIndex +1);
            }

            var result = Directory.GetFiles(path, fileFilter);
            Array.Sort(result);

            return result;
        }

        private IEnumerable<string> GetFilesForArchive(ArchiveFiles request)
        {
            var toReturn = new List<string>();
            var requestFilter = String.IsNullOrEmpty(request.Filter) ? "*" : request.Filter;
            foreach (var filter in (string[])requestFilter.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                toReturn.AddRange(Directory.GetFiles(request.SourcePath, filter));
            }
            return toReturn;
        }

        private string PrepareName(string archiveName)
        {
            var toReturn = archiveName;
            var startIndex = toReturn.IndexOf("{dt:");
            if (startIndex >= 0)
            {
                var endIndex = toReturn.IndexOf("}", startIndex + 1);
                var format = toReturn[(startIndex + "{dt:".Length)..endIndex];

                toReturn = toReturn[0..startIndex] + DateTime.Now.ToString(format) + toReturn.Substring(endIndex + 1);
            }
            return toReturn;
        }
    }
}