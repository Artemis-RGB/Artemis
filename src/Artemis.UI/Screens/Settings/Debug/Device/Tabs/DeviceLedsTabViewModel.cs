using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Device.Tabs
{
    public class DeviceLedsTabViewModel : Screen
    {

        public DeviceLedsTabViewModel(ArtemisDevice device)
        {
            Device = device;
            DisplayName = "LEDS";
        }

        public ArtemisDevice Device { get; }
    }
}