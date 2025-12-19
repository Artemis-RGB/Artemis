using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Plugins.Features;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public class PluginSettingsViewModel : ActivatableViewModelBase
{
    private readonly INotificationService _notificationService;
    private readonly IWindowService _windowService;
    private readonly IPluginManagementService _pluginManagementService;

    public PluginSettingsViewModel(PluginInfo pluginInfo, ISettingsVmFactory settingsVmFactory, IPluginManagementService pluginManagementService, INotificationService notificationService,
        IWindowService windowService)
    {
        _pluginManagementService = pluginManagementService;
        _notificationService = notificationService;
        _windowService = windowService;

        PluginInfo = pluginInfo;
        Reload = ReactiveCommand.CreateFromTask(ExecuteReload);
        PluginViewModel = settingsVmFactory.PluginViewModel(PluginInfo, Reload);
        PluginFeatures = new ObservableCollection<PluginFeatureViewModel>(PluginInfo.Plugin?.Features.Select(f => settingsVmFactory.PluginFeatureViewModel(f, false)) ?? []);
    }

    public PluginInfo PluginInfo { get; }
    public ReactiveCommand<Unit, Unit> Reload { get; }

    public PluginViewModel PluginViewModel { get; }

    public ObservableCollection<PluginFeatureViewModel> PluginFeatures { get; }

    public void ViewLoadException()
    {
        if (PluginInfo.LoadException != null)
            _windowService.ShowExceptionDialog("Plugin failed to load", PluginInfo.LoadException);
    }

    private async Task ExecuteReload()
    {
        if (PluginInfo.Plugin == null)
            return;

        // Unloading the plugin will remove this viewmodel, this method is it's final act 😭
        bool wasEnabled = PluginInfo.Plugin.IsEnabled;
        await Task.Run(() => _pluginManagementService.UnloadPlugin(PluginInfo.Plugin));

        Plugin? plugin = _pluginManagementService.LoadPlugin(PluginInfo.Plugin.Directory);
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