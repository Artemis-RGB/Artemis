using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Device
{
    public partial class DeviceSettingsView : UserControl
    {
        public DeviceSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
