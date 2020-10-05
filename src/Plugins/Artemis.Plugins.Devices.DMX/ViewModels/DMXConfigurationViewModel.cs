using Artemis.Core;

namespace Artemis.Plugins.Devices.DMX.ViewModels
{
    public class DMXConfigurationViewModel : PluginConfigurationViewModel
    {
        public DMXConfigurationViewModel(Plugin plugin) : base(plugin)
        {
            RGB.NET.Devices.DMX.DMXDeviceProvider dmxInstance = RGB.NET.Devices.DMX.DMXDeviceProvider.Instance;
        }
    }
}