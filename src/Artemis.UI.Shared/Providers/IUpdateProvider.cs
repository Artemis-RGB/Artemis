using System.Threading.Tasks;

namespace Artemis.UI.Shared.Providers;

/// <summary>
///     Represents a provider for custom cursors.
/// </summary>
public interface IUpdateProvider
{
    /// <summary>
    ///     Asynchronously checks whether an update is available.
    /// </summary>
    /// <param name="channel">The channel to use when checking updates (i.e. master or development)</param>
    /// <returns>A task returning <see langword="true" /> if an update is available; otherwise <see langword="false" />.</returns>
    Task<bool> CheckForUpdate(string channel);

    /// <summary>
    ///     Applies any available updates.
    /// </summary>
    /// <param name="channel">The channel to use when checking updates (i.e. master or development)</param>
    /// <param name="silent">Whether or not to update silently.</param>
    Task ApplyUpdate(string channel, bool silent);

    /// <summary>
    ///     Offer to install the update to the user.
    /// </summary>
    /// <param name="channel">The channel to use when checking updates (i.e. master or development)</param>
    /// <param name="windowOpen">A boolean indicating whether the main window is open.</param>
    /// <returns>A task returning <see langword="true" /> if the user chose to update; otherwise <see langword="false" />.</returns>
    Task OfferUpdate(string channel, bool windowOpen);
}