using Artemis.UI.Screens.Plugins.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Plugins.Views
{
    public partial class PluginSettingsView : ReactiveUserControl<PluginSettingsViewModel>
    {
        public PluginSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
