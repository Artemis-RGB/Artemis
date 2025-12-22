using System;
using Avalonia.Input;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public partial class OutputPinView : PinView
{
    public OutputPinView()
    {
        InitializeComponent();
        InitializePin(PinPoint);
    }


    private void PinContainer_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Middle)
            ViewModel?.DisconnectPin.Execute().Subscribe();
    }
}