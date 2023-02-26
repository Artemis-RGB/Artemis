using System.Threading.Tasks;
using Artemis.UI.Services.Interfaces;
using Artemis.WebClient.Updating;

namespace Artemis.UI.Services.Updating;

public interface IUpdateService : IArtemisUIService
{
    string? CurrentVersion { get; }
    IGetNextRelease_NextPublishedRelease? CachedLatestRelease { get; }

    Task CacheLatestRelease();
    Task<bool> CheckForUpdate();
    void QueueUpdate();

    ReleaseInstaller GetReleaseInstaller(string releaseId);
    void RestartForUpdate(bool silent);
}