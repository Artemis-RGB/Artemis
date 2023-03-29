using System;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Device;

public class DevicePropertiesView : ReactiveAppWindow<DevicePropertiesViewModel>
{
    public DevicePropertiesView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DeviceVisualizer_OnLedClicked(object? sender, LedClickedEventArgs e)
    {
        if (ViewModel == null)
            return;

        if (!e.PointerReleasedEventArgs.KeyModifiers.HasFlag(KeyModifiers.Shift))
            ViewModel.SelectedLeds.Clear();
        ViewModel.SelectedLeds.Add(e.Led);
    }

    private void DeviceVisualizer_OnClicked(object? sender, PointerReleasedEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            ViewModel?.ClearSelectedLeds.Execute().Subscribe();
    }

    private void DeviceDisplayGrid_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift) && e.Source is not DeviceVisualizer)
            ViewModel?.ClearSelectedLeds.Execute().Subscribe();
    }
}