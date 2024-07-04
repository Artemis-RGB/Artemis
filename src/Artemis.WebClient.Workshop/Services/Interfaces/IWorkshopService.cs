using Artemis.Core;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Models;

namespace Artemis.WebClient.Workshop.Services;

/// <summary>
/// Provides an interface for managing workshop services.
/// </summary>
public interface IWorkshopService
{
    /// <summary>
    /// Gets the icon for a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream containing the icon.</returns>
    Task<Stream?> GetEntryIcon(long entryId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the icon for a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <param name="icon">The stream containing the icon.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An API result.</returns>
    Task<ApiResult> SetEntryIcon(long entryId, Stream icon, CancellationToken cancellationToken);

    /// <summary>
    /// Uploads an image for a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <param name="request">The image upload request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An API result.</returns>
    Task<ApiResult> UploadEntryImage(long entryId, ImageUploadRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an image by its ID.
    /// </summary>
    /// <param name="id">The ID of the image.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteEntryImage(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the status of the workshop.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The status of the workshop.</returns>
    Task<WorkshopStatus> GetWorkshopStatus(CancellationToken cancellationToken);

    /// <summary>
    /// Validates the status of the workshop.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A boolean indicating whether the workshop is reachable.</returns>
    Task<bool> ValidateWorkshopStatus(CancellationToken cancellationToken);

    /// <summary>
    /// Navigates to a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <param name="entryType">The type of the entry.</param>
    Task NavigateToEntry(long entryId, EntryType entryType);

    /// <summary>
    /// Installs a specific entry.
    /// </summary>
    /// <param name="entry">The entry to install.</param>
    /// <param name="release">The release of the entry.</param>
    /// <param name="progress">The progress of the installation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<EntryInstallResult> InstallEntry(IEntrySummary entry, IRelease release, Progress<StreamProgress> progress, CancellationToken cancellationToken);

    /// <summary>
    /// Uninstalls a specific entry.
    /// </summary>
    /// <param name="installedEntry">The installed entry to uninstall.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<EntryUninstallResult> UninstallEntry(InstalledEntry installedEntry, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all installed entries.
    /// </summary>
    /// <returns>A list of all installed entries.</returns>
    List<InstalledEntry> GetInstalledEntries();

    /// <summary>
    /// Gets a specific installed entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <returns>The installed entry.</returns>
    InstalledEntry? GetInstalledEntry(long entryId);

    /// <summary>
    /// Gets the installed plugin entry for a specific plugin.
    /// </summary>
    /// <param name="plugin">The plugin.</param>
    /// <returns>The installed entry.</returns>
    InstalledEntry? GetInstalledEntryByPlugin(Plugin plugin);

    /// <summary>
    /// Gets the installed plugin entry for a specific profile.
    /// </summary>
    /// <param name="profileConfiguration">The profile.</param>
    /// <returns>The installed entry.</returns>
    InstalledEntry? GetInstalledEntryByProfile(ProfileConfiguration profileConfiguration);

    /// <summary>
    /// Removes a specific installed entry for storage.
    /// </summary>
    /// <param name="installedEntry">The installed entry to remove.</param>
    void RemoveInstalledEntry(InstalledEntry installedEntry);

    /// <summary>
    /// Saves a specific installed entry to storage.
    /// </summary>
    /// <param name="entry">The installed entry to save.</param>
    void SaveInstalledEntry(InstalledEntry entry);

    /// <summary>
    /// Initializes the workshop service.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Represents the status of the workshop.
    /// </summary>
    public record WorkshopStatus(bool IsReachable, string Message);

    public event EventHandler<InstalledEntry>? OnInstalledEntrySaved;
    public event EventHandler<InstalledEntry>? OnEntryUninstalled;
    public event EventHandler<InstalledEntry>? OnEntryInstalled;
}