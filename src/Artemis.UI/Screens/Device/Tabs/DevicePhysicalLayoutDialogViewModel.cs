using System;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Artemis.UI.Screens.Device;

public class DevicePhysicalLayoutDialogViewModel : ContentDialogViewModelBase
{
    public DevicePhysicalLayoutDialogViewModel(ArtemisDevice device)
    {
        Device = device;
        ApplyPhysicalLayout = ReactiveCommand.Create<string>(ExecuteApplyPhysicalLayout);
    }

    public ArtemisDevice Device { get; }
    public ReactiveCommand<string, Unit> ApplyPhysicalLayout { get; }
    public bool Applied { get; set; }

    public static async Task<bool> SelectPhysicalLayout(IWindowService windowService, ArtemisDevice device)
    {
        await windowService.CreateContentDialog()
            .WithTitle("Select physical layout")
            .WithViewModel(out DevicePhysicalLayoutDialogViewModel vm, device)
            .WithCloseButtonText("Cancel")
            .ShowAsync();

        return vm.Applied;
    }

    private void ExecuteApplyPhysicalLayout(string physicalLayout)
    {
        Device.PhysicalLayout = Enum.Parse<KeyboardLayoutType>(physicalLayout);
        Applied = true;
        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}