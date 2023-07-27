using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutListView : ReactiveUserControl<LayoutListViewModel>
{
    public LayoutListView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}