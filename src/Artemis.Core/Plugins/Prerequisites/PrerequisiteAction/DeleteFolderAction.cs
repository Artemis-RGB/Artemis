using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin prerequisite action that recursively deletes a folder
    /// </summary>
    public class DeleteFolderAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="target">The target folder to delete recursively</param>
        public DeleteFolderAction(string name, string target) : base(name)
        {
            if (Enum.GetValues<Environment.SpecialFolder>().Select(Environment.GetFolderPath).Contains(target))
                throw new ArtemisCoreException($"Cannot delete special folder {target}, silly goose.");

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
                if (Directory.Exists(Target))
                    Directory.Delete(Target, true);
            }, cancellationToken);

            ShowProgressBar = false;
            Status = $"Removed {Target}";
        }
    }
}