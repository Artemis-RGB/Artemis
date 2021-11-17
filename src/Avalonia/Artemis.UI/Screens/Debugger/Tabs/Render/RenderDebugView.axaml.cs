using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Tabs.Render
{
    public class RenderDebugView : ReactiveUserControl<RenderDebugViewModel>
    {
        public RenderDebugView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}