using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Debugger.Tabs.Performance
{
    public class PerformanceDebugView : ReactiveUserControl<PerformanceDebugViewModel>
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
}