using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Models;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public interface IEntryInstallationHandler
{
    Task<EntryInstallResult> InstallAsync(IEntrySummary entry, IRelease release, Progress<StreamProgress> progress, CancellationToken cancellationToken);
    Task<EntryUninstallResult> UninstallAsync(InstalledEntry installedEntry, CancellationToken cancellationToken);
}