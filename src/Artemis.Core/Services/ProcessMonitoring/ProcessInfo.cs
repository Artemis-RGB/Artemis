namespace Artemis.Core.Services;

public readonly struct ProcessInfo
{
    #region Properties & Fields

    public readonly int ProcessId;
    public readonly string ProcessName;
    public readonly string ImageName; //TODO DarthAffe 01.09.2023: Do we need this if we can't get it through Process.GetProcesses()?
    public readonly string Executable;

    #endregion

    #region Constructors

    public ProcessInfo(int processId, string processName, string imageName, string executable)
    {
        this.ProcessId = processId;
        this.ProcessName = processName;
        this.ImageName = imageName;
        this.Executable = executable;
    }

    #endregion
}