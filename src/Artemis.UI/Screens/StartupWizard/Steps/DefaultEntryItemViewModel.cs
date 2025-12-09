using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Serilog;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public partial class DefaultEntryItemViewModel : ActivatableViewModelBase
{
    private readonly ILogger _logger;
    private readonly IWorkshopService _workshopService;
    private readonly IWindowService _windowService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly ISettingsVmFactory _settingsVmFactory;
    private readonly Progress<StreamProgress> _progress = new();

    [Notify] private bool _isInstalled;
    [Notify] private bool _shouldInstall;
    [Notify] private float _installProgress;
    
    public DefaultEntryItemViewModel(ILogger logger, IEntrySummary entry, IWorkshopService workshopService, IWindowService windowService, IPluginManagementService pluginManagementService,
        ISettingsVmFactory settingsVmFactory)
    {
        _logger = logger;
        _workshopService = workshopService;
        _windowService = windowService;
        _pluginManagementService = pluginManagementService;
        _settingsVmFactory = settingsVmFactory;
        Entry = entry;

        _progress.ProgressChanged += (_, f) => InstallProgress = f.ProgressPercentage;
        this.WhenActivated((CompositeDisposable _) => { IsInstalled = workshopService.GetInstalledEntry(entry.Id) != null; });
    }

    public IEntrySummary Entry { get; }

    public async Task<bool> InstallEntry(CancellationToken cancellationToken)
    {
        if (IsInstalled || !ShouldInstall || Entry.LatestRelease == null)
            return true;

        // Most entries install so fast it looks broken without a small delay
        Task minimumDelay = Task.Delay(100, cancellationToken);
        EntryInstallResult result = await _workshopService.InstallEntry(Entry, Entry.LatestRelease, _progress, cancellationToken);
        await minimumDelay;

        if (!result.IsSuccess)
        {
            await _windowService.CreateContentDialog().WithTitle("Failed to install entry")
                .WithContent($"Failed to install entry '{Entry.Name}' ({Entry.Id}): {result.Message}")
                .WithCloseButtonText("Skip and continue")
                .ShowAsync();
        }
        // If the entry is a plugin, enable the plugin and all features
        else if (result.Entry?.EntryType == EntryType.Plugin)
        {
            await EnablePluginAndFeatures(result.Entry);
        }

        return result.IsSuccess;
    }

    private async Task EnablePluginAndFeatures(InstalledEntry entry)
    {
        if (!entry.TryGetMetadata("PluginId", out Guid pluginId))
            throw new InvalidOperationException("Plugin entry does not contain a PluginId metadata value.");

        Plugin? plugin = _pluginManagementService.GetAllPlugins().FirstOrDefault(p => p.Guid == pluginId);
        if (plugin == null)
            throw new InvalidOperationException($"Plugin with id '{pluginId}' does not exist.");

        // There's quite a bit of UI involved in enabling a plugin, borrowing the PluginSettingsViewModel for this
        PluginViewModel pluginViewModel = _settingsVmFactory.PluginViewModel(plugin, ReactiveCommand.Create(() => { }));
        await pluginViewModel.UpdateEnabled(true);

        // Find features without prerequisites to enable
        foreach (PluginFeatureInfo pluginFeatureInfo in plugin.Features)
        {
            if (pluginFeatureInfo.Instance == null || pluginFeatureInfo.Instance.IsEnabled || pluginFeatureInfo.Prerequisites.Count != 0)
                continue;

            try
            {
                _pluginManagementService.EnablePluginFeature(pluginFeatureInfo.Instance, true);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to enable plugin feature '{FeatureName}', skipping", pluginFeatureInfo.Name);
            }
        }
        
        // If the plugin has a mandatory settings window, open it and wait
        if (plugin.ConfigurationDialog != null && plugin.ConfigurationDialog.IsMandatory)
        {
            if (plugin.Resolve(plugin.ConfigurationDialog.Type) is not PluginConfigurationViewModel viewModel)
                throw new ArtemisUIException($"The type of a plugin configuration dialog must inherit {nameof(PluginConfigurationViewModel)}");
            
            await _windowService.ShowDialogAsync(new PluginSettingsWindowViewModel(viewModel));
        }
    }
}