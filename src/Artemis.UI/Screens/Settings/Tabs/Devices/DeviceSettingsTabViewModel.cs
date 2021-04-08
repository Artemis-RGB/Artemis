using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Devices
{
    public class DeviceSettingsTabViewModel : Conductor<DeviceSettingsViewModel>.Collection.AllActive
    {
        private readonly ISettingsVmFactory _settingsVmFactory;
        private readonly IRgbService _rgbService;
        private readonly IDialogService _dialogService;
        private bool _confirmedDisable;

        public DeviceSettingsTabViewModel(IRgbService rgbService, IDialogService dialogService, ISettingsVmFactory settingsVmFactory)
        {
            DisplayName = "DEVICES";

            _rgbService = rgbService;
            _dialogService = dialogService;
            _settingsVmFactory = settingsVmFactory;
        }

        #region Overrides of AllActive

        /// <inheritdoc />
        protected override void OnActivate()
        {
            // Take it off the UI thread to avoid freezing on tab change
            Task.Run(async () =>
            {
                if (Items.Any())
                    Items.Clear();
                
                await Task.Delay(200);

                List<DeviceSettingsViewModel> instances = _rgbService.Devices.Select(d => _settingsVmFactory.CreateDeviceSettingsViewModel(d)).ToList();
                foreach (DeviceSettingsViewModel deviceSettingsViewModel in instances)
                    Items.Add(deviceSettingsViewModel);
            });
            base.OnActivate();
        }

        #endregion

        public async Task<bool> ShowDeviceDisableDialog()
        {
            if (_confirmedDisable)
                return true;

            bool confirmed = await _dialogService.ShowConfirmDialog(
                "Disabling device",
                "Disabling a device will cause it to stop updating. " +
                "\r\nSome SDKs will even go back to using manufacturer lighting (Artemis restart may be required)."
            );
            if (confirmed)
                _confirmedDisable = true;

            return confirmed;
        }
    }
}