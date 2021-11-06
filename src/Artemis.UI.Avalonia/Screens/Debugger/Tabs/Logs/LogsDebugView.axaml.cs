using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Debugger.Tabs.Logs
{
    public class LogsDebugView : ReactiveUserControl<LogsDebugViewModel>
    {
        public LogsDebugView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}