using Artemis.Core.Models.Surface;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug
{
    public class DeviceDebugViewModel : Screen
    {
        public ArtemisDevice Device { get; }

        public DeviceDebugViewModel(ArtemisDevice device)
        {
            Device = device;
        }
    }
}