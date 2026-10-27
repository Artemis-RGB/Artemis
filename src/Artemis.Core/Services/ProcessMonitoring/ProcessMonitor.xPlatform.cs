using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Artemis.Core.Services;

public static partial class ProcessMonitor
{
    #region Methods

    private static void UpdateProcessInfosCrossPlatform(object? o)
    {
        lock (LOCK)
        {
            try
            {
                HashSet<int> processIds = _scannedProcessIds;
                processIds.Clear();

                foreach (Process process in Process.GetProcesses())
                {
                    int processId = process.Id;
                    processIds.Add(processId);

                    if (!_processes.ContainsKey(processId))
                    {
                        string imageName = string.Empty;
                        string processName = process.ProcessName;
                        string executable = string.Empty; //TODO DarthAffe 01.09.2023: Is there a crossplatform way to do this?

                        ProcessInfo processInfo = new(processId, processName, imageName, executable);
                        _processes.Add(processId, processInfo);
                        _processesSnapshot = default;

                        OnProcessStarted(processInfo);
                    }
                }

                HandleStoppedProcesses(processIds);

                LastUpdate = DateTime.Now;
            }
            catch { /* Should we throw here? I guess no ... */ }
        }
    }

    #endregion
}