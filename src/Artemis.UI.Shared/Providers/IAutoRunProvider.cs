using System.Threading.Tasks;

namespace Artemis.UI.Shared.Providers;

/// <summary>
///     Represents a provider for custom cursors.
/// </summary>
public interface IAutoRunProvider
{
    /// <summary>
    ///     Asynchronously enables auto-run.
    /// </summary>
    /// <param name="recreate">A boolean indicating whether the auto-run configuration should be recreated (the auto run delay changed)</param>
    /// <param name="autoRunDelay">The delay in seconds before the application should start (if supported)</param>
    Task EnableAutoRun(bool recreate, int autoRunDelay);

    /// <summary>
    ///     Asynchronously disables auto-run.
    /// </summary>
    Task DisableAutoRun();
}