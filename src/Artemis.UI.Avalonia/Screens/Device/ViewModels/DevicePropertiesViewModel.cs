using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Avalonia.Ninject.Factories;
using RGB.NET.Core;
using ArtemisLed = Artemis.Core.ArtemisLed;

namespace Artemis.UI.Avalonia.Screens.Device.ViewModels
{
    public class DevicePropertiesViewModel : ActivatableViewModelBase
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