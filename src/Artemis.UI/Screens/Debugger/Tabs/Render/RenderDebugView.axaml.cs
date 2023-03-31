using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Render;

public partial class RenderDebugView : ReactiveUserControl<RenderDebugViewModel>
{
    public RenderDebugView()
    {
        InitializeComponent();
    }

}