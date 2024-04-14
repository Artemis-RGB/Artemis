using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutManageView : ReactiveUserControl<LayoutManageViewModel>
{
    public LayoutManageView()
    {
        InitializeComponent();
    }
}