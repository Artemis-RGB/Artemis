using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin prerequisite action that copies a folder
    /// </summary>
    public class WriteToFileAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="target">The target file to write to (will be created if needed)</param>
        /// <param name="content">The contents to write</param>
        public WriteToFileAction(string name, string target, string content) : base(name)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="target">The target file to write to (will be created if needed)</param>
        /// <param name="content">The contents to write</param>
        public WriteToFileAction(string name, string target, byte[] content) : base(name)
        {
            Target = target;
            ByteContent = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        ///     Gets or sets the target file
        /// </summary>
        public string Target { get; }

        /// <summary>
        ///     Gets the contents that will be written
        /// </summary>
        public string? Content { get; }

        /// <summary>
        ///     Gets the bytes that will be written
        /// </summary>
        public byte[]? ByteContent { get; }
        
        /// <inheritdoc />
        public override async Task Execute(CancellationToken cancellationToken)
        {
            string outputDir = Path.GetDirectoryName(Target)!;
            Utilities.CreateAccessibleDirectory(outputDir);

            ProgressIndeterminate = true;
            Status = $"Writing to {Path.GetFileName(Target)}...";

            if (Content != null)
                await File.WriteAllTextAsync(Target, Content, cancellationToken);
            else if (ByteContent != null)
                await File.WriteAllBytesAsync(Target, ByteContent, cancellationToken);

            ProgressIndeterminate = false;
            Status = $"Finished writing to {Path.GetFileName(Target)}";
        }
    }
}