using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.Providers;
using Artemis.Core.Services;
using Artemis.UI.Screens.StartupWizard;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Avalonia.Threading;
using DryIoc;
using DynamicData;
using FluentAvalonia.Interop;
using ReactiveUI;
using Serilog.Events;

namespace Artemis.UI.Screens.Settings;

public class GeneralTabViewModel : RoutableScreen
{
    private readonly IAutoRunProvider? _autoRunProvider;
    private readonly IProtocolProvider? _protocolProvider;
    private readonly IDebugService _debugService;
    private readonly PluginSetting<LayerBrushReference> _defaultLayerBrushDescriptor;
    private readonly INotificationService _notificationService;
    private readonly ISettingsService _settingsService;
    private readonly IUpdateService _updateService;
    private readonly IWindowService _windowService;
    private bool _startupWizardOpen;

    public GeneralTabViewModel(IContainer container,
        ISettingsService settingsService,
        IPluginManagementService pluginManagementService,
        IDebugService debugService,
        IWindowService windowService,
        IUpdateService updateService,
        INotificationService notificationService)
    {
        DisplayName = "General";
        _settingsService = settingsService;
        _debugService = debugService;
        _windowService = windowService;
        _updateService = updateService;
        _notificationService = notificationService;
        _autoRunProvider = container.Resolve<IAutoRunProvider>(IfUnresolved.ReturnDefault);
        _protocolProvider = container.Resolve<IProtocolProvider>(IfUnresolved.ReturnDefault);

        List<LayerBrushProvider> layerBrushProviders = pluginManagementService.GetFeaturesOfType<LayerBrushProvider>();
        List<IGraphicsContextProvider> graphicsContextProviders = container.Resolve<List<IGraphicsContextProvider>>();
        LayerBrushDescriptors = new ObservableCollection<LayerBrushDescriptor>(layerBrushProviders.SelectMany(l => l.LayerBrushDescriptors));
        GraphicsContexts = new ObservableCollection<string> {"Software"};
        GraphicsContexts.AddRange(graphicsContextProviders.Select(p => p.GraphicsContextName));

        _defaultLayerBrushDescriptor = _settingsService.GetSetting("ProfileEditor.DefaultLayerBrushDescriptor", new LayerBrushReference
        {
            LayerBrushProviderId = "Artemis.Plugins.LayerBrushes.Color.ColorBrushProvider-92a9d6ba",
            BrushType = "SolidBrush"
        });

        ShowLogs = ReactiveCommand.Create(ExecuteShowLogs);
        CheckForUpdate = ReactiveCommand.CreateFromTask(ExecuteCheckForUpdate);
        ShowSetupWizard = ReactiveCommand.CreateFromTask(ExecuteShowSetupWizard);
        ShowDebugger = ReactiveCommand.Create(ExecuteShowDebugger);
        ShowDataFolder = ReactiveCommand.Create(ExecuteShowDataFolder);

        this.WhenActivated(d =>
        {
            UIAutoRun.SettingChanged += UIAutoRunOnSettingChanged;
            UIUseProtocol.SettingChanged += UIUseProtocolOnSettingChanged;
            UIAutoRunDelay.SettingChanged += UIAutoRunDelayOnSettingChanged;
            EnableMica.SettingChanged += EnableMicaOnSettingChanged;

            Dispatcher.UIThread.InvokeAsync(ApplyAutoRun);
            Dispatcher.UIThread.Invoke(ApplyProtocolAssociation);
            Disposable.Create(() =>
            {
                UIAutoRun.SettingChanged -= UIAutoRunOnSettingChanged;
                UIUseProtocol.SettingChanged -= UIUseProtocolOnSettingChanged;
                UIAutoRunDelay.SettingChanged -= UIAutoRunDelayOnSettingChanged;
                EnableMica.SettingChanged -= EnableMicaOnSettingChanged;

                _settingsService.SaveAllSettings();
            }).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> ShowLogs { get; }
    public ReactiveCommand<Unit, Unit> CheckForUpdate { get; }
    public ReactiveCommand<Unit, Unit> ShowSetupWizard { get; }
    public ReactiveCommand<Unit, Unit> ShowDebugger { get; }
    public ReactiveCommand<Unit, Unit> ShowDataFolder { get; }

    public bool IsAutoRunSupported => _autoRunProvider != null;
    public bool IsWindows11 => OSVersionHelper.IsWindows11();
    public bool IsWindows => OSVersionHelper.IsWindows();

    public ObservableCollection<LayerBrushDescriptor> LayerBrushDescriptors { get; }
    public ObservableCollection<string> GraphicsContexts { get; }

    public ObservableCollection<RenderSettingViewModel> RenderScales { get; } = new()
    {
        new RenderSettingViewModel("25%", 0.25),
        new RenderSettingViewModel("50%", 0.5),
        new RenderSettingViewModel("100%", 1)
    };

    public ObservableCollection<RenderSettingViewModel> TargetFrameRates { get; } = new()
    {
        new RenderSettingViewModel("10 FPS", 10),
        new RenderSettingViewModel("20 FPS", 20),
        new RenderSettingViewModel("30 FPS", 30),
        new RenderSettingViewModel("45 FPS", 45),
        new RenderSettingViewModel("60 FPS (lol)", 60),
        new RenderSettingViewModel("144 FPS (omegalol)", 144)
    };

    public LayerBrushDescriptor? SelectedLayerBrushDescriptor
    {
        get => LayerBrushDescriptors.FirstOrDefault(d => d.MatchesLayerBrushReference(_defaultLayerBrushDescriptor.Value));
        set
        {
            if (value != null) _defaultLayerBrushDescriptor.Value = new LayerBrushReference(value);
        }
    }

    public RenderSettingViewModel? SelectedRenderScale
    {
        get => RenderScales.FirstOrDefault(s => Math.Abs(s.Value - CoreRenderScale.Value) < 0.01);
        set
        {
            if (value != null)
                CoreRenderScale.Value = value.Value;
        }
    }

    public RenderSettingViewModel? SelectedTargetFrameRate
    {
        get => TargetFrameRates.FirstOrDefault(s => Math.Abs(s.Value - CoreTargetFrameRate.Value) < 0.01);
        set
        {
            if (value != null)
                CoreTargetFrameRate.Value = (int) value.Value;
        }
    }

    public PluginSetting<bool> UIAutoRun => _settingsService.GetSetting("UI.AutoRun", false);
    public PluginSetting<bool> UIUseProtocol => _settingsService.GetSetting("UI.UseProtocol", true);
    public PluginSetting<int> UIAutoRunDelay => _settingsService.GetSetting("UI.AutoRunDelay", 15);
    public PluginSetting<bool> UIShowOnStartup => _settingsService.GetSetting("UI.ShowOnStartup", true);
    public PluginSetting<bool> EnableMica => _settingsService.GetSetting("UI.EnableMica", true);
    public PluginSetting<bool> UICheckForUpdates => _settingsService.GetSetting("UI.Updating.AutoCheck", true);
    public PluginSetting<bool> WorkshopShowNotifications => _settingsService.GetSetting("Workshop.ShowNotifications", true);
    public PluginSetting<bool> UIAutoUpdate => _settingsService.GetSetting("UI.Updating.AutoInstall", true);
    public PluginSetting<bool> ProfileEditorShowDataModelValues => _settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);
    public PluginSetting<LogEventLevel> CoreLoggingLevel => _settingsService.GetSetting("Core.LoggingLevel", LogEventLevel.Information);
    public PluginSetting<string> CorePreferredGraphicsContext => _settingsService.GetSetting("Core.PreferredGraphicsContext", "Software");
    public PluginSetting<double> CoreRenderScale => _settingsService.GetSetting("Core.RenderScale", 0.5);
    public PluginSetting<int> CoreTargetFrameRate => _settingsService.GetSetting("Core.TargetFrameRate", 30);
    public PluginSetting<bool> WebServerEnabled => _settingsService.GetSetting("WebServer.Enabled", true);
    public PluginSetting<bool> WebServerRemoteAccess => _settingsService.GetSetting("WebServer.RemoteAccess", false);
    public PluginSetting<int> WebServerPort => _settingsService.GetSetting("WebServer.Port", 9696);

    private void ExecuteShowLogs()
    {
        Utilities.OpenFolder(Constants.LogsFolder);
    }

    private async Task ExecuteCheckForUpdate(CancellationToken cancellationToken)
    {
        try
        {
            // If an update was available a popup was shown, no need to continue
            if (await _updateService.CheckForUpdate())
                return;

            _notificationService.CreateNotification()
                .WithTitle("No update available")
                .WithMessage("You are running the latest version in your current channel")
                .Show();
        }
        catch (Exception e)
        {
            _notificationService.CreateNotification()
                .WithTitle("Failed to check for update")
                .WithMessage(e.Message)
                .WithSeverity(NotificationSeverity.Warning)
                .Show();
        }
    }

    private async Task ExecuteShowSetupWizard()
    {
        _startupWizardOpen = true;
        await _windowService.ShowDialogAsync<StartupWizardViewModel, bool>();
        _startupWizardOpen = false;
    }

    private void ExecuteShowDebugger()
    {
        _debugService.ShowDebugger();
    }

    private void ExecuteShowDataFolder()
    {
        Utilities.OpenFolder(Constants.DataFolder);
    }

    private async Task ApplyAutoRun()
    {
        if (_autoRunProvider == null || _startupWizardOpen)
            return;

        try
        {
            if (UIAutoRun.Value)
                await _autoRunProvider.EnableAutoRun(false, UIAutoRunDelay.Value);
            else
                await _autoRunProvider.DisableAutoRun();
        }
        catch (Exception exception)
        {
            _windowService.ShowExceptionDialog("Failed to apply auto-run", exception);
        }
    }
    
    private void ApplyProtocolAssociation()
    {
        if (_protocolProvider == null)
            return;
        
        try
        {
            if (UIUseProtocol.Value)
                _protocolProvider.AssociateWithProtocol("artemis");
            else
                _protocolProvider.DisassociateWithProtocol("artemis");
        }
        catch (Exception exception)
        {
            _windowService.ShowExceptionDialog("Failed to apply protocol association", exception);
        }
    }

    private async void UIAutoRunOnSettingChanged(object? sender, EventArgs e)
    {
        await ApplyAutoRun();
    }
    
    private void UIUseProtocolOnSettingChanged(object? sender, EventArgs e)
    {
        ApplyProtocolAssociation();
    }

    private async void UIAutoRunDelayOnSettingChanged(object? sender, EventArgs e)
    {
        if (_autoRunProvider == null || !UIAutoRun.Value || _startupWizardOpen)
            return;

        try
        {
            await _autoRunProvider.EnableAutoRun(true, UIAutoRunDelay.Value);
        }
        catch (Exception exception)
        {
            _windowService.ShowExceptionDialog("Failed to apply auto-run", exception);
        }
    }

    private void EnableMicaOnSettingChanged(object? sender, EventArgs e)
    {
        Shared.UI.SetMicaEnabled(EnableMica.Value);
    }
}