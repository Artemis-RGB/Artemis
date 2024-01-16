namespace Artemis.Core.Services;

/// <summary>
/// This readonly struct provides information about a process.
/// </summary>
public readonly struct ProcessInfo
{
    #region Properties & Fields

    /// <summary>
    /// Gets the Identifier for the process.
    /// </summary>
    public readonly int ProcessId;

    /// <summary>
    /// Gets the name of the process.
    /// </summary>
    public readonly string ProcessName;

    /// <summary>
    /// Gets the Image Name of the Process.
    /// </summary>
    public readonly string ImageName; // TODO DarthAffe 01.09.2023: Do we need this if we can't get it through Process.GetProcesses()?
    
    /// <summary>
    /// Gets the Executable associated with the Process.
    /// </summary>
    public readonly string Executable;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessInfo"/> struct.
    /// </summary>
    /// <param name="processId">The identifier for the process.</param>
    /// <param name="processName">The name of the process.</param>
    /// <param name="imageName">The Image Name of the process.</param>
    /// <param name="executable">The executable associated with the process.</param>
    public ProcessInfo(int processId, string processName, string imageName, string executable)
    {
        ProcessId = processId;
        ProcessName = processName;
        ImageName = imageName;
        Executable = executable;
    }

    #endregion
}