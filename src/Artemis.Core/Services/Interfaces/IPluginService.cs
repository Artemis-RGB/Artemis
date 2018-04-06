using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;
using Artemis.Core.ProfileElements;

namespace Artemis.Core.Services.Interfaces
{
    public interface IPluginService : IArtemisService, IDisposable
    {
        /// <summary>
        ///     Indicates wether or not plugins are currently being loaded
        /// </summary>
        bool LoadingPlugins { get; }

        /// <summary>
        ///     All loaded plugins
        /// </summary>
        ReadOnlyCollection<PluginInfo> Plugins { get; }

        /// <summary>
        ///     Loads all installed plugins. If plugins already loaded this will reload them all
        /// </summary>
        Task LoadPlugins();

        /// <summary>
        ///     Reloads the plugin accompanying the provided plugin info
        /// </summary>
        /// <param name="pluginInfo">The plugin info containing the plugin to reload</param>
        Task ReloadPlugin(PluginInfo pluginInfo);

        /// <summary>
        ///     Gets the view model of the module accompanying the provided plugin info
        /// </summary>
        /// <param name="pluginInfo">The plugin info containing the module for which to load the view model</param>
        Task<IModuleViewModel> GetModuleViewModel(PluginInfo pluginInfo);

        /// <summary>
        ///     Occurs when a single plugin has loaded
        /// </summary>
        event EventHandler<PluginEventArgs> PluginLoaded;

        /// <summary>
        ///     Occurs when a single plugin has reloaded
        /// </summary>
        event EventHandler<PluginEventArgs> PluginReloaded;

        /// <summary>
        ///     Occurs when loading all plugins has started
        /// </summary>
        event EventHandler StartedLoadingPlugins;

        /// <summary>
        ///     Occurs when loading all plugins has finished
        /// </summary>
        event EventHandler FinishedLoadedPlugins;

        /// <summary>
        ///     If found, returns an instance of the layer type matching the given GUID
        /// </summary>
        /// <param name="layerTypeGuid">The GUID of the layer type to find</param>
        /// <returns>An instance of the layer type</returns>
        ILayerType GetLayerTypeByGuid(Guid layerTypeGuid);
    }
}