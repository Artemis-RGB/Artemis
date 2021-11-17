using Artemis.UI.Screens.Plugins.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Plugins.Views
{
    public partial class PluginFeatureView : ReactiveUserControl<PluginFeatureViewModel>
    {
        public PluginFeatureView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
