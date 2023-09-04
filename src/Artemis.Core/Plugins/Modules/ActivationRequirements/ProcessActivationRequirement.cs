using System;
using Artemis.Core.Services;

namespace Artemis.Core.Modules;

/// <summary>
///     Evaluates to true or false by checking if the specified process is running
/// </summary>
public class ProcessActivationRequirement : IModuleActivationRequirement
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ProcessActivationRequirement" /> class
    /// </summary>
    /// <param name="processName">The name of the process that must run</param>
    /// <param name="location">The location of where the process must be running from (optional)</param>
    public ProcessActivationRequirement(string? processName, string? location = null)
    {
        if (string.IsNullOrWhiteSpace(processName) && string.IsNullOrWhiteSpace(location))
            throw new ArgumentNullException($"Atleast one {nameof(processName)} and {nameof(location)} must not be null");

        ProcessName = processName;
        Location = location;
    }

    /// <summary>
    ///     The name of the process that must run
    /// </summary>
    public string? ProcessName { get; set; }

    /// <summary>
    ///     The location of where the process must be running from
    /// </summary>
    public string? Location { get; set; }

    /// <inheritdoc />
    public bool Evaluate()
    {
        return ProcessMonitor.IsProcessRunning(ProcessName, Location);
    }

    /// <inheritdoc />
    public string GetUserFriendlyDescription()
    {
        string description = $"Requirement met when \"{ProcessName}.exe\" is running";
        if (Location != null)
            description += $" from \"{Location}\"";

        return description;
    }
}