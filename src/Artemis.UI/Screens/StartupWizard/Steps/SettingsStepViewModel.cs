using System;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Services;
using DryIoc;
using ReactiveUI;


namespace Artemis.UI.Screens.StartupWizard.Steps;

public class SettingsStepViewModel : WizardStepViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IWindowService _windowService;
    private readonly IAutoRunProvider? _autoRunProvider;
    private readonly IProtocolProvider? _protocolProvider;

    public SettingsStepViewModel(IContainer container, ISettingsService settingsService, IDeviceService deviceService, IWindowService windowService)
    {
        _settingsService = settingsService;
        _windowService = windowService;
        _autoRunProvider = container.Resolve<IAutoRunProvider>(IfUnresolved.ReturnDefault);
        _protocolProvider = container.Resolve<IProtocolProvider>(IfUnresolved.ReturnDefault);

        Continue = ReactiveCommand.Create(() => Wizard.ChangeScreen<FinishStepViewModel>());
        GoBack = ReactiveCommand.Create(() =>
        {
            // Without devices, skip to the default entries screen
            if (deviceService.EnabledDevices.Count == 0)
                Wizard.ChangeScreen<DefaultEntriesStepViewModel>();
            else
                Wizard.ChangeScreen<SurfaceStepViewModel>();
        });

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

    public PluginSetting<bool> UIAutoRun => _settingsService.GetSetting("UI.AutoRun", false);
    public PluginSetting<bool> UIUseProtocol => _settingsService.GetSetting("UI.UseProtocol", true);
    public PluginSetting<int> UIAutoRunDelay => _settingsService.GetSetting("UI.AutoRunDelay", 15);
    public PluginSetting<bool> UIShowOnStartup => _settingsService.GetSetting("UI.ShowOnStartup", true);
    public PluginSetting<bool> UICheckForUpdates => _settingsService.GetSetting("UI.Updating.AutoCheck", true);
    public PluginSetting<bool> UIAutoUpdate => _settingsService.GetSetting("UI.Updating.AutoInstall", true);
    public bool IsAutoRunSupported => _autoRunProvider != null;

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