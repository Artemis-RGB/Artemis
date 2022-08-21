using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core;

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
    /// <param name="arguments">
    ///     Optional arguments to pass to your script, you are responsible for proper quoting etc.
    ///     <para>Arguments are available in PowerShell as <c>$args[0], $args[1]</c> etc.</para>
    /// </param>
    public RunPowerShellAction(string name, string scriptPath, bool elevate = false, string? arguments = null) : base(name)
    {
        ScriptPath = scriptPath;
        Elevate = elevate;
        Arguments = arguments;
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

    /// <summary>
    ///     Gets optional arguments to pass to your script, you are responsible for proper quoting etc.
    ///     <para>Arguments are available in PowerShell as <c>$args[0], $args[1]</c> etc.</para>
    /// </summary>
    public string? Arguments { get; }

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

        int result = await ExecuteFileAction.RunProcessAsync("powershell.exe", $"-ExecutionPolicy Unrestricted -File {ScriptPath} {Arguments}", Elevate);

        Status = $"PowerShell exited with code {result}";
    }
}