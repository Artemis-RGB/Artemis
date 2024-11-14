using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Avalonia.Threading;
using Material.Icons;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins.Features;

public partial class PluginFeatureViewModel : ActivatableViewModelBase
{
    private readonly ICoreService _coreService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IWindowService _windowService;
    [Notify] private bool _enabling;

    public PluginFeatureViewModel(PluginFeatureInfo pluginFeatureInfo,
        bool showShield,
        ICoreService coreService,
        IWindowService windowService,
        IPluginManagementService pluginManagementService)
    {
        _coreService = coreService;
        _windowService = windowService;
        _pluginManagementService = pluginManagementService;

        FeatureInfo = pluginFeatureInfo;
        ShowShield = FeatureInfo.Plugin.Info.RequiresAdmin && showShield;

        ShowLogsFolder = ReactiveCommand.Create(ExecuteShowLogsFolder);
        ViewLoadException = ReactiveCommand.Create(ExecuteViewLoadException);
        InstallPrerequisites = ReactiveCommand.CreateFromTask(ExecuteInstallPrerequisites);
        RemovePrerequisites = ReactiveCommand.CreateFromTask(ExecuteRemovePrerequisites);

        this.WhenActivated(d =>
        {
            _pluginManagementService.PluginFeatureEnabling += OnFeatureEnabling;
            _pluginManagementService.PluginFeatureEnabled += OnFeatureEnableStopped;
            _pluginManagementService.PluginFeatureEnableFailed += OnFeatureEnableStopped;

            FeatureInfo.Plugin.Enabled += PluginOnToggled;
            FeatureInfo.Plugin.Disabled += PluginOnToggled;

            Disposable.Create(() =>
            {
                _pluginManagementService.PluginFeatureEnabling -= OnFeatureEnabling;
                _pluginManagementService.PluginFeatureEnabled -= OnFeatureEnableStopped;
                _pluginManagementService.PluginFeatureEnableFailed -= OnFeatureEnableStopped;

                FeatureInfo.Plugin.Enabled -= PluginOnToggled;
                FeatureInfo.Plugin.Disabled -= PluginOnToggled;
            }).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> ShowLogsFolder { get; }
    public ReactiveCommand<Unit, Unit> ViewLoadException { get; }
    public ReactiveCommand<Unit, Unit> InstallPrerequisites { get; }
    public ReactiveCommand<Unit, Unit> RemovePrerequisites { get; }

    public PluginFeatureInfo FeatureInfo { get; }
    public Exception? LoadException => FeatureInfo.LoadException;
    public bool ShowShield { get; }
    
    public bool IsEnabled
    {
        get => FeatureInfo.AlwaysEnabled || (FeatureInfo.Instance != null && FeatureInfo.Instance.IsEnabled);
        set => Dispatcher.UIThread.InvokeAsync(() => UpdateEnabled(value));
    }

    public bool CanToggleEnabled => FeatureInfo.Plugin.IsEnabled && !FeatureInfo.AlwaysEnabled;
    public bool CanInstallPrerequisites => FeatureInfo.PlatformPrerequisites.Any();
    public bool CanRemovePrerequisites => FeatureInfo.PlatformPrerequisites.Any(p => p.UninstallActions.Any());
    public bool IsPopupEnabled => CanInstallPrerequisites || CanRemovePrerequisites;

    public MaterialIconKind FeatureIcon
    {
        get
        {
            if (FeatureInfo.FeatureType.IsAssignableTo(typeof(DeviceProvider)))
                return MaterialIconKind.Devices;
            if (FeatureInfo.FeatureType.IsAssignableTo(typeof(Module)))
                return MaterialIconKind.VectorRectangle;
            if (FeatureInfo.FeatureType.IsAssignableTo(typeof(LayerBrushProvider)))
                return MaterialIconKind.Brush;
            if (FeatureInfo.FeatureType.IsAssignableTo(typeof(LayerEffectProvider)))
                return MaterialIconKind.AutoAwesome;
            return MaterialIconKind.Extension;
        }
    }
    
    public string FeatureType 
    {
        get
        {
            if (FeatureInfo.FeatureType.IsAssignableTo(typeof(DeviceProvider)))
                return "Device Provider";
            if (FeatureInfo.FeatureType.IsAssignableTo(typeof(Module)))
                return "Module";
            if (FeatureInfo.FeatureType.IsAssignableTo(typeof(LayerBrushProvider)))
                return "Layer Brush";
            if (FeatureInfo.FeatureType.IsAssignableTo(typeof(LayerEffectProvider)))
                return "Layer Effect";
            return "Miscellaneous feature";
        }
    }
    
    private void ExecuteShowLogsFolder()
    {
        try
        {
            Utilities.OpenFolder(Constants.LogsFolder);
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Welp, we couldn\'t open the logs folder for you", e);
        }
    }

    private void ExecuteViewLoadException()
    {
        if (LoadException != null)
            _windowService.ShowExceptionDialog("Feature failed to enable", LoadException);
    }

    private async Task ExecuteInstallPrerequisites()
    {
        if (FeatureInfo.PlatformPrerequisites.Any())
            await PluginPrerequisitesInstallDialogViewModel.Show(_windowService, new List<IPrerequisitesSubject> {FeatureInfo});
    }

    private async Task ExecuteRemovePrerequisites()
    {
        if (FeatureInfo.PlatformPrerequisites.Any(p => p.UninstallActions.Any()))
        {
            await PluginPrerequisitesUninstallDialogViewModel.Show(_windowService, new List<IPrerequisitesSubject> {FeatureInfo});
            this.RaisePropertyChanged(nameof(IsEnabled));
        }
    }

    private async Task UpdateEnabled(bool enable)
    {
        if (IsEnabled == enable)
        {
            this.RaisePropertyChanged(nameof(IsEnabled));
            return;
        }

        if (FeatureInfo.Instance == null)
        {
            this.RaisePropertyChanged(nameof(IsEnabled));
            await ShowFailureDialog($"Failed to enable '{FeatureInfo.Name}'", $"Feature '{FeatureInfo.Name}' is in a broken state and cannot enable.");
            return;
        }

        if (enable)
        {
            Enabling = true;

            try
            {
                if (FeatureInfo.Plugin.Info.RequiresAdmin && !_coreService.IsElevated)
                {
                    bool confirmed = await _windowService.ShowConfirmContentDialog("Enable feature", "The plugin of this feature requires admin rights, are you sure you want to enable it?");
                    if (!confirmed)
                    {
                        this.RaisePropertyChanged(nameof(IsEnabled));
                        return;
                    }
                }

                // Check if all prerequisites are met async
                if (!FeatureInfo.ArePrerequisitesMet())
                {
                    await PluginPrerequisitesInstallDialogViewModel.Show(_windowService, new List<IPrerequisitesSubject> {FeatureInfo});
                    if (!FeatureInfo.ArePrerequisitesMet())
                    {
                        this.RaisePropertyChanged(nameof(IsEnabled));
                        return;
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() => _pluginManagementService.EnablePluginFeature(FeatureInfo.Instance!, true));
            }
            catch (Exception e)
            {
                await ShowFailureDialog($"Failed to enable '{FeatureInfo.Name}'", e.Message);
            }
            finally
            {
                Enabling = false;
            }
        }
        else
        {
            _pluginManagementService.DisablePluginFeature(FeatureInfo.Instance, true);
            this.RaisePropertyChanged(nameof(IsEnabled));
        }
    }

    private void OnFeatureEnabling(object? sender, PluginFeatureEventArgs e)
    {
        if (e.PluginFeature != FeatureInfo.Instance) return;

        Enabling = true;
    }

    private void OnFeatureEnableStopped(object? sender, PluginFeatureEventArgs e)
    {
        if (e.PluginFeature != FeatureInfo.Instance) return;

        Enabling = false;

        this.RaisePropertyChanged(nameof(IsEnabled));
        this.RaisePropertyChanged(nameof(LoadException));
    }

    private void PluginOnToggled(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(CanToggleEnabled));
    }
    
    private async Task ShowFailureDialog(string action, string message)
    {
        ContentDialogBuilder builder = _windowService.CreateContentDialog()
            .WithTitle(action)
            .WithContent(message)
            .HavingPrimaryButton(b => b.WithText("View logs").WithCommand(ShowLogsFolder));
        // If available, add a secondary button pointing to the support page
        if (FeatureInfo.Plugin.Info.HelpPage != null)
            builder = builder.HavingSecondaryButton(b => b.WithText("Open support page").WithAction(() => Utilities.OpenUrl(FeatureInfo.Plugin.Info.HelpPage.ToString())));

        await builder.ShowAsync();
    }
}