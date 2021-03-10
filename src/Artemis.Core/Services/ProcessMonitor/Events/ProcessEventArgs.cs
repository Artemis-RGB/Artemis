using System;
using System.Diagnostics;

namespace Artemis.Core.Services
{
    public class ProcessEventArgs : EventArgs
    {
        public Process Process { get; }

        public ProcessEventArgs(Process process)
        {
            Process = process;
        }
    }
}
