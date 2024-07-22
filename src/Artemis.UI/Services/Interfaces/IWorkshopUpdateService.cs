using System.Threading.Tasks;
using Artemis.WebClient.Workshop.Models;

namespace Artemis.UI.Services.Interfaces;

public interface IWorkshopUpdateService : IArtemisUIService
{
    /// <summary>
    /// Automatically updates all installed entries that have auto-update enabled and have a new version available.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task AutoUpdateEntries();
    
    /// <summary>
    /// Automatically updates the provided entry if a new version is available.
    /// </summary>
    /// <param name="entry">The entry to update.</param>
    /// <returns>A task of <see langword="true"/> if the entry was updated, <see langword="false"/> otherwise.</returns>
    Task<bool> AutoUpdateEntry(InstalledEntry entry);

    /// <summary>
    /// Disable workshop update notifications.
    /// </summary>
    void DisableNotifications();
}