using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Plugins.Interfaces;
using Artemis.Plugins.Models;

namespace Artemis.Core.Services.Interfaces
{
    public interface IPluginService : IArtemisService, IDisposable
    {
        bool LoadingPlugins { get; }
        ReadOnlyCollection<PluginInfo> Plugins { get; }

        /// <summary>
        ///     Loads all installed plugins. If plugins already loaded this will reload them all
        /// </summary>
        /// <returns></returns>
        Task LoadPlugins();

        Task ReloadPlugin(PluginInfo pluginInfo);
        Task<IPluginViewModel> GetPluginViewModel(PluginInfo pluginInfo);

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
    }
}