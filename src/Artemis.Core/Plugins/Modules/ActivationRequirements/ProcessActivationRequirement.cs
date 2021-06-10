﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Artemis.Core.Services;
using Ninject;

namespace Artemis.Core.Modules
{
    /// <summary>
    ///     Evaluates to true or false by checking if the specified process is running
    /// </summary>
    public class ProcessActivationRequirement : IModuleActivationRequirement
    {
        private readonly IProcessMonitorService _processMonitorService;

        /// <summary>
        ///     Creates a new instance of the <see cref="ProcessActivationRequirement" /> class
        /// </summary>
        /// <param name="processName">The name of the process that must run</param>
        /// <param name="location">The location of where the process must be running from (optional)</param>
        public ProcessActivationRequirement(string? processName, string? location = null)
        {
            if (string.IsNullOrWhiteSpace(processName) && string.IsNullOrWhiteSpace(location))
                throw new ArgumentNullException($"Atleast one {nameof(processName)} and {nameof(location)} must not be null");

            // Let's not make a habit out of this :P
            if (CoreService.Kernel == null)
                throw new ArtemisCoreException("Cannot create a ProcessActivationRequirement before initializing the Core");
            _processMonitorService = CoreService.Kernel.Get<IProcessMonitorService>();

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
            if (ProcessName == null && Location == null)
                return false;

            IEnumerable<Process> processes = _processMonitorService.GetRunningProcesses();
            if (ProcessName != null)
                processes = processes.Where(p => string.Equals(p.ProcessName, ProcessName, StringComparison.InvariantCultureIgnoreCase));
            if (Location != null)
                processes = processes.Where(p => string.Equals(Path.GetDirectoryName(p.GetProcessFilename()), Location, StringComparison.InvariantCultureIgnoreCase));

            return processes.Where(p => !p.HasExited).Any();
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