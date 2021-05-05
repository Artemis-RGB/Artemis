using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin prerequisite action that executes a file
    /// </summary>
    public class ExecuteFileAction : PluginPrerequisiteAction
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ExecuteFileAction" />
        /// </summary>
        /// <param name="name">The name of the action</param>
        /// <param name="fileName">The target file to execute</param>
        /// <param name="arguments">A set of command-line arguments to use when starting the application</param>
        /// <param name="waitForExit">A boolean indicating whether the action should wait for the process to exit</param>
        public ExecuteFileAction(string name, string fileName, string? arguments = null, bool waitForExit = true) : base(name)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Arguments = arguments;
            WaitForExit = waitForExit;
        }

        /// <summary>
        ///     Gets the target file to execute
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets a set of command-line arguments to use when starting the application
        /// </summary>
        public string? Arguments { get; }

        /// <summary>
        ///     Gets a boolean indicating whether the action should wait for the process to exit
        /// </summary>
        public bool WaitForExit { get; }

        /// <inheritdoc />
        public override async Task Execute(CancellationToken cancellationToken)
        {
            if (WaitForExit)
            {
                Status = $"Running {FileName} and waiting for exit..";
                ShowProgressBar = true;
                ProgressIndeterminate = true;

                int result = await RunProcessAsync(FileName, Arguments);

                Status = $"{FileName} exited with code {result}";
            }
            else
            {
                Status = $"Running {FileName}";
                Process process = new()
                {
                    StartInfo = {FileName = FileName, Arguments = Arguments!},
                    EnableRaisingEvents = true
                };
                process.Start();
            }
        }

        private static Task<int> RunProcessAsync(string fileName, string? arguments)
        {
            TaskCompletionSource<int> tcs = new();

            Process process = new()
            {
                StartInfo = {FileName = fileName, Arguments = arguments!},
                EnableRaisingEvents = true
            };

            process.Exited += (_, _) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }
    }
}