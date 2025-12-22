using System.Reactive;
using ReactiveUI;
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
        _deviceService.AutoArrangeDevices(layout == "left");
        Wizard.ChangeScreen<SettingsStepViewModel>();
    }
}