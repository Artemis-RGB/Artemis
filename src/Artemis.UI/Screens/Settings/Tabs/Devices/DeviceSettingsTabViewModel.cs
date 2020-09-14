using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Devices
{
    public class DeviceSettingsTabViewModel : Conductor<DeviceSettingsViewModel>.Collection.AllActive
    {
        private readonly ISettingsVmFactory _settingsVmFactory;
        private readonly ISurfaceService _surfaceService;
        private BindableCollection<DeviceSettingsViewModel> _deviceSettingsViewModels;

        public DeviceSettingsTabViewModel(ISurfaceService surfaceService, ISettingsVmFactory settingsVmFactory)
        {
            DisplayName = "DEVICES";

            _surfaceService = surfaceService;
            _settingsVmFactory = settingsVmFactory;
        }
        
        protected override void OnActivate()
        {
            // Take it off the UI thread to avoid freezing on tab change
            Task.Run(() =>
            {
                Items.Clear();
                var instances = _surfaceService.ActiveSurface.Devices.Select(d => _settingsVmFactory.CreateDeviceSettingsViewModel(d)).ToList();
                foreach (var deviceSettingsViewModel in instances)
                    Items.Add(deviceSettingsViewModel);
            });
        }
    }
}