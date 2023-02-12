using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.VisualScripting.Nodes.Transition.Screens;

public class EasingFunctionView : UserControl
{
    public EasingFunctionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}