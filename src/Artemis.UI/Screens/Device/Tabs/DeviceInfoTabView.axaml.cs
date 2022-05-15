using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device
{
    public partial class DeviceInfoTabView : ReactiveUserControl<DeviceInfoTabViewModel>
    {
        public DeviceInfoTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
