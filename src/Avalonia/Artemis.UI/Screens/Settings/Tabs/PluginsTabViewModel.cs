using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.Interfaces;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings.Tabs
{
    public class PluginsTabViewModel : ActivatableViewModelBase
    {
        private readonly INotificationService _notificationService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly ISettingsVmFactory _settingsVmFactory;
        private readonly IWindowService _windowService;
        private List<PluginSettingsViewModel>? _instances;
        private string? _searchPluginInput;

        public PluginsTabViewModel(IPluginManagementService pluginManagementService, INotificationService notificationService, IWindowService windowService, ISettingsVmFactory settingsVmFactory)
        {
            _pluginManagementService = pluginManagementService;
            _notificationService = notificationService;
            _windowService = windowService;
            _settingsVmFactory = settingsVmFactory;

            DisplayName = "Plugins";
            Plugins = new ObservableCollection<PluginSettingsViewModel>();

            this.WhenAnyValue(x => x.SearchPluginInput).Throttle(TimeSpan.FromMilliseconds(100)).Subscribe(SearchPlugins);
            this.WhenActivated((CompositeDisposable _) => GetPluginInstances());
        }

        public ObservableCollection<PluginSettingsViewModel> Plugins { get; }

        public string? SearchPluginInput
        {
            get => _searchPluginInput;
            set => this.RaiseAndSetIfChanged(ref _searchPluginInput, value);
        }

        public void OpenUrl(string url)
        {
            Utilities.OpenUrl(url);
        }

        public async Task ImportPlugin()
        {
            string[]? files = await _windowService.CreateOpenFileDialog().WithTitle("Import Artemis plugin").HavingFilter(f => f.WithExtension("zip").WithName("ZIP files")).ShowAsync();
            if (files == null)
                return;

            // Take the actual import off of the UI thread
            await Task.Run(() =>
            {
                Plugin plugin = _pluginManagementService.ImportPlugin(files[0]);

                GetPluginInstances();
                SearchPluginInput = plugin.Info.Name;

                // Enable it via the VM to enable the prerequisite dialog
                PluginSettingsViewModel pluginViewModel = Plugins.FirstOrDefault(i => i.Plugin == plugin);
                if (pluginViewModel is {IsEnabled: false})
                    pluginViewModel.IsEnabled = true;

                _notificationService.CreateNotification()
                    .WithTitle("Success")
                    .WithMessage($"Imported plugin: {plugin.Info.Name}")
                    .WithSeverity(NotificationSeverity.Success)
                    .Show();
            });
        }

        public void GetPluginInstances()
        {
            Plugins.Clear();
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _instances = _pluginManagementService.GetAllPlugins()
                    .Select(p => _settingsVmFactory.CreatePluginSettingsViewModel(p))
                    .OrderBy(i => i.Plugin.Info.Name)
                    .ToList();

                SearchPlugins(SearchPluginInput);
            }, DispatcherPriority.Background);
        }

        private void SearchPlugins(string? searchPluginInput)
        {
            if (_instances == null)
                return;

            List<PluginSettingsViewModel> instances = _instances;
            string? search = searchPluginInput?.ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                instances = instances.Where(i => i.Plugin.Info.Name.ToLower().Contains(search) ||
                                                 i.Plugin.Info.Description != null && i.Plugin.Info.Description.ToLower().Contains(search)).ToList();

            foreach (PluginSettingsViewModel pluginSettingsViewModel in instances)
            {
                if (!Plugins.Contains(pluginSettingsViewModel))
                    Plugins.Add(pluginSettingsViewModel);
            }

            foreach (PluginSettingsViewModel pluginSettingsViewModel in Plugins.ToList())
            {
                if (!instances.Contains(pluginSettingsViewModel))
                    Plugins.Remove(pluginSettingsViewModel);
            }

            Plugins.Sort(i => i.Plugin.Info.Name);
        }
    }
}