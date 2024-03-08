using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class ProfileEntryInstallationHandler : IEntryInstallationHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IProfileService _profileService;
    private readonly IWorkshopService _workshopService;

    public ProfileEntryInstallationHandler(IHttpClientFactory httpClientFactory, IProfileService profileService, IWorkshopService workshopService)
    {
        _httpClientFactory = httpClientFactory;
        _profileService = profileService;
        _workshopService = workshopService;
    }

    public async Task<EntryInstallResult> InstallAsync(IEntryDetails entry, IRelease release, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        using MemoryStream stream = new();

        // Download the provided release
        try
        {
            HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
            await client.DownloadDataAsync($"releases/download/{release.Id}", stream, progress, cancellationToken);
        }
        catch (Exception e)
        {
            return EntryInstallResult.FromFailure(e.Message);
        }

        // Find existing installation to potentially replace the profile
        InstalledEntry? installedEntry = _workshopService.GetInstalledEntry(entry.Id);
        if (installedEntry != null && installedEntry.TryGetMetadata("ProfileId", out Guid profileId))
        {
            ProfileConfiguration? existing = _profileService.ProfileCategories.SelectMany(c => c.ProfileConfigurations).FirstOrDefault(c => c.ProfileId == profileId);
            if (existing != null)
            {
                ProfileConfiguration overwritten = await _profileService.OverwriteProfile(stream, existing);
                installedEntry.SetMetadata("ProfileId", overwritten.ProfileId);

                // Update the release and return the profile configuration
                UpdateRelease(installedEntry, release);
                return EntryInstallResult.FromSuccess(installedEntry);
            }
        }

        // Ensure there is an installed entry
        installedEntry ??= new InstalledEntry(entry, release);

        // Add the profile as a fresh import
        ProfileCategory category = _profileService.ProfileCategories.FirstOrDefault(c => c.Name == "Workshop") ?? _profileService.CreateProfileCategory("Workshop", true);
        ProfileConfiguration imported = await _profileService.ImportProfile(stream, category, true, true, null);
        installedEntry.SetMetadata("ProfileId", imported.ProfileId);
        
        // Update the release and return the profile configuration
        UpdateRelease(installedEntry, release);
        return EntryInstallResult.FromSuccess(installedEntry);
    }

    public async Task<EntryUninstallResult> UninstallAsync(InstalledEntry installedEntry, CancellationToken cancellationToken)
    {
        if (!installedEntry.TryGetMetadata("ProfileId", out Guid profileId))
            return EntryUninstallResult.FromFailure("Local reference does not contain a GUID");

        return await Task.Run(() =>
        {
            try
            {
                // Find the profile if still there
                ProfileConfiguration? profile = _profileService.ProfileCategories.SelectMany(c => c.ProfileConfigurations).FirstOrDefault(c => c.ProfileId == profileId);
                if (profile != null)
                    _profileService.DeleteProfile(profile);

                // Remove the release
                _workshopService.RemoveInstalledEntry(installedEntry);
            }
            catch (Exception e)
            {
                return EntryUninstallResult.FromFailure(e.Message);
            }

            return EntryUninstallResult.FromSuccess();
        }, cancellationToken);
    }

    private void UpdateRelease(InstalledEntry installedEntry, IRelease release)
    {
        installedEntry.ApplyRelease(release);
        _workshopService.SaveInstalledEntry(installedEntry);
    }
}