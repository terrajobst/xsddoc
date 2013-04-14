using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XsdDocumentation.Build
{
    public sealed class Zip : Task
    {
        public ITaskItem WorkingDirectory { get; set; }

        [Required]
        public ITaskItem ZipFileName { get; set; }

        [Required]
        public ITaskItem[] Files { get; set; }

        public override bool Execute()
        {
            try
            {
                var workingDirectory = WorkingDirectory.ItemSpec;
                var zipFileName = ZipFileName.ItemSpec;
                var fileNames = Files.Select(f => f.ItemSpec);
                CreateZipArchive(workingDirectory, zipFileName, fileNames);

            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }

            return !Log.HasLoggedErrors;
        }

        private static void CreateZipArchive(string workingDirectory, string zipFileName, IEnumerable<string> fileNames)
        {
            using (var fileStream = File.Create(zipFileName))
            using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
            {
                foreach (var fileName in fileNames)
                {
                    var relativePath = GetRelativePath(fileName, workingDirectory);
                    zipArchive.CreateEntryFromFile(fileName, relativePath, CompressionLevel.Optimal);
                }
            }
        }

        private static string GetRelativePath(string fileName, string workingDirectory)
        {
            if (!fileName.StartsWith(workingDirectory, StringComparison.OrdinalIgnoreCase))
                return Path.GetFileName(fileName);

            var backslashCompensation = workingDirectory.EndsWith("\\") ? 1 : 0;
            return fileName.Substring(workingDirectory.Length + backslashCompensation);
        }
    }
}