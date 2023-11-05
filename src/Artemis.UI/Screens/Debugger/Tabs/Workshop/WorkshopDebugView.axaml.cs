using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Workshop;

public partial class WorkshopDebugView : ReactiveUserControl<WorkshopDebugViewModel>
{
    public WorkshopDebugView()
    {
        InitializeComponent();
    }
}