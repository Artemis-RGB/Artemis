using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Artemis.Core.Events;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Home;
using Artemis.UI.Screens.News;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.Workshop;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf.Transitions;
using Stylet;

namespace Artemis.UI.Screens
{
    public class RootViewModel : Conductor<IScreen>
    {
        private readonly ICollection<IScreenViewModel> _artemisViewModels;
        private readonly IModuleViewModelFactory _moduleViewModelFactory;
        private readonly IPluginService _pluginService;

        public RootViewModel(ICollection<IScreenViewModel> artemisViewModels, IPluginService pluginService, IModuleViewModelFactory moduleViewModelFactory)
        {
            _artemisViewModels = artemisViewModels;
            _pluginService = pluginService;
            _moduleViewModelFactory = moduleViewModelFactory;

            // Activate the home item
            ActiveItem = _artemisViewModels.First(v => v.GetType() == typeof(HomeViewModel));
            ActiveItemReady = true;

            // Sync up with the plugin service
            Modules = new BindableCollection<Core.Plugins.Abstract.Module>();
            Modules.AddRange(_pluginService.GetPluginsOfType<Core.Plugins.Abstract.Module>());

            _pluginService.PluginEnabled += PluginServiceOnPluginEnabled;
            _pluginService.PluginDisabled += PluginServiceOnPluginDisabled;
            PropertyChanged += OnSelectedModuleChanged;
            PropertyChanged += OnSelectedPageChanged;
        }

        public IObservableCollection<Core.Plugins.Abstract.Module> Modules { get; set; }
        public bool MenuOpen { get; set; }
        public ListBoxItem SelectedPage { get; set; }
        public Core.Plugins.Abstract.Module SelectedModule { get; set; }
        public bool ActiveItemReady { get; set; }

        public async Task NavigateToSelectedModule()
        {
            if (SelectedModule == null)
                return;

            MenuOpen = false;
            SelectedPage = null;

            // Create a view model for the given plugin info (which will be a module)
            var viewModel = await Task.Run(() => _moduleViewModelFactory.CreateModuleViewModel(SelectedModule));
            ActivateItem(viewModel);
        }

        private void PluginServiceOnPluginEnabled(object sender, PluginEventArgs e)
        {
            var existing = Modules.FirstOrDefault(m => _pluginService.GetPluginInfo(m)?.Guid == e.PluginInfo.Guid);
            if (existing != null)
            {
                if (SelectedModule == existing && SelectedModule != null)
                    SelectedModule = null;
                Modules.Remove(existing);
            }

            if (e.PluginInfo.Instance is Core.Plugins.Abstract.Module module)
                Modules.Add(module);
        }

        private void PluginServiceOnPluginDisabled(object sender, PluginEventArgs e)
        {
            var existing = Modules.FirstOrDefault(m => _pluginService.GetPluginInfo(m)?.Guid == e.PluginInfo.Guid);
            if (existing != null)
            {
                if (SelectedModule == existing && SelectedModule != null)
                    SelectedModule = null;
                Modules.Remove(existing);
            }
        }

        private async void OnSelectedModuleChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedModule))
                await NavigateToSelectedModule();
        }

        private async void OnSelectedPageChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedPage" || SelectedPage == null)
                return;

            SelectedModule = null;
            MenuOpen = false;
            ActiveItemReady = false;

            // Let the menu close smoothly to avoid a sluggish feeling
            await Task.Delay(400);

            switch (SelectedPage.Name)
            {
                case "Home":
                    ActivateItem(_artemisViewModels.First(v => v is HomeViewModel));
                    break;
                case "News":
                    ActivateItem(_artemisViewModels.First(v => v is NewsViewModel));
                    break;
                case "Workshop":
                    ActivateItem(_artemisViewModels.First(v => v is WorkshopViewModel));
                    break;
                case "SurfaceEditor":
                    ActivateItem(_artemisViewModels.First(v => v is SurfaceEditorViewModel));
                    break;
                case "Settings":
                    ActivateItem(_artemisViewModels.First(v => v is SettingsViewModel));
                    break;
            }

            ActiveItemReady = true;
        }
    }
}