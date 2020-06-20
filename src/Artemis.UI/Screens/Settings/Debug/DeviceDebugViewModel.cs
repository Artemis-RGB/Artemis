using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Artemis.Core.Models.Surface;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.Interfaces;
// using PropertyChanged;
using Stylet;

namespace Artemis.UI.Screens.Settings.Debug
{
    public class DeviceDebugViewModel : Screen
    {
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;

        public DeviceDebugViewModel(ArtemisDevice device, IDeviceService deviceService, IDialogService dialogService)
        {
            _deviceService = deviceService;
            _dialogService = dialogService;
            Device = device;
        }

        // [DependsOn(nameof(SelectedLed))]
        public List<ArtemisLed> SelectedLeds => SelectedLed != null ? new List<ArtemisLed> {SelectedLed} : null;

        public ArtemisDevice Device { get; }
        public ArtemisLed SelectedLed { get; set; }

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

        public async void OpenPluginDirectory()
        {
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Device.Plugin.PluginInfo.Directory.FullName);
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("Welp, we couldn't open the device's plugin folder for you", e);
            }
        }

        public async void OpenImageDirectory()
        {
            if (!CanOpenImageDirectory)
                return;

            try
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.GetDirectoryName(Device.RgbDevice.DeviceInfo.Image.AbsolutePath));
            }
            catch (Exception e)
            {
                await _dialogService.ShowExceptionDialog("Welp, we couldn't open the device's image folder for you", e);
            }
        }

        #endregion

        // ReSharper restore UnusedMember.Global
    }
}