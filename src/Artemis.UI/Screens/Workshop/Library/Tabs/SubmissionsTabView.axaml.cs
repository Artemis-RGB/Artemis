using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class SubmissionsTabView : ReactiveUserControl<SubmissionsTabViewModel>
{
    public SubmissionsTabView()
    {
        InitializeComponent();
    }
}