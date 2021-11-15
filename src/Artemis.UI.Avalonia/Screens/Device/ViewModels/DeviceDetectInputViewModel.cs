using Artemis.Core;

namespace Artemis.UI.Avalonia.Screens.Device.ViewModels
{
    public class DeviceDetectInputViewModel
    {
        public DeviceDetectInputViewModel(ArtemisDevice device)
        {
            Device = device;
        }

        public ArtemisDevice Device { get; }
        public bool MadeChanges { get; set; }
    }
}