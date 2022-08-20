using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Avalonia;
using RGB.NET.Core;

namespace Artemis.UI.Screens.Device
{
    public class DeviceInfoTabViewModel : ActivatableViewModelBase
    {
        private readonly INotificationService _notificationService;

        public DeviceInfoTabViewModel(ArtemisDevice device, INotificationService notificationService)
        {
            _notificationService = notificationService;

            Device = device;
            DisplayName = "Info";

            DefaultLayoutPath = Device.DeviceProvider.LoadLayout(Device).FilePath;
        }

        public bool IsKeyboard => Device.DeviceType == RGBDeviceType.Keyboard;
        public ArtemisDevice Device { get; }

        public string DefaultLayoutPath { get; }

        public async Task CopyToClipboard(string content)
        {
            if (Application.Current?.Clipboard == null)
                return;

            await Application.Current.Clipboard.SetTextAsync(content);
            _notificationService.CreateNotification().WithMessage("Copied path to clipboard.").Show();
        }
    }
}