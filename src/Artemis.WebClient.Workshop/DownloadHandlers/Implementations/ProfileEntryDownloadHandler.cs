using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Utilities;

namespace Artemis.WebClient.Workshop.DownloadHandlers.Implementations;

public class ProfileEntryDownloadHandler : IEntryDownloadHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IProfileService _profileService;

    public ProfileEntryDownloadHandler(IHttpClientFactory httpClientFactory, IProfileService profileService)
    {
        _httpClientFactory = httpClientFactory;
        _profileService = profileService;
    }

    public async Task<EntryInstallResult<ProfileConfiguration>> InstallProfileAsync(Guid releaseId, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
            using MemoryStream stream = new();
            await client.DownloadDataAsync($"releases/download/{releaseId}", stream, progress, cancellationToken);

            ProfileCategory category = _profileService.ProfileCategories.FirstOrDefault(c => c.Name == "Workshop") ?? _profileService.CreateProfileCategory("Workshop", true);
            ProfileConfiguration profileConfiguration = await _profileService.ImportProfile(stream, category, true, true, null);
            return EntryInstallResult<ProfileConfiguration>.FromSuccess(profileConfiguration);
        }
        catch (Exception e)
        {
            return EntryInstallResult<ProfileConfiguration>.FromFailure(e.Message);
        }
    }
}