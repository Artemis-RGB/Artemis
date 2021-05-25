using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin prerequisite action that runs a PowerShell script
    ///     <para>Note: To run an inline script instead, use <see cref="RunInlinePowerShellAction" /></para>
    /// </summary>
    public class RunPowerShellAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="scriptPath">The full path of the script to run</param>
        /// <param name="elevate">A boolean indicating whether the file should run with administrator privileges</param>
        public RunPowerShellAction(string name, string scriptPath, bool elevate = false) : base(name)
        {
            ScriptPath = scriptPath;
            Elevate = elevate;
            ProgressIndeterminate = true;
        }

        /// <summary>
        ///     Gets the inline full path of the script to run
        /// </summary>
        public string ScriptPath { get; }

        /// <summary>
        ///     Gets a boolean indicating whether the file should run with administrator privileges
        /// </summary>
        public bool Elevate { get; }

        /// <inheritdoc />
        public override async Task Execute(CancellationToken cancellationToken)
        {
            if (!ScriptPath.EndsWith(".ps1"))
                throw new ArtemisPluginException($"Script at path {ScriptPath} must have the .ps1 extension or PowerShell will refuse to run it");
            if (!File.Exists(ScriptPath))
                throw new ArtemisCoreException($"Script not found at path {ScriptPath}");

            Status = "Running PowerShell script and waiting for exit..";
            ShowProgressBar = true;
            ProgressIndeterminate = true;

            int result = await ExecuteFileAction.RunProcessAsync("powershell.exe", $"-File {ScriptPath}", Elevate);

            Status = $"PowerShell exited with code {result}";
        }
    }
}