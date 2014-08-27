using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XsdDocumentation.Build
{
    public sealed class Zip : Task
    {
        public string WorkingDirectory { get; set; }

        [Required]
        public string ZipFileName { get; set; }

        [Required]
        public string[] Files { get; set; }

        public override bool Execute()
        {
            try
            {
                CreateZipArchive(WorkingDirectory, ZipFileName, Files);
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