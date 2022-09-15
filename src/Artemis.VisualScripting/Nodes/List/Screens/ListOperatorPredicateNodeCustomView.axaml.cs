using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.VisualScripting.Nodes.List.Screens;

public partial class ListOperatorPredicateNodeCustomView : ReactiveUserControl<ListOperatorPredicateNodeCustomViewModel>
{
    public ListOperatorPredicateNodeCustomView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}