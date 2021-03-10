using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.Core.Services
{
    public interface IProcessMonitorService : IArtemisService
    {
        event EventHandler<ProcessEventArgs> ProcessStarted;

        event EventHandler<ProcessEventArgs> ProcessStopped;

        IEnumerable<Process> GetRunningProcesses();
    }
}
