using System.Threading.Tasks;

namespace Artemis.UI.Services.Interfaces;

public interface IUpdateService : IArtemisUIService
{
    /// <summary>
    ///     Gets a boolean indicating whether updating is supported.
    /// </summary>
    bool UpdatingSupported { get; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether auto-updating is suspended.
    /// </summary>
    bool SuspendAutoUpdate { get; set; }

    /// <summary>
    ///     Manually checks for updates and offers to install it if found.
    /// </summary>
    /// <returns>Whether an update was found, regardless of whether the user chose to install it.</returns>
    Task ManualUpdate();
}