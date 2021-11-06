using System.Threading.Tasks;
using Artemis.Core;
using Avalonia;
using RGB.NET.Core;

namespace Artemis.UI.Avalonia.Screens.Device.ViewModels
{
    public class DeviceInfoTabViewModel : ActivatableViewModelBase
    {
        public DeviceInfoTabViewModel(ArtemisDevice device)
        {
            Device = device;
            DisplayName = "Info";

            DefaultLayoutPath = Device.DeviceProvider.LoadLayout(Device).FilePath;
        }

        public bool IsKeyboard => Device.DeviceType == RGBDeviceType.Keyboard;
        public ArtemisDevice Device { get; }

        public string DefaultLayoutPath { get; }

        public async Task CopyToClipboard(string content)
        {
            await Application.Current.Clipboard.SetTextAsync(content);
            // ((DeviceDialogViewModel) Parent).DeviceMessageQueue.Enqueue("Copied path to clipboard.");
        }
    }
}