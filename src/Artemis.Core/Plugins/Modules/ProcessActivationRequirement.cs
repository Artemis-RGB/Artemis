using System.Diagnostics;
using System.IO;
using System.Linq;
using Artemis.Core.Extensions;

namespace Artemis.Core.Plugins.Modules
{
    public class ProcessActivationRequirement : IModuleActivationRequirement
    {
        public string ProcessName { get; set; }
        public string Location { get; set; }

        public ProcessActivationRequirement(string processName, string location = null)
        {
            ProcessName = processName;
            Location = location;
        }


        public bool Evaluate()
        {
            if (ProcessName == null && Location == null)
                return false;

            var processes = ProcessName != null ? Process.GetProcessesByName(ProcessName).Where(p => !p.HasExited) : Process.GetProcesses().Where(p => !p.HasExited);
            return Location != null 
                ? processes.Any(p => Path.GetDirectoryName(p.GetProcessFilename()) == Location)
                : processes.Any();
        }
    }

    public interface IModuleActivationRequirement
    {
        bool Evaluate();
    }
}