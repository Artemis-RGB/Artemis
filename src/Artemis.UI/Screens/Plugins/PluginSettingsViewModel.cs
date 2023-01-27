using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public class PluginSettingsViewModel : ActivatableViewModelBase
{
    private readonly INotificationService _notificationService;
    private readonly Plugin _plugin;

    private readonly IPluginManagementService _pluginManagementService;

    public PluginSettingsViewModel(Plugin plugin, ISettingsVmFactory settingsVmFactory, IPluginManagementService pluginManagementService, INotificationService notificationService)
    {
        _plugin = plugin;
        _pluginManagementService = pluginManagementService;
        _notificationService = notificationService;

        Reload = ReactiveCommand.CreateFromTask(ExecuteReload);
        PluginViewModel = settingsVmFactory.PluginViewModel(_plugin, Reload);
        PluginFeatures = new ObservableCollection<PluginFeatureViewModel>(_plugin.Features.Select(f => settingsVmFactory.PluginFeatureViewModel(f, false)));
    }

    public ReactiveCommand<Unit, Unit> Reload { get; }

    public PluginViewModel PluginViewModel { get; }

    public ObservableCollection<PluginFeatureViewModel> PluginFeatures { get; }

    private async Task ExecuteReload()
    {
        // Unloading the plugin will remove this viewmodel, this method is it's final act 😭
        bool wasEnabled = _plugin.IsEnabled;
        await Task.Run(() => _pluginManagementService.UnloadPlugin(_plugin));

        Plugin? plugin = _pluginManagementService.LoadPlugin(_plugin.Directory);
        if (plugin != null && wasEnabled)
        {
            await Task.Run(() => _pluginManagementService.EnablePlugin(plugin, true, true));
            _notificationService.CreateNotification().WithTitle("Reloaded plugin.").Show();
        }
        else if (plugin == null)
        {
            _notificationService.CreateNotification().WithTitle("Failed to load plugin after unloading it.").Show();
        }
    }
}