using System.Windows;
using Artemis.Core;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.Screens.Settings.Device.Tabs
{
    public class DeviceInfoTabViewModel : Screen
    {
        private string _defaultLayoutPath;

        public DeviceInfoTabViewModel(ArtemisDevice device)
        {
            Device = device;
            DisplayName = "INFO";
        }

        public bool IsKeyboard => Device.DeviceType == RGBDeviceType.Keyboard;
        public ArtemisDevice Device { get; }

        public string DefaultLayoutPath
        {
            get => _defaultLayoutPath;
            set => SetAndNotify(ref _defaultLayoutPath, value);
        }

        public void CopyToClipboard(string content)
        {
            Clipboard.SetText(content);
            ((DeviceDialogViewModel) Parent).DeviceMessageQueue.Enqueue("Copied path to clipboard.");
        }

        protected override void OnInitialActivate()
        {
            DefaultLayoutPath = Device.DeviceProvider.LoadLayout(Device).FilePath;
            base.OnInitialActivate();
        }
    }
}