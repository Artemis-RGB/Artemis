using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin prerequisite action that extracts a ZIP file
    /// </summary>
    public class ExtractArchiveAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ExtractArchiveAction"/>.
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="fileName">The ZIP file to extract</param>
        /// <param name="target">The folder into which to extract the file</param>
        public ExtractArchiveAction(string name, string fileName, string target) : base(name)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Target = target ?? throw new ArgumentNullException(nameof(target));

            ShowProgressBar = true;
        }

        /// <summary>
        ///     Gets the file to extract
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     Gets the folder into which to extract the file
        /// </summary>
        public string Target { get; }

        /// <inheritdoc />
        public override async Task Execute(CancellationToken cancellationToken)
        {
            using HttpClient client = new();

            ShowSubProgressBar = true;
            Status = $"Extracting {FileName}";

            Utilities.CreateAccessibleDirectory(Target);

            await using (FileStream fileStream = new(FileName, FileMode.Open))
            {
                ZipArchive archive = new(fileStream);
                long count = 0;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    await using Stream unzippedEntryStream = entry.Open();
                    Progress.Report((count, archive.Entries.Count));
                    if (entry.Length > 0)
                    {
                        string path = Path.Combine(Target, entry.FullName);
                        CreateDirectoryForFile(path);
                        await using Stream extractStream = new FileStream(path, FileMode.OpenOrCreate);
                        await unzippedEntryStream.CopyToAsync(entry.Length, extractStream, SubProgress, cancellationToken);
                    }

                    count++;
                }
            }

            Progress.Report((1, 1));
            ShowSubProgressBar = false;
            Status = "Finished extracting";
        }

        private static void CreateDirectoryForFile(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            if (directory == null)
                throw new ArtemisCoreException($"Failed to get directory from path {path}");
            
            Utilities.CreateAccessibleDirectory(directory);
        }
    }
}