using Artemis.UI.Avalonia.Screens.Settings.Tabs.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Settings.Tabs.Views
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