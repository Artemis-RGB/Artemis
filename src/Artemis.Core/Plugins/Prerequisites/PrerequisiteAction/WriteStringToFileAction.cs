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
    public class WriteStringToFileAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="target">The target file to write to (will be created if needed)</param>
        /// <param name="content">The contents to write</param>
        public WriteStringToFileAction(string name, string target, string content) : base(name)
        {
            Target = target;
            Content = content ?? throw new ArgumentNullException(nameof(content));

            ProgressIndeterminate = true;
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
        ///     Gets the string that will be written
        /// </summary>
        public string Content { get; }

        /// <inheritdoc />
        public override async Task Execute(CancellationToken cancellationToken)
        {
            string outputDir = Path.GetDirectoryName(Target)!;
            Utilities.CreateAccessibleDirectory(outputDir);

            ShowProgressBar = true;
            Status = $"Writing to {Path.GetFileName(Target)}...";

            if (Append)
                await File.AppendAllTextAsync(Target, Content, cancellationToken);
            else
                await File.WriteAllTextAsync(Target, Content, cancellationToken);

            ShowProgressBar = false;
            Status = $"Finished writing to {Path.GetFileName(Target)}";
        }
    }
}