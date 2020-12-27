using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Artemis.UI.Installer.Services.Prerequisites;

namespace Artemis.UI.Installer.Services
{
    public class InstallationService : IInstallationService
    {
        public InstallationService(IEnumerable<IPrerequisite> prerequisites)
        {
            InstallationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Artemis");
            Prerequisites = prerequisites.ToList();
        }

        public string InstallationDirectory { get; set; }
        public List<IPrerequisite> Prerequisites { get; }
    }
}