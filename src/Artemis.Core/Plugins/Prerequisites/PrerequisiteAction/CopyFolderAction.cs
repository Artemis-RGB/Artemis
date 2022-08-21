using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;

namespace Artemis.Core;

/// <summary>
///     Represents a plugin prerequisite action that copies a folder
/// </summary>
public class CopyFolderAction : PluginPrerequisiteAction
{
    /// <summary>
    ///     Creates a new instance of a copy folder action
    /// </summary>
    /// <param name="name">The name of the action</param>
    /// <param name="source">The source folder to copy</param>
    /// <param name="target">The target folder to copy to (will be created if needed)</param>
    public CopyFolderAction(string name, string source, string target) : base(name)
    {
        Source = source;
        Target = target;

        ShowProgressBar = true;
        ShowSubProgressBar = true;
    }

    /// <summary>
    ///     Gets the source directory
    /// </summary>
    public string Source { get; }

    /// <summary>
    ///     Gets or sets the target directory
    /// </summary>
    public string Target { get; }

    /// <inheritdoc />
    public override async Task Execute(CancellationToken cancellationToken)
    {
        DirectoryInfo source = new(Source);
        DirectoryInfo target = new(Target);

        if (!source.Exists)
            throw new ArtemisCoreException($"The source directory at '{source}' was not found.");

        int filesCopied = 0;
        FileInfo[] files = source.GetFiles("*", SearchOption.AllDirectories);

        foreach (FileInfo fileInfo in files)
        {
            string outputPath = fileInfo.FullName.Replace(source.FullName, target.FullName);
            string outputDir = Path.GetDirectoryName(outputPath)!;
            Utilities.CreateAccessibleDirectory(outputDir);

            void SubProgressOnProgressReported(object? sender, EventArgs e)
            {
                if (SubProgress.ProgressPerSecond != 0)
                    Status = $"Copying {fileInfo.Name} - {SubProgress.ProgressPerSecond.Bytes().Humanize("#.##")}/sec";
                else
                    Status = $"Copying {fileInfo.Name}";
            }

            Progress.Report((filesCopied, files.Length));
            SubProgress.ProgressReported += SubProgressOnProgressReported;

            await using FileStream sourceStream = fileInfo.OpenRead();
            await using FileStream destinationStream = File.Create(outputPath);

            await sourceStream.CopyToAsync(fileInfo.Length, destinationStream, SubProgress, cancellationToken);

            filesCopied++;
            SubProgress.ProgressReported -= SubProgressOnProgressReported;
        }

        Progress.Report((filesCopied, files.Length));
        Status = $"Finished copying {filesCopied} file(s)";
    }
}