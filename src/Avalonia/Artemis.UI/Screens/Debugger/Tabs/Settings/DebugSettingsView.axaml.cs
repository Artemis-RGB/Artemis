using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Tabs.Settings
{
    public class DebugSettingsView : ReactiveUserControl<DebugSettingsViewModel>
    {
        public DebugSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}