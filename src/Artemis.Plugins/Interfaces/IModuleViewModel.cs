using Artemis.Plugins.Models;
using Stylet;

namespace Artemis.Plugins.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to create a view model for a module
    /// </summary>
    public interface IModuleViewModel : IScreen
    {
        PluginInfo PluginInfo { get; set; }
    }
}