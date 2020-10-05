using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Artemis.Core.Modules
{
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
        public ProcessActivationRequirement(string processName, string location = null)
        {
            ProcessName = processName;
            Location = location;
        }

        /// <summary>
        ///     The name of the process that must run
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        ///     The location of where the process must be running from
        /// </summary>
        public string Location { get; set; }

        /// <inheritdoc />
        public bool Evaluate()
        {
            if (ProcessName == null && Location == null)
                return false;

            IEnumerable<Process> processes = ProcessName != null ? Process.GetProcessesByName(ProcessName).Where(p => !p.HasExited) : Process.GetProcesses().Where(p => !p.HasExited);
            return Location != null
                ? processes.Any(p => string.Equals(Path.GetDirectoryName(p.GetProcessFilename()), Location, StringComparison.CurrentCultureIgnoreCase))
                : processes.Any();
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
}