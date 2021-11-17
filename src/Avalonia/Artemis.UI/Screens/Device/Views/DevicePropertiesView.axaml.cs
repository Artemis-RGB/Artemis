using Artemis.UI.Screens.Device.ViewModels;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device.Views
{
    public partial class DevicePropertiesView : ReactiveWindow<DevicePropertiesViewModel>
    {
        public DevicePropertiesView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
