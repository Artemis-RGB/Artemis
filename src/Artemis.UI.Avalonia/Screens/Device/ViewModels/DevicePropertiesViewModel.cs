using Artemis.Core;

namespace Artemis.UI.Avalonia.Screens.Device.ViewModels
{
    public class DevicePropertiesViewModel : ActivatableViewModelBase
    {
        public DevicePropertiesViewModel(ArtemisDevice device)
        {
            Device = device;
        }

        public ArtemisDevice Device { get; }
    }
}