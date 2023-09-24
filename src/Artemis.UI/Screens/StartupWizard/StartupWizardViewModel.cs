using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Services;
using DryIoc;
using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard;

public class StartupWizardViewModel : DialogViewModelBase<bool>
{
    private readonly IAutoRunProvider? _autoRunProvider;
    private readonly IProtocolProvider? _protocolProvider;
    private readonly IRgbService _rgbService;
    private readonly ISettingsService _settingsService;
    private readonly IWindowService _windowService;
    private int _currentStep;
    private bool _showContinue;
    private bool _showFinish;
    private bool _showGoBack;

    public StartupWizardViewModel(IContainer container,
        ISettingsService settingsService,
        IRgbService rgbService,
        IPluginManagementService pluginManagementService,
        IWindowService windowService,
        ISettingsVmFactory settingsVmFactory)
    {
        _settingsService = settingsService;
        _rgbService = rgbService;
        _windowService = windowService;
        _autoRunProvider = container.Resolve<IAutoRunProvider>(IfUnresolved.ReturnDefault);
        _protocolProvider = container.Resolve<IProtocolProvider>(IfUnresolved.ReturnDefault);

        Continue = ReactiveCommand.Create(ExecuteContinue);
        GoBack = ReactiveCommand.Create(ExecuteGoBack);
        SkipOrFinishWizard = ReactiveCommand.Create(ExecuteSkipOrFinishWizard);
        SelectLayout = ReactiveCommand.Create<string>(ExecuteSelectLayout);
        Version = $"Version {Constants.CurrentVersion}";

        // Take all compatible plugins that have an always-enabled device provider
        DeviceProviders = new ObservableCollection<PluginViewModel>(pluginManagementService.GetAllPlugins()
            .Where(p => p.Info.IsCompatible && p.Features.Any(f => f.AlwaysEnabled && f.FeatureType.IsAssignableTo(typeof(DeviceProvider))))
            .OrderBy(p => p.Info.Name)
            .Select(p => settingsVmFactory.PluginViewModel(p, ReactiveCommand.Create(() => new Unit()))));

        CurrentStep = 1;
        SetupButtons();

        this.WhenActivated(d =>
        {
            UIAutoRun.SettingChanged += UIAutoRunOnSettingChanged;
            UIUseProtocol.SettingChanged += UIUseProtocolOnSettingChanged;
            UIAutoRunDelay.SettingChanged += UIAutoRunDelayOnSettingChanged;

            Disposable.Create(() =>
            {
                UIAutoRun.SettingChanged -= UIAutoRunOnSettingChanged;
                UIUseProtocol.SettingChanged -= UIUseProtocolOnSettingChanged;
                UIAutoRunDelay.SettingChanged -= UIAutoRunDelayOnSettingChanged;

                _settingsService.SaveAllSettings();
            }).DisposeWith(d);
        });
    }

    public ReactiveCommand<Unit, Unit> Continue { get; }
    public ReactiveCommand<Unit, Unit> GoBack { get; }
    public ReactiveCommand<Unit, Unit> SkipOrFinishWizard { get; }
    public ReactiveCommand<string, Unit> SelectLayout { get; }

    public string Version { get; }
    public ObservableCollection<PluginViewModel> DeviceProviders { get; }

    public bool IsAutoRunSupported => _autoRunProvider != null;

    public PluginSetting<bool> UIAutoRun => _settingsService.GetSetting("UI.AutoRun", false);
    public PluginSetting<bool> UIUseProtocol => _settingsService.GetSetting("UI.UseProtocol", true);
    public PluginSetting<int> UIAutoRunDelay => _settingsService.GetSetting("UI.AutoRunDelay", 15);
    public PluginSetting<bool> UIShowOnStartup => _settingsService.GetSetting("UI.ShowOnStartup", true);
    public PluginSetting<bool> UICheckForUpdates => _settingsService.GetSetting("UI.Updating.AutoCheck", true);
    public PluginSetting<bool> UIAutoUpdate => _settingsService.GetSetting("UI.Updating.AutoInstall", true);

    public int CurrentStep
    {
        get => _currentStep;
        set => RaiseAndSetIfChanged(ref _currentStep, value);
    }

    public bool ShowContinue
    {
        get => _showContinue;
        set => RaiseAndSetIfChanged(ref _showContinue, value);
    }

    public bool ShowGoBack
    {
        get => _showGoBack;
        set => RaiseAndSetIfChanged(ref _showGoBack, value);
    }

    public bool ShowFinish
    {
        get => _showFinish;
        set => RaiseAndSetIfChanged(ref _showFinish, value);
    }

    private void ExecuteGoBack()
    {
        if (CurrentStep > 1)
            CurrentStep--;

        // Skip the settings step if none of it's contents are supported
        if (CurrentStep == 4 && !IsAutoRunSupported)
            CurrentStep--;

        SetupButtons();
    }

    private void ExecuteContinue()
    {
        if (CurrentStep < 5)
            CurrentStep++;

        // Skip the settings step if none of it's contents are supported
        if (CurrentStep == 4 && !IsAutoRunSupported)
            CurrentStep++;

        SetupButtons();
    }

    private void SetupButtons()
    {
        ShowContinue = CurrentStep != 3 && CurrentStep < 5;
        ShowGoBack = CurrentStep > 1;
        ShowFinish = CurrentStep == 5;
    }

    private void ExecuteSkipOrFinishWizard()
    {
        PluginSetting<bool> setting = _settingsService.GetSetting("UI.SetupWizardCompleted", false);
        setting.Value = true;
        setting.Save();

        Close(true);
    }


    private void ExecuteSelectLayout(string layout)
    {
        // TODO: Implement the layout
        _rgbService.AutoArrangeDevices();

        ExecuteContinue();
    }

    private async Task ApplyAutoRun()
    {
        if (_autoRunProvider == null)
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
        if (_autoRunProvider == null || !UIAutoRun.Value)
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
}