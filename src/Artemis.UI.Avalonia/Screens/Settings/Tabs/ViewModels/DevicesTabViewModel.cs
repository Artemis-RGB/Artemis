using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Avalonia.Ninject.Factories;
using Artemis.UI.Avalonia.Screens.Device.ViewModels;
using Artemis.UI.Avalonia.Shared;
using Artemis.UI.Avalonia.Shared.Services.Interfaces;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Settings.Tabs.ViewModels
{
    public class DevicesTabViewModel : ActivatableViewModelBase
    {
        private readonly IDeviceVmFactory _deviceVmFactory;
        private readonly IRgbService _rgbService;
        private readonly IWindowService _windowService;
        private bool _confirmedDisable;

        public DevicesTabViewModel(IRgbService rgbService, IWindowService windowService, IDeviceVmFactory deviceVmFactory)
        {
            DisplayName = "Devices";

            _rgbService = rgbService;
            _windowService = windowService;
            _deviceVmFactory = deviceVmFactory;

            Devices = new ObservableCollection<DeviceSettingsViewModel>();
            this.WhenActivated(disposables =>
            {
                GetDevices();

                Observable.FromEventPattern<DeviceEventArgs>(x => rgbService.DeviceAdded += x, x => rgbService.DeviceAdded -= x)
                    .Subscribe(d => AddDevice(d.EventArgs.Device))
                    .DisposeWith(disposables);
                Observable.FromEventPattern<DeviceEventArgs>(x => rgbService.DeviceRemoved += x, x => rgbService.DeviceRemoved -= x)
                    .Subscribe(d => RemoveDevice(d.EventArgs.Device))
                    .DisposeWith(disposables);
            });
        }

        private void GetDevices()
        {
            Devices.Clear();
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Devices.AddRange(_rgbService.Devices.Select(d => _deviceVmFactory.DeviceSettingsViewModel(d, this)));
            }, DispatcherPriority.Background);
        }

        public ObservableCollection<DeviceSettingsViewModel> Devices { get; }

        public async Task<bool> ShowDeviceDisableDialog()
        {
            if (_confirmedDisable)
                return true;

            bool confirmed = await _windowService.ShowConfirmContentDialog(
                "Disabling device",
                "Disabling a device will cause it to stop updating. " +
                "\r\nSome SDKs will even go back to using manufacturer lighting (Artemis restart may be required)."
            );
            if (confirmed)
                _confirmedDisable = true;

            return confirmed;
        }

        private void AddDevice(ArtemisDevice device)
        {
            Devices.Add(_deviceVmFactory.DeviceSettingsViewModel(device, this));
        }

        private void RemoveDevice(ArtemisDevice device)
        {
            DeviceSettingsViewModel? viewModel = Devices.FirstOrDefault(i => i.Device == device);
            if (viewModel != null)
                Devices.Remove(viewModel);
        }
    }
}