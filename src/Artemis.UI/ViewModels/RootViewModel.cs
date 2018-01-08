using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Interfaces;
using Artemis.Plugins.Models;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

namespace Artemis.UI.ViewModels
{
    public class RootViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly ICollection<IArtemisViewModel> _artemisViewModels;
        private readonly IPluginService _pluginService;

        public RootViewModel(ICollection<IArtemisViewModel> artemisViewModels, IPluginService pluginService)
        {
            _artemisViewModels = artemisViewModels;
            _pluginService = pluginService;
            
            // Add the built-in items
            Items.AddRange(artemisViewModels);
            // Activate the home item
            ActiveItem = _artemisViewModels.First(v => v.GetType() == typeof(HomeViewModel));

            // Sync up with the plugin service
            Modules = new BindableCollection<PluginInfo>();
            LoadingPlugins = _pluginService.LoadingPlugins;
            _pluginService.StartedLoadingPlugins += PluginServiceOnStartedLoadingPlugins;
            _pluginService.FinishedLoadedPlugins += PluginServiceOnFinishedLoadedPlugins;

            if (!LoadingPlugins)
                Modules.AddRange(_pluginService.Plugins.Where(p => p.Plugin is IModule));
        }

        public IObservableCollection<PluginInfo> Modules { get; set; }

        public bool MenuOpen { get; set; }
        public bool LoadingPlugins { get; set; }

        private void PluginServiceOnStartedLoadingPlugins(object sender, EventArgs eventArgs)
        {
            LoadingPlugins = true;
            Modules.Clear();
        }

        private void PluginServiceOnFinishedLoadedPlugins(object sender, EventArgs eventArgs)
        {
            LoadingPlugins = false;
            Modules.AddRange(_pluginService.Plugins.Where(p => p.Plugin is IModule));
            NavigateToModule(Modules.First());
        }

        public void NavigateToHome()
        {
            ActivateItem(_artemisViewModels.First(v => v.GetType() == typeof(HomeViewModel)));
            MenuOpen = false;
        }

        public void NavigateToNews()
        {
        }

        public void NavigateToWorkshop()
        {
        }

        public void NavigateToSettings()
        {
            ActivateItem(_artemisViewModels.First(v => v.GetType() == typeof(SettingsViewModel)));
            MenuOpen = false;
        }

        public async void NavigateToModule(PluginInfo pluginInfo)
        {
            // Create a view model for the given plugin info (which will be a module)
            var viewModel = await _pluginService.GetPluginViewModel(pluginInfo);
            // Tell Stylet to active the view model, the view manager will compile and show the XAML
            ActivateItem(viewModel);
        }
    }
}