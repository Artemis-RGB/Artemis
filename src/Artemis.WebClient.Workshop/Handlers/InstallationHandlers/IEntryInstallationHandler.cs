using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public interface IEntryInstallationHandler
{
    Task<EntryInstallResult> InstallAsync(IGetEntryById_Entry entry, long releaseId, Progress<StreamProgress> progress, CancellationToken cancellationToken);
    Task<EntryUninstallResult> UninstallAsync(InstalledEntry installedEntry, CancellationToken cancellationToken);
}