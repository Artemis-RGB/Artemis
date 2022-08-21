using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.Core;

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
    /// <param name="elevate">A boolean indicating whether the file should run with administrator privileges</param>
    public ExecuteFileAction(string name, string fileName, string? arguments = null, bool waitForExit = true, bool elevate = false) : base(name)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        Arguments = arguments;
        WaitForExit = waitForExit;
        Elevate = elevate;
    }

    /// <summary>
    ///     Gets the target file to execute
    /// </summary>
    public string FileName { get; }

    /// <summary>
    ///     Gets a set of command-line arguments to use when starting the application
    /// </summary>
    public string? Arguments { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the action should wait for the process to exit
    /// </summary>
    public bool WaitForExit { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the file should run with administrator privileges
    /// </summary>
    public bool Elevate { get; }

    /// <inheritdoc />
    public override async Task Execute(CancellationToken cancellationToken)
    {
        if (WaitForExit)
        {
            Status = $"Running {FileName} and waiting for exit..";
            ShowProgressBar = true;
            ProgressIndeterminate = true;

            int result = await RunProcessAsync(FileName, Arguments, Elevate);

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

    internal static Task<int> RunProcessAsync(string fileName, string? arguments, bool elevate)
    {
        TaskCompletionSource<int> tcs = new();

        Process process = new()
        {
            StartInfo =
            {
                FileName = fileName,
                Arguments = arguments!,
                Verb = elevate ? "RunAs" : "",
                UseShellExecute = elevate
            },
            EnableRaisingEvents = true
        };

        process.Exited += (_, _) =>
        {
            tcs.SetResult(process.ExitCode);
            process.Dispose();
        };

        try
        {
            process.Start();
        }
        catch (Win32Exception e)
        {
            if (!elevate || e.NativeErrorCode != 0x4c7)
                throw;
            tcs.SetResult(-1);
        }


        return tcs.Task;
    }
}