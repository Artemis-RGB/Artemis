using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.External.Screens;

public partial class LayerPropertyNodeCustomView : ReactiveUserControl<LayerPropertyNodeCustomViewModel>
{
    public LayerPropertyNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}