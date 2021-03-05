using Artemis.Core;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Device.Tabs
{
    public class DeviceInfoTabViewModel : Screen
    {
        public DeviceInfoTabViewModel(ArtemisDevice device)
        {
            Device = device;
            DisplayName = "INFO";
        }

        public bool IsKeyboard => Device.RgbDevice is IKeyboard;
        public ArtemisDevice Device { get; }
    }
}