using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug.Device.Tabs
{
    public class DevicePropertiesTabViewModel : Screen
    {
        public DevicePropertiesTabViewModel(ArtemisDevice device)
        {
            Device = device;
            DisplayName = "PROPERTIES";
        }

        public ArtemisDevice Device { get; }
    }
} 