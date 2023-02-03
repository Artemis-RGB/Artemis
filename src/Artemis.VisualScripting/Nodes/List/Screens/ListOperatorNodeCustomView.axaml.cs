using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.List.Screens;

public partial class ListOperatorNodeCustomView : ReactiveUserControl<ListOperatorNodeCustomViewModel>
{
    public ListOperatorNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}