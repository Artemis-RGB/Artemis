using System;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
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
        private readonly ListLedGroup _ledGroup;
        private readonly ISnackbarMessageQueue _mainMessageQueue;

        public SurfaceDeviceDetectInputViewModel(ArtemisDevice device, IInputService inputService, ISnackbarMessageQueue mainMessageQueue)
        {
            Device = device;
            Title = $"{Device.RgbDevice.DeviceInfo.DeviceName} - Detect input";
            IsMouse = Device.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Mouse;

            _inputService = inputService;
            _mainMessageQueue = mainMessageQueue;
            _inputService.IdentifyDevice(Device);
            _inputService.DeviceIdentified += InputServiceOnDeviceIdentified;

            // Create a LED group way at the top
            _ledGroup = new ListLedGroup(Device.Leds.Select(l => l.RgbLed))
            {
                Brush = new SolidColorBrush(new Color(255, 255, 0)),
                ZIndex = 999
            };
        }

        public ArtemisDevice Device { get; }
        public string Title { get; }
        public bool IsMouse { get; }

        public override void OnDialogClosed(object sender, DialogClosingEventArgs e)
        {
            base.OnDialogClosed(sender, e);
            _inputService.DeviceIdentified -= InputServiceOnDeviceIdentified;
            _ledGroup.Detach();
        }

        private void InputServiceOnDeviceIdentified(object sender, EventArgs e)
        {
            Session?.Close(true);
            _mainMessageQueue.Enqueue($"{Device.RgbDevice.DeviceInfo.DeviceName} identified 😁");
        }
    }
}