using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class InstalledTabItemView : ReactiveUserControl<InstalledTabItemViewModel>
{
    public InstalledTabItemView()
    {
        InitializeComponent();
    }
}