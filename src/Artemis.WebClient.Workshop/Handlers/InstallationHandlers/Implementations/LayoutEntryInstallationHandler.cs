using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class LayoutEntryInstallationHandler : IEntryInstallationHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWorkshopService _workshopService;

    public LayoutEntryInstallationHandler(IHttpClientFactory httpClientFactory, IWorkshopService workshopService)
    {
        _httpClientFactory = httpClientFactory;
        _workshopService = workshopService;
    }

    public async Task<EntryInstallResult> InstallAsync(IGetEntryById_Entry entry, long releaseId, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        using MemoryStream stream = new();

        // Download the provided release
        try
        {
            HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
            await client.DownloadDataAsync($"releases/download/{releaseId}", stream, progress, cancellationToken);
        }
        catch (Exception e)
        {
            return EntryInstallResult.FromFailure(e.Message);
        }
        
        // return EntryInstallResult.FromSuccess();
    }

    public async Task<EntryUninstallResult> UninstallAsync(InstalledEntry installedEntry, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        
        if (!Guid.TryParse(installedEntry.LocalReference, out Guid profileId))
            return EntryUninstallResult.FromFailure("Local reference does not contain a GUID");
    }
}