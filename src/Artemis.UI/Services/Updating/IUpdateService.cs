using System;
using System.Threading.Tasks;
using Artemis.UI.Services.Interfaces;
using Artemis.WebClient.Updating;

namespace Artemis.UI.Services.Updating;

public interface IUpdateService : IArtemisUIService
{
    /// <summary>
    ///     Gets the current update channel.
    /// </summary>
    string Channel { get; }

    /// <summary>
    ///     Gets the version number of the previous release that was installed, if any.
    /// </summary>
    string? PreviousVersion { get; }

    /// <summary>
    ///     The latest cached release, can be updated by calling <see cref="CachedLatestRelease" />.
    /// </summary>
    IGetNextRelease_NextPublishedRelease? CachedLatestRelease { get; }

    /// <summary>
    ///     Asynchronously caches the latest release.
    /// </summary>
    Task CacheLatestRelease();

    /// <summary>
    ///     Asynchronously checks whether an update is available on the current <see cref="Channel" />.
    /// </summary>
    Task<bool> CheckForUpdate();

    /// <summary>
    ///     Creates a release installed for a release with the provided ID.
    /// </summary>
    /// <param name="releaseId">The ID of the release to create the installer for.</param>
    /// <returns>The resulting release installer.</returns>
    ReleaseInstaller GetReleaseInstaller(Guid releaseId);

    /// <summary>
    ///     Restarts the application to install a pending update.
    /// </summary>
    /// <param name="source">The source from which the restart is requested.</param>
    /// <param name="silent">A boolean indicating whether to perform a silent install of the update.</param>
    void RestartForUpdate(string source, bool silent);

    /// <summary>
    ///     Initializes the update service.
    /// </summary>
    /// <param name="performAutoUpdate"></param>
    /// <returns>A boolean indicating whether a restart will occur to install a pending update.</returns>
    bool Initialize(bool performAutoUpdate);
}