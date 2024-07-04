using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Screens.Plugins.Features;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Threading;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginManageViewModel : RoutableScreen<WorkshopDetailParameters>
{
    private readonly ISettingsVmFactory _settingsVmFactory;
    private readonly IRouter _router;
    private readonly IWorkshopService _workshopService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IWindowService _windowService;
    [Notify] private PluginViewModel? _pluginViewModel;
    [Notify] private ObservableCollection<PluginFeatureViewModel>? _pluginFeatures;

    public PluginManageViewModel(ISettingsVmFactory settingsVmFactory, IRouter router, IWorkshopService workshopService, IPluginManagementService pluginManagementService, IWindowService windowService)
    {
        _settingsVmFactory = settingsVmFactory;
        _router = router;
        _workshopService = workshopService;
        _pluginManagementService = pluginManagementService;
        _windowService = windowService;
        ParameterSource = ParameterSource.Route;
    }

    public async Task Close()
    {
        await _router.GoUp();
    }

    /// <inheritdoc />
    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        InstalledEntry? installedEntry = _workshopService.GetInstalledEntry(parameters.EntryId);
        if (installedEntry == null || !installedEntry.TryGetMetadata("PluginId", out Guid pluginId))
        {
            // TODO: Fix cancelling without this workaround, currently navigation is stopped but the page still opens
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await _windowService.ShowConfirmContentDialog("Invalid plugin", "The plugin you're trying to manage is invalid or doesn't exist", "Go back", null);
                await Close();
            });
            return;
        }

        Plugin? plugin = _pluginManagementService.GetAllPlugins().FirstOrDefault(p => p.Guid == pluginId);
        if (plugin == null)
        {
            // TODO: Fix cancelling without this workaround, currently navigation is stopped but the page still opens
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await _windowService.ShowConfirmContentDialog("Invalid plugin", "The plugin you're trying to manage is invalid or doesn't exist", "Go back", null);
                await Close();
            });
            return;
        }

        PluginViewModel = _settingsVmFactory.PluginViewModel(plugin, ReactiveCommand.Create(() => { }));
        PluginFeatures = new ObservableCollection<PluginFeatureViewModel>(plugin.Features.Select(f => _settingsVmFactory.PluginFeatureViewModel(f, false)));

        // If additional arguments were provided and it is a boolean, auto-enable the plugin
        if (args.Options.AdditionalArguments is true)
        {
            await PluginViewModel.AutoEnable();
        }
    }
}