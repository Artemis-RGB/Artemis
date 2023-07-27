using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutDetailsView : ReactiveUserControl<LayoutDetailsViewModel>
{
    public LayoutDetailsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}