using System.IO.Compression;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Utilities;
using Newtonsoft.Json;

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

            using ZipArchive zipArchive = new(stream, ZipArchiveMode.Read);
            List<ZipArchiveEntry> profiles = zipArchive.Entries.Where(e => e.Name.EndsWith("json", StringComparison.InvariantCultureIgnoreCase)).ToList();
            ZipArchiveEntry userProfileEntry = profiles.First();
            ProfileConfigurationExportModel profile = await GetProfile(userProfileEntry);
            
            ProfileCategory category = _profileService.ProfileCategories.FirstOrDefault(c => c.Name == "Workshop") ?? _profileService.CreateProfileCategory("Workshop", true);
            ProfileConfiguration profileConfiguration = _profileService.ImportProfile(category, profile, true, true, null);
            return EntryInstallResult<ProfileConfiguration>.FromSuccess(profileConfiguration);
        }
        catch (Exception e)
        {
            return EntryInstallResult<ProfileConfiguration>.FromFailure(e.Message);
        }
    }

    private async Task<ProfileConfigurationExportModel> GetProfile(ZipArchiveEntry userProfileEntry)
    {
        await using Stream stream = userProfileEntry.Open();
        using StreamReader reader = new(stream);

        return JsonConvert.DeserializeObject<ProfileConfigurationExportModel>(await reader.ReadToEndAsync(), IProfileService.ExportSettings)!;
    }
}