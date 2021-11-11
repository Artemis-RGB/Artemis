using Artemis.UI.Avalonia.Screens.Plugins.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Plugins.Views
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
