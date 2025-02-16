using System;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.StartupWizard.Steps;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using DryIoc;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.StartupWizard;

public partial class StartupWizardViewModel : DialogViewModelBase<bool>
{
    private readonly IContainer _container;
    private readonly ISettingsService _settingsService;
    [Notify] private WizardStepViewModel _screen;

    public StartupWizardViewModel(IContainer container, IWindowService windowService, ISettingsService settingsService)
    {
        _container = container;
        _settingsService = settingsService;
        _screen = _container.Resolve<WelcomeStepViewModel>();
        _screen.Wizard = this;

        WindowService = windowService;
        Version = $"Version {Constants.CurrentVersion}";
    }

    public IWindowService WindowService { get; }
    public string Version { get; }

    public void ChangeScreen<TWizardStepViewModel>() where TWizardStepViewModel : WizardStepViewModel
    {
        try
        {
            Screen = _container.Resolve<TWizardStepViewModel>();
            Screen.Wizard = this;
        }
        catch (Exception e)
        {
            WindowService.ShowExceptionDialog("Wizard screen failed to activate", e);
        }
    }

    public void SkipOrFinishWizard()
    {
        PluginSetting<bool> setting = _settingsService.GetSetting("UI.SetupWizardCompleted", false);
        setting.Value = true;
        setting.Save();
        
        Close(true);
    }
}