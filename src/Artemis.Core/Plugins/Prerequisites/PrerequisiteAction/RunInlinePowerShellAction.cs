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
        public RunInlinePowerShellAction(string name, string code, bool elevate = false) : base(name)
        {
            Code = code;
            Elevate = elevate;
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

                int result = await ExecuteFileAction.RunProcessAsync("powershell.exe", $"-File {file}", Elevate);

                Status = $"PowerShell exited with code {result}";
            }
            finally
            {
                File.Delete(file);
            }
        }
    }
}