using System;
using System.Linq;
using Artemis.Core.Services;
using Artemis.UI.Screens.SurfaceEditor.Visualization;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using RGB.NET.Brushes;
using RGB.NET.Core;
using RGB.NET.Groups;

namespace Artemis.UI.Screens.SurfaceEditor.Dialogs
{
    public class SurfaceDeviceDetectInputViewModel : DialogViewModelBase
    {
        private readonly IInputService _inputService;
        private readonly ISnackbarMessageQueue _mainMessageQueue;
        private string _title;
        private ListLedGroup _ledGroup;

        public SurfaceDeviceDetectInputViewModel(SurfaceDeviceViewModel surfaceDeviceViewModel, IInputService inputService, ISnackbarMessageQueue mainMessageQueue)
        {
            SurfaceDeviceViewModel = surfaceDeviceViewModel;
            Title = $"{SurfaceDeviceViewModel.Device.RgbDevice.DeviceInfo.DeviceName} - Detect input";

            // Create a LED group way at the top
            _ledGroup = new ListLedGroup(SurfaceDeviceViewModel.Device.Leds.Select(l => l.RgbLed))
            {
                Brush = new SolidColorBrush(new Color(255, 255, 0)),
                ZIndex = 999
            };


            _inputService = inputService;
            _mainMessageQueue = mainMessageQueue;
            _inputService.IdentifyDevice(surfaceDeviceViewModel.Device);
            _inputService.DeviceIdentified += InputServiceOnDeviceIdentified;
        }

        public SurfaceDeviceViewModel SurfaceDeviceViewModel { get; }

        public string Title
        {
            get => _title;
            set => SetAndNotify(ref _title, value);
        }

        public override void OnDialogClosed(object sender, DialogClosingEventArgs e)
        {
            base.OnDialogClosed(sender, e);
            _inputService.DeviceIdentified -= InputServiceOnDeviceIdentified;
            _ledGroup.Detach();
        }

        public void Cancel()
        {
            Session?.Close(false);
        }

        private void InputServiceOnDeviceIdentified(object sender, EventArgs e)
        {
            Session?.Close(true);
            _mainMessageQueue.Enqueue("Device identified.");
        }
    }
}