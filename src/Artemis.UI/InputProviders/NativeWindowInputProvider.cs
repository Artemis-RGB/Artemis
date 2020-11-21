using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Linearstar.Windows.RawInput;
using Linearstar.Windows.RawInput.Native;
using RGB.NET.Core;

namespace Artemis.UI.InputProviders
{
    public class NativeWindowInputProvider : InputProvider
    {
        private const int WM_INPUT = 0x00FF;

        private readonly ISurfaceService _surfaceService;
        private List<ArtemisDevice> _keyboards;
        private DateTime _lastMouseUpdate;
        private List<ArtemisDevice> _mice;
        private SpongeWindow _sponge;

        public NativeWindowInputProvider(ISurfaceService surfaceService)
        {
            _surfaceService = surfaceService;

            _sponge = new SpongeWindow();
            _sponge.WndProcCalled += SpongeOnWndProcCalled;

            RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.ExInputSink | RawInputDeviceFlags.NoLegacy, _sponge.Handle);
            RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, _sponge.Handle);

            _surfaceService.ActiveSurfaceConfigurationSelected += SurfaceConfigurationChanged;
            _surfaceService.SurfaceConfigurationUpdated += SurfaceConfigurationChanged;
            GetDevices(surfaceService.ActiveSurface);
        }

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sponge?.DestroyHandle();
                _sponge = null;

                _surfaceService.ActiveSurfaceConfigurationSelected -= SurfaceConfigurationChanged;
                _surfaceService.SurfaceConfigurationUpdated -= SurfaceConfigurationChanged;
            }

            base.Dispose(disposing);
        }

        #endregion

        private void SurfaceConfigurationChanged(object sender, SurfaceConfigurationEventArgs e)
        {
            GetDevices(e.Surface);
        }

        private void GetDevices(ArtemisSurface surface)
        {
            _keyboards = surface.Devices.Where(d => d.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard).ToList();
            _mice = surface.Devices.Where(d => d.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Mouse).ToList();
        }

        private void SpongeOnWndProcCalled(object sender, Message message)
        {
            if (message.Msg != WM_INPUT)
                return;

            RawInputData data = RawInputData.FromHandle(message.LParam);
            switch (data)
            {
                case RawInputMouseData mouse:
                    HandleMouseData(mouse);
                    break;
                case RawInputKeyboardData keyboard:
                    HandleKeyboardData(keyboard);
                    break;
            }
        }

        private void HandleKeyboardData(RawInputKeyboardData data)
        {
            // Get the keyboard that submitted the data
            ArtemisDevice match = _keyboards?.FirstOrDefault();
            if (match == null)
                return;

            InputKey key = (InputKey) KeyInterop.KeyFromVirtualKey(data.Keyboard.VirutalKey);

            // Debug.WriteLine($"VK: {key} ({data.Keyboard.VirutalKey}), Flags: {data.Keyboard.Flags}, Scan code: {data.Keyboard.ScanCode}");

            // Sometimes we get double hits and they resolve to None, ignore those
            if (key == InputKey.None)
                return;

            // Right alt triggers LeftCtrl with a different scan code for some reason, ignore those
            if (key == InputKey.LeftCtrl && data.Keyboard.ScanCode == 56)
                return;

            // Duplicate keys with different positions can be identified by the LeftKey flag (even though its set of the key that's physically on the right)
            if (data.Keyboard.Flags == RawKeyboardFlags.LeftKey || data.Keyboard.Flags == (RawKeyboardFlags.LeftKey | RawKeyboardFlags.Up))
            {
                if (key == InputKey.Enter)
                    key = InputKey.NumPadEnter;
                if (key == InputKey.LeftCtrl)
                    key = InputKey.RightCtrl;
                if (key == InputKey.LeftAlt)
                    key = InputKey.RightAlt;
            }

            if (key == InputKey.LeftShift && data.Keyboard.ScanCode == 54)
                key = InputKey.RightShift;

            bool isDown = data.Keyboard.Flags != RawKeyboardFlags.Up &&
                          data.Keyboard.Flags != (RawKeyboardFlags.Up | RawKeyboardFlags.LeftKey) &&
                          data.Keyboard.Flags != (RawKeyboardFlags.Up | RawKeyboardFlags.RightKey);

            OnKeyboardDataReceived(new InputProviderKeyboardEventArgs(match, key, isDown));
        }

        private void HandleMouseData(RawInputMouseData data)
        {
            // Only handle mouse movement 25 times per second
            if (data.Mouse.Buttons == RawMouseButtonFlags.None)
                if (DateTime.Now - _lastMouseUpdate < TimeSpan.FromMilliseconds(40))
                    return;

            _lastMouseUpdate = DateTime.Now;

            // Get the keyboard that submitted the data
            ArtemisDevice match = _mice?.FirstOrDefault();
            if (match == null)
                return;
        }
    }
}