using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Core.Services
{
    /// <summary>
    /// A service that provides events for started and stopped processes and a list of all running processes.
    /// </summary>
    public interface IProcessMonitorService : IArtemisService
    {
        /// <summary>
        /// Occurs when a process starts.
        /// </summary>
        event EventHandler<ProcessEventArgs> ProcessStarted;

        /// <summary>
        /// Occurs when a process stops.
        /// </summary>
        event EventHandler<ProcessEventArgs> ProcessStopped;

        /// <summary>
        /// Returns an enumerable with the processes running on the system.
        /// </summary>
        IEnumerable<Process> GetRunningProcesses();
    }
}
