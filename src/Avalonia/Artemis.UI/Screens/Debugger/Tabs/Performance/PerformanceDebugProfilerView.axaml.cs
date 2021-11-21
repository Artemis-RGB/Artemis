using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Debugger.Tabs.Performance
{
    public partial class PerformanceDebugProfilerView : UserControl
    {
        public PerformanceDebugProfilerView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
