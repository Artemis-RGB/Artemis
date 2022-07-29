using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reflection;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard;

public class StartupWizardViewModel : DialogViewModelBase<bool>
{
    private readonly IRgbService _rgbService;
    private readonly ISettingsService _settingsService;
    private int _currentStep;
    private bool _showContinue;
    private bool _showGoBack;
    private bool _showFinish;

    public StartupWizardViewModel(ISettingsService settingsService, IRgbService rgbService, IPluginManagementService pluginManagementService, ISettingsVmFactory settingsVmFactory)
    {
        _settingsService = settingsService;
        _rgbService = rgbService;

        Continue = ReactiveCommand.Create(ExecuteContinue);
        GoBack = ReactiveCommand.Create(ExecuteGoBack);
        SkipOrFinishWizard = ReactiveCommand.Create(ExecuteSkipOrFinishWizard);
        SelectLayout = ReactiveCommand.Create<string>(ExecuteSelectLayout);

        AssemblyInformationalVersionAttribute? versionAttribute = typeof(StartupWizardViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        Version = $"Version {versionAttribute?.InformationalVersion} build {Constants.BuildInfo.BuildNumberDisplay}";

        // Take all compatible plugins that have an always-enabled device provider
        DeviceProviders = new ObservableCollection<PluginViewModel>(pluginManagementService.GetAllPlugins()
            .Where(p => p.Info.IsCompatible && p.Features.Any(f => f.AlwaysEnabled && f.FeatureType.IsAssignableTo(typeof(DeviceProvider))))
            .OrderBy(p => p.Info.Name)
            .Select(p => settingsVmFactory.PluginViewModel(p, null)));

        CurrentStep = 1;
        SetupButtons();
    }

    
    public ReactiveCommand<Unit, Unit> Continue { get; }
    public ReactiveCommand<Unit, Unit> GoBack { get; }
    public ReactiveCommand<Unit, Unit> SkipOrFinishWizard { get; }
    public ReactiveCommand<string, Unit> SelectLayout { get; }

    public string Version { get; }
    public ObservableCollection<PluginViewModel> DeviceProviders { get; }
    
    public PluginSetting<bool> UIAutoRun => _settingsService.GetSetting("UI.AutoRun", false);
    public PluginSetting<int> UIAutoRunDelay => _settingsService.GetSetting("UI.AutoRunDelay", 15);
    public PluginSetting<bool> UIShowOnStartup => _settingsService.GetSetting("UI.ShowOnStartup", true);
    public PluginSetting<bool> UICheckForUpdates => _settingsService.GetSetting("UI.CheckForUpdates", true);

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

        SetupButtons();
    }

    private void ExecuteContinue()
    {
        if (CurrentStep < 5)
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
}