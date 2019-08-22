using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Artemis.Core.Events;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

namespace Artemis.UI.ViewModels.Screens
{
    public class RootViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly ICollection<IScreenViewModel> _artemisViewModels;
        private readonly IPluginService _pluginService;

        public RootViewModel(ICollection<IScreenViewModel> artemisViewModels, IPluginService pluginService)
        {
            _artemisViewModels = artemisViewModels;
            _pluginService = pluginService;

            // Add the built-in items
            Items.AddRange(artemisViewModels);
            // Activate the home item
            ActiveItem = _artemisViewModels.First(v => v.GetType() == typeof(HomeViewModel));

            // Sync up with the plugin service
            Modules = new BindableCollection<Module>();
            // Modules.AddRange(_pluginService.GetPluginsOfType<Module>());

            _pluginService.PluginEnabled += PluginServiceOnPluginEnabled;
            _pluginService.PluginDisabled += PluginServiceOnPluginDisabled;
            PropertyChanged += OnSelectedModuleChanged;
            PropertyChanged += OnSelectedPageChanged;
        }

        public IObservableCollection<Module> Modules { get; set; }
        public bool MenuOpen { get; set; }
        public ListBoxItem SelectedPage { get; set; }
        public Module SelectedModule { get; set; }

        public async Task NavigateToSelectedModule()
        {
            if (SelectedModule == null)
                return;

            // Create a view model for the given plugin info (which will be a module)
            var viewModel = await Task.Run(() => SelectedModule.GetMainViewModel());
            // Tell Stylet to active the view model, the view manager will compile and show the XAML
            ActivateItem(viewModel);

            SelectedPage = null;
            MenuOpen = false;
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

            if (e.PluginInfo.Instance is Module module)
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
                case "Editor":
                    ActivateItem(_artemisViewModels.First(v => v.GetType() == typeof(EditorViewModel)));
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