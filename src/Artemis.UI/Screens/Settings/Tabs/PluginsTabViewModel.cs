using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings
{
    public class PluginsTabViewModel : ActivatableViewModelBase
    {
        private readonly INotificationService _notificationService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IWindowService _windowService;
        private string? _searchPluginInput;

        public PluginsTabViewModel(IPluginManagementService pluginManagementService, INotificationService notificationService, IWindowService windowService, ISettingsVmFactory settingsVmFactory)
        {
            _pluginManagementService = pluginManagementService;
            _notificationService = notificationService;
            _windowService = windowService;

            DisplayName = "Plugins";

            SourceList<Plugin> plugins = new();
            IObservable<Func<Plugin, bool>> pluginFilter = this.WhenAnyValue(vm => vm.SearchPluginInput).Throttle(TimeSpan.FromMilliseconds(100)).Select(CreatePredicate);

            plugins.Connect()
                .Filter(pluginFilter)
                .Sort(SortExpressionComparer<Plugin>.Ascending(p => p.Info.Name))
                .TransformAsync(p => Dispatcher.UIThread.InvokeAsync(() => settingsVmFactory.PluginSettingsViewModel(p), DispatcherPriority.Background))
                .Bind(out ReadOnlyObservableCollection<PluginSettingsViewModel> pluginViewModels)
                .Subscribe();
            Plugins = pluginViewModels;

            this.WhenActivated(d =>
            {
                plugins.AddRange(_pluginManagementService.GetAllPlugins());
                Observable.FromEventPattern<PluginEventArgs>(x => _pluginManagementService.PluginLoaded += x, x => _pluginManagementService.PluginLoaded -= x)
                    .Subscribe(a => plugins.Add(a.EventArgs.Plugin))
                    .DisposeWith(d);
                Observable.FromEventPattern<PluginEventArgs>(x => _pluginManagementService.PluginUnloaded += x, x => _pluginManagementService.PluginUnloaded -= x)
                    .Subscribe(a => plugins.Remove(a.EventArgs.Plugin))
                    .DisposeWith(d);
                Disposable.Create(() => plugins.Clear()).DisposeWith(d);
            });
            ImportPlugin = ReactiveCommand.CreateFromTask(ExecuteImportPlugin);
        }

        public ReadOnlyObservableCollection<PluginSettingsViewModel> Plugins { get; }
        public ReactiveCommand<Unit, Unit> ImportPlugin { get; }

        public string? SearchPluginInput
        {
            get => _searchPluginInput;
            set => RaiseAndSetIfChanged(ref _searchPluginInput, value);
        }

        public void OpenUrl(string url)
        {
            Utilities.OpenUrl(url);
        }

        private async Task ExecuteImportPlugin()
        {
            string[]? files = await _windowService.CreateOpenFileDialog().WithTitle("Import Artemis plugin").HavingFilter(f => f.WithExtension("zip").WithName("ZIP files")).ShowAsync();
            if (files == null)
                return;

            try
            {
                Plugin plugin = _pluginManagementService.ImportPlugin(files[0]);
                SearchPluginInput = plugin.Info.Name;

                // Wait for the VM to be created asynchronously (it would be better to respond to some event here)
                await Task.Delay(200);
                // Enable it via the VM to enable the prerequisite dialog
                PluginSettingsViewModel? settingsViewModel = Plugins.FirstOrDefault(i => i.PluginViewModel.Plugin == plugin);
                if (settingsViewModel != null && !settingsViewModel.PluginViewModel.IsEnabled)
                    await settingsViewModel.PluginViewModel.UpdateEnabled(true);

                _notificationService.CreateNotification()
                    .WithTitle("Plugin imported")
                    .WithMessage($"Added the '{plugin.Info.Name}' plugin")
                    .WithSeverity(NotificationSeverity.Success)
                    .Show();
            }
            catch (Exception e)
            {
                await _windowService.ShowConfirmContentDialog("Failed to import plugin", "Make sure the selected ZIP file is a valid Artemis plugin.\r\n" + e.Message, "Close", null);
            }
        }

        private Func<Plugin, bool> CreatePredicate(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return _ => true;

            return data => data.Info.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase) ||
                           (data.Info.Description != null && data.Info.Description.Contains(text, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}