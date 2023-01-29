using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Input.Screens;

public partial class PressedKeyPositionNodeCustomView : ReactiveUserControl<PressedKeyPositionNodeCustomViewModel>
{
    public PressedKeyPositionNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}