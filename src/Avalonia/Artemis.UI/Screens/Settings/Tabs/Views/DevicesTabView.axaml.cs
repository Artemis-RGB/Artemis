using Artemis.UI.Screens.Settings.Tabs.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Settings.Tabs.Views
{
    public class DevicesTabView : ReactiveUserControl<DevicesTabViewModel>
    {
        public DevicesTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}