using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.VisualScripting.Pins;

public class OutputPinView : PinView
{
    public OutputPinView()
    {
        InitializeComponent();
        InitializePin(this.Get<Border>("PinPoint"));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}