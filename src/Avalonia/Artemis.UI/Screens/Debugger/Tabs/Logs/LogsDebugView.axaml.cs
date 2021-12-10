using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Logs
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