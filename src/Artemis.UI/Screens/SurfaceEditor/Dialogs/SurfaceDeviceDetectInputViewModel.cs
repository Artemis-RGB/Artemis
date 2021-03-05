using System;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using MaterialDesignThemes.Wpf;
using RGB.NET.Core;

namespace Artemis.UI.Screens.SurfaceEditor.Dialogs
{
    public class SurfaceDeviceDetectInputViewModel : DialogViewModelBase
    {
        private readonly IInputService _inputService;
        private readonly IMessageService _messageService;
        private readonly IRgbService _rgbService;
        private readonly ListLedGroup _ledGroup;

        public SurfaceDeviceDetectInputViewModel(ArtemisDevice device, IInputService inputService, IMessageService messageService, IRgbService rgbService)
        {
            Device = device;
            Title = $"{Device.RgbDevice.DeviceInfo.DeviceName} - Detect input";
            IsMouse = Device.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Mouse;

            _inputService = inputService;
            _messageService = messageService;
            _rgbService = rgbService;
            _inputService.IdentifyDevice(Device);
            _inputService.DeviceIdentified += InputServiceOnDeviceIdentified;

            // Create a LED group way at the top
            _ledGroup = new ListLedGroup(_rgbService.Surface, Device.Leds.Select(l => l.RgbLed))
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
            _messageService.ShowMessage($"{Device.RgbDevice.DeviceInfo.DeviceName} identified 😁");
        }
    }
}