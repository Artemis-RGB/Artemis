using Artemis.UI.Avalonia.Screens.Device.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Device.Views
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