using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin prerequisite action that deletes a file
    /// </summary>
    public class DeleteFileAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="target">The target folder to delete recursively</param>
        public DeleteFileAction(string name, string target) : base(name)
        {
            Target = target;
            ProgressIndeterminate = true;
        }

        /// <summary>
        ///     Gets or sets the target directory
        /// </summary>
        public string Target { get; }

        /// <inheritdoc />
        public override async Task Execute(CancellationToken cancellationToken)
        {
            ShowProgressBar = true;
            Status = $"Removing {Target}";

            await Task.Run(() =>
            {
                if (File.Exists(Target))
                    File.Delete(Target);
            }, cancellationToken);

            ShowProgressBar = false;
            Status = $"Removed {Target}";
        }
    }
}