using System.Reactive;
using Artemis.UI.Shared;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public class SurfaceStepViewModel : WizardStepViewModel
{
    private readonly IDeviceService _deviceService;

    public SurfaceStepViewModel(IDeviceService deviceService)
    {
        _deviceService = deviceService;
        SelectLayout = ReactiveCommand.Create<string>(ExecuteSelectLayout);
        
        Continue = ReactiveCommand.Create(() => Wizard.ChangeScreen<SettingsStepViewModel>());
        GoBack = ReactiveCommand.Create(() => Wizard.ChangeScreen<LayoutsStepViewModel>());
    }

    public ReactiveCommand<string, Unit> SelectLayout { get; set; }

    private void ExecuteSelectLayout(string layout)
    {
        // TODO: Implement the layout
        _deviceService.AutoArrangeDevices();

        Wizard.ChangeScreen<SettingsStepViewModel>();
    }
}