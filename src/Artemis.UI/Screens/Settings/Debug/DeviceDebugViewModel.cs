using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using Stylet; // using PropertyChanged;

namespace Artemis.UI.Screens.Settings.Debug
{
    public class DeviceDebugViewModel : Screen
    {
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;
        private ArtemisLed _selectedLed;

        public DeviceDebugViewModel(ArtemisDevice device, IDeviceService deviceService, IDialogService dialogService)
        {
            _deviceService = deviceService;
            _dialogService = dialogService;
            Device = device;
        }

        public List<ArtemisLed> SelectedLeds => SelectedLed != null ? new List<ArtemisLed> {SelectedLed} : null;
        public ArtemisDevice Device { get; }

        public ArtemisLed SelectedLed
        {
            get => _selectedLed;
            set
            {
                if (!SetAndNotify(ref _selectedLed, value)) return;
                NotifyOfPropertyChange(nameof(SelectedLeds));
            }
        }

        public bool CanOpenImageDirectory => Device.RgbDevice.DeviceInfo.Image != null;

        // ReSharper disable UnusedMember.Global

        #region Command handlers

        public void ClearSelection()
        {
            SelectedLed = null;
        }

        public void IdentifyDevice()
        {
            _deviceService.IdentifyDevice(Device);
        }

        public void OpenPluginDirectory()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Device.DeviceProvider.Plugin.Directory.FullName);
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Welp, we couldn't open the device's plugin folder for you", e);
            }
        }

        public void OpenImageDirectory()
        {
            if (!CanOpenImageDirectory)
                return;

            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.GetDirectoryName(Device.RgbDevice.DeviceInfo.Image.AbsolutePath));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Welp, we couldn't open the device's image folder for you", e);
            }
        }

        #endregion

        // ReSharper restore UnusedMember.Global
    }
}