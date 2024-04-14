using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutDescriptionView : ReactiveUserControl<LayoutDescriptionViewModel>
{
    public LayoutDescriptionView()
    {
        InitializeComponent();
    }
}