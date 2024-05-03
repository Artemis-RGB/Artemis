using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutListDefaultView : ReactiveUserControl<LayoutListDefaultViewModel>
{
    public LayoutListDefaultView()
    {
        InitializeComponent();
    }
}