using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared;
using RGB.NET.Core;
using ArtemisLed = Artemis.Core.ArtemisLed;

namespace Artemis.UI.Screens.Device
{
    public class DevicePropertiesViewModel : DialogViewModelBase<object>
    {
        public DevicePropertiesViewModel(ArtemisDevice device, IDeviceVmFactory deviceVmFactory)
        {
            Device = device;
            SelectedLeds = new ObservableCollection<ArtemisLed>();
            Tabs = new ObservableCollection<ActivatableViewModelBase>();

            Tabs.Add(deviceVmFactory.DevicePropertiesTabViewModel(device));
            Tabs.Add(deviceVmFactory.DeviceInfoTabViewModel(device));
            if (Device .DeviceType == RGBDeviceType.Keyboard)
                Tabs.Add(deviceVmFactory.InputMappingsTabViewModel(device, SelectedLeds));
            Tabs.Add(deviceVmFactory.DeviceLedsTabViewModel(device, SelectedLeds));
        }

        public ArtemisDevice Device { get; }
        public ObservableCollection<ArtemisLed> SelectedLeds { get; }
        public ObservableCollection<ActivatableViewModelBase> Tabs { get; }
    }
}