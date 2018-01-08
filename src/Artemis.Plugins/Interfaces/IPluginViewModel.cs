using Artemis.Plugins.Models;
using Stylet;

namespace Artemis.Plugins.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to create a view model for a plugin
    /// </summary>
    public interface IPluginViewModel : IScreen
    {
        PluginInfo PluginInfo { get; set; }
    }
}