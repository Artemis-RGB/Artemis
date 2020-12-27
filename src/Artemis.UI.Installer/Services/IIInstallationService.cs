using System.Collections.Generic;
using Artemis.UI.Installer.Services.Prerequisites;

namespace Artemis.UI.Installer.Services
{
    public interface IInstallationService
    {
        string InstallationDirectory { get; set; }
        List<IPrerequisite> Prerequisites { get; }
    }
}