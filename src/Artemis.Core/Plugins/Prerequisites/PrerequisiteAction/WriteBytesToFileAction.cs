using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin prerequisite action that copies a folder
    /// </summary>
    public class WriteBytesToFileAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="target">The target file to write to (will be created if needed)</param>
        /// <param name="content">The contents to write</param>
        public WriteBytesToFileAction(string name, string target, byte[] content) : base(name)
        {
            Target = target;
            ByteContent = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        ///     Gets or sets the target file
        /// </summary>
        public string Target { get; }

        /// <summary>
        ///     Gets or sets a boolean indicating whether or not to append to the file if it exists already, if set to
        ///     <see langword="false" /> the file will be deleted and recreated
        /// </summary>
        public bool Append { get; set; } = false;

        /// <summary>
        ///     Gets the bytes that will be written
        /// </summary>
        public byte[] ByteContent { get; }

        /// <inheritdoc />
        public override async Task Execute(CancellationToken cancellationToken)
        {
            string outputDir = Path.GetDirectoryName(Target)!;
            Utilities.CreateAccessibleDirectory(outputDir);

            ShowProgressBar = true;
            Status = $"Writing to {Path.GetFileName(Target)}...";

            if (!Append && File.Exists(Target))
                File.Delete(Target);
            
            await using Stream fileStream = File.OpenWrite(Target);
            await using MemoryStream sourceStream = new(ByteContent);
            await sourceStream.CopyToAsync(sourceStream.Length, fileStream, Progress, cancellationToken);

            ShowProgressBar = false;
            Status = $"Finished writing to {Path.GetFileName(Target)}";
        }
    }
}