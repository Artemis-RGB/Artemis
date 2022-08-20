using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Color.Screens;

public partial class QuantizeNodeCustomView : ReactiveUserControl<QuantizeNodeCustomViewModel>
{
    public QuantizeNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}