using System.Threading.Tasks;
using Artemis.Core;

namespace Artemis.UI.Services.Interfaces;

public interface IPluginInteractionService : IArtemisUIService
{
    /// <summary>
    /// Enables a plugin, showing prerequisites and config windows as necessary.
    /// </summary>
    /// <param name="plugin">The plugin to enable.</param>
    /// <param name="showMandatoryConfigWindow"></param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<bool> EnablePlugin(Plugin plugin, bool showMandatoryConfigWindow);

    /// <summary>
    /// Disables a plugin, stopping all its features and services.
    /// </summary>
    /// <param name="plugin">The plugin to disable.</param>
    /// <returns>A task representing the asynchronous operation with a boolean indicating success.</returns>
    Task<bool> DisablePlugin(Plugin plugin);

    /// <summary>
    /// Removes a plugin from the system, optionally running uninstall actions for prerequisites.
    /// </summary>
    /// <param name="plugin">The plugin to remove.</param>
    /// <returns>A task representing the asynchronous operation with a boolean indicating success.</returns>
    Task<bool> RemovePlugin(Plugin plugin);

    /// <summary>
    /// Removes all settings and configuration data for a plugin, temporarily disabling it during the process.
    /// </summary>
    /// <param name="plugin">The plugin whose settings should be cleared.</param>
    /// <returns>A task representing the asynchronous operation with a boolean indicating success.</returns>
    Task<bool> RemovePluginSettings(Plugin plugin);

    /// <summary>
    /// Removes the prerequisites for a plugin.
    /// </summary>
    /// <param name="plugin">The plugin whose prerequisites should be removed.</param>
    /// <param name="forPluginRemoval">Whether the prerequisites are being removed for a plugin removal.</param>
    /// <returns>A task representing the asynchronous operation with a boolean indicating success.</returns>
    Task<bool> RemovePluginPrerequisites(Plugin plugin, bool forPluginRemoval);
}