using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Interfaces;
using Artemis.UI.ViewModels.Settings;
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

            PropertyChanged += OnSelectedModuleChanged;
            PropertyChanged += OnSelectedPageChanged;
        }


        public IObservableCollection<PluginInfo> Modules { get; set; }

        public bool MenuOpen { get; set; }
        public bool LoadingPlugins { get; set; }
        public ListBoxItem SelectedPage { get; set; }
        public PluginInfo SelectedModule { get; set; }

        private void PluginServiceOnStartedLoadingPlugins(object sender, EventArgs eventArgs)
        {
            LoadingPlugins = true;

            Modules.Clear();
            SelectedModule = null;
        }

        private void PluginServiceOnFinishedLoadedPlugins(object sender, EventArgs eventArgs)
        {
            Modules.AddRange(_pluginService.Plugins.Where(p => p.Plugin is IModule));
            SelectedModule = null;

            LoadingPlugins = false;
        }

        public async Task NavigateToSelectedModule()
        {
            if (SelectedModule == null || LoadingPlugins)
                return;

            // Create a view model for the given plugin info (which will be a module)
            var viewModel = await _pluginService.GetModuleViewModel(SelectedModule);
            // Tell Stylet to active the view model, the view manager will compile and show the XAML
            ActivateItem(viewModel);

            SelectedPage = null;
            MenuOpen = false;
        }

        private async void OnSelectedModuleChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedModule")
                await NavigateToSelectedModule();
        }

        private void OnSelectedPageChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedPage" || SelectedPage == null)
                return;

            switch (SelectedPage.Name)
            {
                case "Home":
                    ActivateItem(_artemisViewModels.First(v => v.GetType() == typeof(HomeViewModel)));
                    break;
                case "News":
                    // ActivateItem(_artemisViewModels.First(v => v.GetType() == typeof(NewsViewModel)));
                    break;
                case "Workshop":
                    // ActivateItem(_artemisViewModels.First(v => v.GetType() == typeof(WorkshopViewModel)));
                    break;
                case "Settings":
                    ActivateItem(_artemisViewModels.First(v => v.GetType() == typeof(SettingsViewModel)));
                    break;
            }

            SelectedModule = null;
            MenuOpen = false;
        }
    }
}