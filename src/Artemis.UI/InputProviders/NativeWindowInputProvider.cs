using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using Artemis.Core;
using Artemis.Core.Services;
using Linearstar.Windows.RawInput;
using Linearstar.Windows.RawInput.Native;
using Serilog;
using MouseButton = Artemis.Core.Services.MouseButton;

namespace Artemis.UI.InputProviders
{
    public class NativeWindowInputProvider : InputProvider
    {
        private const int WM_INPUT = 0x00FF;

        private readonly ILogger _logger;
        private readonly IInputService _inputService;
        private DateTime _lastMouseUpdate;
        private SpongeWindow _sponge;

        public NativeWindowInputProvider(ILogger logger, IInputService inputService)
        {
            _logger = logger;
            _inputService = inputService;

            _sponge = new SpongeWindow();
            _sponge.WndProcCalled += SpongeOnWndProcCalled;

            RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink, _sponge.Handle);
            RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, _sponge.Handle);
        }

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sponge?.DestroyHandle();
                _sponge = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        private void SpongeOnWndProcCalled(object sender, Message message)
        {
            if (message.Msg != WM_INPUT)
                return;

            RawInputData data = RawInputData.FromHandle(message.LParam);
            switch (data)
            {
                case RawInputMouseData mouse:
                    HandleMouseData(data, mouse);
                    break;
                case RawInputKeyboardData keyboard:
                    HandleKeyboardData(data, keyboard);
                    break;
            }
        }

        #region Keyboard

        private void HandleKeyboardData(RawInputData data, RawInputKeyboardData keyboardData)
        {
            KeyboardKey key = (KeyboardKey) KeyInterop.KeyFromVirtualKey(keyboardData.Keyboard.VirutalKey);
            // Debug.WriteLine($"VK: {key} ({keyboardData.Keyboard.VirutalKey}), Flags: {keyboardData.Keyboard.Flags}, Scan code: {keyboardData.Keyboard.ScanCode}");

            // Sometimes we get double hits and they resolve to None, ignore those
            if (key == KeyboardKey.None)
                return;

            // Right alt triggers LeftCtrl with a different scan code for some reason, ignore those
            if (key == KeyboardKey.LeftCtrl && keyboardData.Keyboard.ScanCode == 56)
                return;

            string identifier = data.Device?.DevicePath;

            // Let the core know there is an identifier so it can store new identifications if applicable
            if (identifier != null)
                OnIdentifierReceived(identifier, InputDeviceType.Keyboard);

            ArtemisDevice device = null;
            if (identifier != null)
            {
                try
                {
                    device = _inputService.GetDeviceByIdentifier(this, identifier, InputDeviceType.Keyboard);
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Failed to retrieve input device by its identifier");
                }
            }

            // Duplicate keys with different positions can be identified by the LeftKey flag (even though its set of the key that's physically on the right)
            if (keyboardData.Keyboard.Flags == RawKeyboardFlags.LeftKey || keyboardData.Keyboard.Flags == (RawKeyboardFlags.LeftKey | RawKeyboardFlags.Up))
            {
                if (key == KeyboardKey.Enter)
                    key = KeyboardKey.NumPadEnter;
                if (key == KeyboardKey.LeftCtrl)
                    key = KeyboardKey.RightCtrl;
                if (key == KeyboardKey.LeftAlt)
                    key = KeyboardKey.RightAlt;
            }

            if (key == KeyboardKey.LeftShift && keyboardData.Keyboard.ScanCode == 54)
                key = KeyboardKey.RightShift;

            bool isDown = keyboardData.Keyboard.Flags != RawKeyboardFlags.Up &&
                          keyboardData.Keyboard.Flags != (RawKeyboardFlags.Up | RawKeyboardFlags.LeftKey) &&
                          keyboardData.Keyboard.Flags != (RawKeyboardFlags.Up | RawKeyboardFlags.RightKey);

            OnKeyboardDataReceived(device, key, isDown);
        }

        #endregion

        #region Mouse

        private int _mouseDeltaX;
        private int _mouseDeltaY;

        private void HandleMouseData(RawInputData data, RawInputMouseData mouseData)
        {
            // Only submit mouse movement 25 times per second but increment the delta
            // This can create a small inaccuracy of course, but Artemis is not a shooter :')
            if (mouseData.Mouse.Buttons == RawMouseButtonFlags.None)
            {
                _mouseDeltaX += mouseData.Mouse.LastX;
                _mouseDeltaY += mouseData.Mouse.LastY;
                if (DateTime.Now - _lastMouseUpdate < TimeSpan.FromMilliseconds(40))
                    return;
            }

            ArtemisDevice device = null;
            string identifier = data.Device?.DevicePath;
            if (identifier != null)
            {
                try
                {
                    device = _inputService.GetDeviceByIdentifier(this, identifier, InputDeviceType.Keyboard);
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Failed to retrieve input device by its identifier");
                }
            }

            // Debug.WriteLine($"Buttons: {data.Mouse.Buttons}, Data: {data.Mouse.ButtonData}, Flags: {data.Mouse.Flags}, XY: {data.Mouse.LastX},{data.Mouse.LastY}");

            // Movement
            if (mouseData.Mouse.Buttons == RawMouseButtonFlags.None)
            {
                Win32Point cursorPosition = GetCursorPosition();
                OnMouseMoveDataReceived(device, cursorPosition.X, cursorPosition.Y, _mouseDeltaX, _mouseDeltaY);
                _mouseDeltaX = 0;
                _mouseDeltaY = 0;
                _lastMouseUpdate = DateTime.Now;
                return;
            }

            // Now we know its not movement, let the core know there is an identifier so it can store new identifications if applicable
            if (identifier != null)
                OnIdentifierReceived(identifier, InputDeviceType.Mouse);

            // Scrolling
            if (mouseData.Mouse.ButtonData != 0)
            {
                if (mouseData.Mouse.Buttons == RawMouseButtonFlags.MouseWheel)
                    OnMouseScrollDataReceived(device, MouseScrollDirection.Vertical, mouseData.Mouse.ButtonData);
                else if (mouseData.Mouse.Buttons == RawMouseButtonFlags.MouseHorizontalWheel)
                    OnMouseScrollDataReceived(device, MouseScrollDirection.Horizontal, mouseData.Mouse.ButtonData);
                return;
            }

            // Button presses
            MouseButton button = MouseButton.Left;
            bool isDown = false;

            // Left
            if (DetermineMouseButton(mouseData, RawMouseButtonFlags.LeftButtonDown, RawMouseButtonFlags.LeftButtonUp, ref isDown))
                button = MouseButton.Left;
            // Middle
            else if (DetermineMouseButton(mouseData, RawMouseButtonFlags.MiddleButtonDown, RawMouseButtonFlags.MiddleButtonUp, ref isDown))
                button = MouseButton.Middle;
            // Right
            else if (DetermineMouseButton(mouseData, RawMouseButtonFlags.RightButtonDown, RawMouseButtonFlags.RightButtonUp, ref isDown))
                button = MouseButton.Right;
            // Button 4
            else if (DetermineMouseButton(mouseData, RawMouseButtonFlags.Button4Down, RawMouseButtonFlags.Button4Up, ref isDown))
                button = MouseButton.Button4;
            else if (DetermineMouseButton(mouseData, RawMouseButtonFlags.Button5Down, RawMouseButtonFlags.Button5Up, ref isDown))
                button = MouseButton.Button5;

            OnMouseButtonDataReceived(device, button, isDown);
        }

        private bool DetermineMouseButton(RawInputMouseData data, RawMouseButtonFlags downButton, RawMouseButtonFlags upButton, ref bool isDown)
        {
            if (data.Mouse.Buttons == downButton || data.Mouse.Buttons == upButton)
            {
                isDown = data.Mouse.Buttons == downButton;
                return true;
            }

            isDown = false;
            return false;
        }

        #endregion

        #region Native

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public readonly int X;
            public readonly int Y;
        }

        private static Win32Point GetCursorPosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return w32Mouse;
        }

        #endregion
    }
}