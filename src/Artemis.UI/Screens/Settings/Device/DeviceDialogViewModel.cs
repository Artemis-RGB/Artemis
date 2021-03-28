using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using Ookii.Dialogs.Wpf;
using RGB.NET.Layout;
using Stylet;

namespace Artemis.UI.Screens.Settings.Device
{
    public class DeviceDialogViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly IDeviceService _deviceService;
        private readonly IDialogService _dialogService;
        private readonly IRgbService _rgbService;
        private ArtemisLed _selectedLed;
        private SnackbarMessageQueue _deviceMessageQueue;

        public DeviceDialogViewModel(ArtemisDevice device, IDeviceService deviceService, IRgbService rgbService, IDialogService dialogService, IDeviceDebugVmFactory factory)
        {
            _deviceService = deviceService;
            _rgbService = rgbService;
            _dialogService = dialogService;

            Device = device;
            PanZoomViewModel = new PanZoomViewModel();

            Items.Add(factory.DevicePropertiesTabViewModel(device));
            Items.Add(factory.DeviceInfoTabViewModel(device));
            Items.Add(factory.DeviceLedsTabViewModel(device));
            ActiveItem = Items.First();
            DisplayName = $"{device.RgbDevice.DeviceInfo.Model} | Artemis";
        }

        protected override void OnInitialActivate()
        {
            DeviceMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(5));
            Device.DeviceUpdated += DeviceOnDeviceUpdated;
            base.OnInitialActivate();
        }

        protected override void OnClose()
        {
            Device.DeviceUpdated -= DeviceOnDeviceUpdated;
            base.OnClose();
        }

        public ArtemisDevice Device { get; }
        public PanZoomViewModel PanZoomViewModel { get; }

        public SnackbarMessageQueue DeviceMessageQueue
        {
            get => _deviceMessageQueue;
            set => SetAndNotify(ref _deviceMessageQueue, value);
        }

        public ArtemisLed SelectedLed
        {
            get => _selectedLed;
            set
            {
                if (!SetAndNotify(ref _selectedLed, value)) return;
                NotifyOfPropertyChange(nameof(SelectedLeds));
            }
        }

        public bool CanExportLayout => Device.Layout?.IsValid ?? false;

        public List<ArtemisLed> SelectedLeds => SelectedLed != null ? new List<ArtemisLed> {SelectedLed} : null;

        public bool CanOpenImageDirectory => Device.Layout?.Image != null;

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
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", Path.GetDirectoryName(Device.Layout.Image.AbsolutePath));
            }
            catch (Exception e)
            {
                _dialogService.ShowExceptionDialog("Welp, we couldn't open the device's image folder for you", e);
            }
        }

        public void ReloadLayout()
        {
            _rgbService.ApplyBestDeviceLayout(Device);
        }

        public void ExportLayout()
        {
            if (Device.Layout == null)
                return;

            VistaFolderBrowserDialog dialog = new()
            {
                Description = "Select layout export target folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                SelectedPath = Path.Combine(Constants.DataFolder, "user layouts")
            };

            bool? result = dialog.ShowDialog();
            if (result != true)
                return;

            string directory = Path.Combine(
                dialog.SelectedPath,
                Device.RgbDevice.DeviceInfo.Manufacturer,
                Device.RgbDevice.DeviceInfo.DeviceType.ToString()
            );
            string filePath = Path.Combine(directory, Device.GetLayoutFileName());
            Core.Utilities.CreateAccessibleDirectory(directory);

            // XML
            XmlSerializer serializer = new(typeof(DeviceLayout));
            using StreamWriter writer = new(filePath);
            serializer.Serialize(writer, Device.Layout!.RgbLayout);

            // Device images
            if (!Uri.IsWellFormedUriString(Device.Layout.FilePath, UriKind.Absolute))
                return;

            Uri targetDirectory = new(directory + "/", UriKind.Absolute);
            Uri sourceDirectory = new(Path.GetDirectoryName(Device.Layout.FilePath)! + "/", UriKind.Absolute);
            Uri deviceImageTarget = new(targetDirectory, Device.Layout.LayoutCustomDeviceData.DeviceImage);

            // Create folder (if needed) and copy image
            Core.Utilities.CreateAccessibleDirectory(Path.GetDirectoryName(deviceImageTarget.LocalPath)!);
            if (Device.Layout.Image != null && File.Exists(Device.Layout.Image.LocalPath) && !File.Exists(deviceImageTarget.LocalPath))
                File.Copy(Device.Layout.Image.LocalPath, deviceImageTarget.LocalPath);

            foreach (ArtemisLedLayout ledLayout in Device.Layout.Leds)
            {
                if (ledLayout.LayoutCustomLedData.LogicalLayouts == null)
                    continue;

                // Only the image of the current logical layout is available as an URI, iterate each layout and find the images manually
                foreach (LayoutCustomLedDataLogicalLayout logicalLayout in ledLayout.LayoutCustomLedData.LogicalLayouts)
                {
                    Uri image = new(sourceDirectory, logicalLayout.Image);
                    Uri imageTarget = new(targetDirectory, logicalLayout.Image);

                    // Create folder (if needed) and copy image
                    Core.Utilities.CreateAccessibleDirectory(Path.GetDirectoryName(imageTarget.LocalPath)!);
                    if (File.Exists(image.LocalPath) && !File.Exists(imageTarget.LocalPath))
                        File.Copy(image.LocalPath, imageTarget.LocalPath);
                }
            }
        }

        #endregion

        // ReSharper restore UnusedMember.Global

        #region Event handlers

        private void DeviceOnDeviceUpdated(object sender, EventArgs e)
        {
            NotifyOfPropertyChange(nameof(CanExportLayout));
        }

        public void OnLedClicked(object sender, LedClickedEventArgs e)
        {
            SelectedLed = e.Led;
        }

        #endregion
    }
}