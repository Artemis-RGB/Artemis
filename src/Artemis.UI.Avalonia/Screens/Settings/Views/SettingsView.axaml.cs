using Artemis.UI.Avalonia.Screens.Settings.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Settings.Views
{
    public class SettingsView : ReactiveUserControl<SettingsViewModel>
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}