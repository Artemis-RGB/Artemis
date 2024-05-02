using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.LayoutFinder;

public partial class LayoutFinderView : ReactiveUserControl<LayoutFinderViewModel>
{
    public LayoutFinderView()
    {
        InitializeComponent();
    }
}