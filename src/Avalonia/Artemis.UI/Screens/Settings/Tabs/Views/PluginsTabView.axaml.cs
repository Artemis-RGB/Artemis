using Artemis.UI.Screens.Settings.Tabs.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Settings.Tabs.Views
{
    public partial class PluginsTabView : ReactiveUserControl<PluginsTabViewModel>
    {
        public PluginsTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
