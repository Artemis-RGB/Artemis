using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Home;

public partial class WorkshopOfflineView : ReactiveUserControl<WorkshopOfflineViewModel>
{
    public WorkshopOfflineView()
    {
        InitializeComponent();
    }
}