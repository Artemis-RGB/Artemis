using Artemis.Core.Models.Surface;
using Artemis.UI.Screens.Settings.Tabs.Devices;

namespace Artemis.UI.Ninject.Factories
{
    public interface IDeviceSettingsViewModelFactory : IArtemisUIFactory
    {
        DeviceSettingsViewModel Create(ArtemisDevice device);
    }
}