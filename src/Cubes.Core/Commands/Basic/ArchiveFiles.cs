namespace Cubes.Core.Commands.Basic
{
    public class ArchiveFiles : Request<ArchiveFilesResult>
    {
        /// <summary>
        /// Source path for searching files
        /// </summary>
        /// <value></value>
        public string SourcePath { get; set; }

        /// <summary>
        /// Filter for searching, multiple values separated by pipe (|)
        /// </summary>
        /// <value></value>
        public string Filter { get; set; }

        /// <summary>
        /// Targets for archive created
        /// </summary>
        /// <value></value>
        public string[] Targets { get; set; }

        /// <summary>
        /// Name for the archive.static Can contain:
        /// - {dt:format}, to add current datetime part, i.e "{dt:yyyyMMdd}".
        /// </summary>
        /// <value></value>
        public string ArchiveName { get; set; }

        /// <summary>
        /// Keep at most this number of files. Works only when positive number.
        /// </summary>
        /// <value></value>
        public int FilesToKeep { get; set; }
    }
}

// SAMPLE
//
// {
//   "sourcePath": "D:\\Code\\CubesNEXT\\Core\\src\\Cubes.Host\\bin\\Debug\\netcoreapp3.1",
//   "filter": "Settings\\*.yaml|Logs\\*",
//   "targets": ["D:\\wrk_Temp"],
//   "archiveName": "Backup.{dt:yyyyMMddHHmm}.zip",
//   "FilesToKeep": 2
// }
//