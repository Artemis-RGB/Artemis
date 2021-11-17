using Artemis.UI.Screens.Device.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device.Views
{
    public class DeviceDetectInputView : ReactiveUserControl<DeviceDetectInputViewModel>
    {
        public DeviceDetectInputView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}