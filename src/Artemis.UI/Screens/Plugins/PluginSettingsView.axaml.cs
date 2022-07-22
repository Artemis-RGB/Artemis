using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

namespace Artemis.UI.Screens.Plugins
{
    public partial class PluginSettingsView : ReactiveUserControl<PluginSettingsViewModel>
    {
        private readonly CheckBox _enabledToggle;

        public PluginSettingsView()
        {
            InitializeComponent();
            _enabledToggle = this.Find<CheckBox>("EnabledToggle");
            _enabledToggle.Click += EnabledToggleOnClick;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void EnabledToggleOnClick(object? sender, RoutedEventArgs e)
        {
            Dispatcher.UIThread.Post(() => ViewModel?.UpdateEnabled(!ViewModel.Plugin.IsEnabled));
        }
    }
}