using Artemis.UI.Avalonia.Screens.Device.Tabs.ViewModels;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Device.Tabs.Views
{
    public partial class DevicePropertiesTabView : ReactiveUserControl<DevicePropertiesTabViewModel>
    {
        public DevicePropertiesTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            ViewModel?.BrowseCustomLayout();
        }
    }
}
