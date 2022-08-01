using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class InputPinView : PinView
{
    public InputPinView()
    {
        InitializeComponent();
        InitializePin(this.Get<Border>("PinPoint"));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void PinContainer_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Middle)
            ViewModel?.DisconnectPin.Execute().Subscribe();
    }
}