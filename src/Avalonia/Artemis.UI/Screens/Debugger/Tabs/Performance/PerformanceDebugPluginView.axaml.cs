using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Debugger.Tabs.Performance
{
    public partial class PerformanceDebugPluginView : UserControl
    {
        public PerformanceDebugPluginView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
