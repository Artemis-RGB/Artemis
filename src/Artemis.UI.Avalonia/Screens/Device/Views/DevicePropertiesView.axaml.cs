using Artemis.UI.Avalonia.Screens.Device.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Device.Views
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
