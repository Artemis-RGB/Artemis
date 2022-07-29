using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public class PluginSettingsViewModel : ActivatableViewModelBase
{
    private Plugin _plugin;
    
    private readonly ISettingsVmFactory _settingsVmFactory;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly INotificationService _notificationService;
    
    private PluginViewModel _pluginViewModel;

    public PluginSettingsViewModel(Plugin plugin, ISettingsVmFactory settingsVmFactory, IPluginManagementService pluginManagementService, INotificationService notificationService)
    {
        _plugin = plugin;
        _settingsVmFactory = settingsVmFactory;
        _pluginManagementService = pluginManagementService;
        _notificationService = notificationService;

        Reload = ReactiveCommand.CreateFromTask(ExecuteReload);
        
        PluginViewModel = settingsVmFactory.PluginViewModel(_plugin, Reload);
        PluginFeatures = new ObservableCollection<PluginFeatureViewModel>();
        foreach (PluginFeatureInfo pluginFeatureInfo in _plugin.Features)
            PluginFeatures.Add(settingsVmFactory.PluginFeatureViewModel(pluginFeatureInfo, false));
    }

    public ReactiveCommand<Unit, Unit> Reload { get; }

    public PluginViewModel PluginViewModel
    {
        get => _pluginViewModel;
        private set => RaiseAndSetIfChanged(ref _pluginViewModel, value);
    }

    public ObservableCollection<PluginFeatureViewModel> PluginFeatures { get; }

    private async Task ExecuteReload()
    {
        bool wasEnabled = _plugin.IsEnabled;
        await Task.Run(() => _pluginManagementService.UnloadPlugin(_plugin));

        PluginFeatures.Clear();
        _plugin = _pluginManagementService.LoadPlugin(_plugin.Directory);
        
        PluginViewModel = _settingsVmFactory.PluginViewModel(_plugin, Reload);
        foreach (PluginFeatureInfo pluginFeatureInfo in _plugin.Features)
            PluginFeatures.Add(_settingsVmFactory.PluginFeatureViewModel(pluginFeatureInfo, false));

        if (wasEnabled)
            await PluginViewModel.UpdateEnabled(true);

        _notificationService.CreateNotification().WithTitle("Reloaded plugin.").Show();
    }
}