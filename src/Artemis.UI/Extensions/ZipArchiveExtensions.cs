using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Artemis.UI.Extensions;

// Taken from System.IO.Compression with progress reporting slapped on top
public static class ZipArchiveExtensions
{
    /// <summary>
    /// Extracts all the files in the zip archive to a directory on the file system.
    /// </summary>
    /// <param name="source">The zip archive to extract files from.</param>
    /// <param name="destinationDirectoryName">The path to the directory to place the extracted files in. You can specify either a relative or an absolute path. A relative path is interpreted as relative to the current working directory.</param>
    /// <param name="overwriteFiles">A boolean indicating whether to override existing files</param>
    /// <param name="progress">The progress to report to.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    public static void ExtractToDirectory(this ZipArchive source, string destinationDirectoryName, bool overwriteFiles, IProgress<float> progress, CancellationToken cancellationToken)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (destinationDirectoryName == null)
            throw new ArgumentNullException(nameof(destinationDirectoryName));

        for (int index = 0; index < source.Entries.Count; index++)
        {
            ZipArchiveEntry entry = source.Entries[index];
            entry.ExtractRelativeToDirectory(destinationDirectoryName, overwriteFiles);
            progress.Report((index + 1f) / source.Entries.Count * 100f);
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private static void ExtractRelativeToDirectory(this ZipArchiveEntry source, string destinationDirectoryName, bool overwrite)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (destinationDirectoryName == null)
            throw new ArgumentNullException(nameof(destinationDirectoryName));

        // Note that this will give us a good DirectoryInfo even if destinationDirectoryName exists:
        DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
        string destinationDirectoryFullPath = di.FullName;
        if (!destinationDirectoryFullPath.EndsWith(Path.DirectorySeparatorChar))
            destinationDirectoryFullPath += Path.DirectorySeparatorChar;

        string fileDestinationPath = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, source.FullName));

        if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, StringComparison))
            throw new IOException($"The file '{fileDestinationPath}' already exists.");

        if (Path.GetFileName(fileDestinationPath).Length == 0)
        {
            // If it is a directory:

            if (source.Length != 0)
                throw new IOException("Extracting Zip entry would have resulted in a file outside the specified destination directory.");

            Directory.CreateDirectory(fileDestinationPath);
        }
        else
        {
            // If it is a file:
            // Create containing directory:
            Directory.CreateDirectory(Path.GetDirectoryName(fileDestinationPath)!);
            source.ExtractToFile(fileDestinationPath, overwrite: overwrite);
        }
    }
    private static StringComparison StringComparison => IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    private static bool IsCaseSensitive => !(OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsIOS() || OperatingSystem.IsTvOS() || OperatingSystem.IsWatchOS());
}