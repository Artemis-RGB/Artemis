using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Performance;

public partial class PerformanceDebugView : ReactiveUserControl<PerformanceDebugViewModel>
{
    public PerformanceDebugView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}