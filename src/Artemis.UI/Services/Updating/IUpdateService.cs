using System.Threading.Tasks;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Services.Updating;

public interface IUpdateService : IArtemisUIService
{
    Task<bool> CheckForUpdate();
    Task InstallRelease(string releaseId);
    string? CurrentVersion { get; }
}