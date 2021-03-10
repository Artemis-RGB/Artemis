using System;

namespace Artemis.Core.Services
{
    public class ProcessEventArgs : EventArgs
    {
        public string ProcessName { get; }

        public ProcessEventArgs(string name)
        {
            ProcessName = name;
        }
    }
}
