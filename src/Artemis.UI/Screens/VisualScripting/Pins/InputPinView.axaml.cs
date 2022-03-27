using Avalonia.Controls;
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
}