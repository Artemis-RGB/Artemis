using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin prerequisite action runs inline powershell
    /// </summary>
    public class RunInlinePowerShellAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of a copy folder action
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="code">The inline code to run</param>
        /// <param name="elevate">A boolean indicating whether the file should run with administrator privileges</param>
        /// <param name="arguments">
        ///     Optional arguments to pass to your script, you are responsible for proper quoting etc.
        ///     <para>Arguments are available in PowerShell as <c>$args[0], $args[1]</c> etc.</para>
        /// </param>
        public RunInlinePowerShellAction(string name, string code, bool elevate = false, string? arguments = null) : base(name)
        {
            Code = code;
            Elevate = elevate;
            Arguments = arguments;
            ProgressIndeterminate = true;
        }

        /// <summary>
        ///     Gets the inline code to run
        /// </summary>
        public string Code { get; }

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
            string file = Path.GetTempFileName().Replace(".tmp", ".ps1");
            try
            {
                string code =
                    @"try 
                    { 
                        " + Code + @"
                        Start-Sleep 1
                    }
                    catch
                    {
                        Write-Error $_.Exception.ToString()
                        pause
                    }";
                
                await File.WriteAllTextAsync(file, code, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                Status = "Running PowerShell script and waiting for exit..";
                ShowProgressBar = true;
                ProgressIndeterminate = true;

                int result = await ExecuteFileAction.RunProcessAsync("powershell.exe", $"-File {file} {Arguments}", Elevate);

                Status = $"PowerShell exited with code {result}";
            }
            finally
            {
                File.Delete(file);
            }
        }
    }
}