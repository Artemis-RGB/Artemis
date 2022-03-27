using System;
using System.Collections.Generic;
using Artemis.Core.Services;
using Artemis.UI.Linux.Utilities;
using Serilog;

namespace Artemis.UI.Linux.Providers.Input
{
    public class LinuxInputProvider : InputProvider
    {
        private readonly IInputService _inputService;
        private readonly ILogger _logger;
        private readonly List<LinuxInputDeviceReader> _readers;

        public LinuxInputProvider(ILogger logger, IInputService inputService)
        {
            _logger = logger;
            _inputService = inputService;
            _readers = new List<LinuxInputDeviceReader>();

            foreach (LinuxInputDevice deviceDefinition in LinuxInputDeviceFinder.Find())
            {
                LinuxInputDeviceReader? reader = new LinuxInputDeviceReader(deviceDefinition);
                reader.InputEvent += OnInputEvent;
                _readers.Add(reader);
            }
        }

        private void OnInputEvent(object? sender, LinuxInputEventArgs e)
        {
            if (sender is not LinuxInputDeviceReader reader)
                return;

            if (reader.InputDevice.IsKeyboard)
            {
                HandleKeyboardData(reader.InputDevice, e);
            }
            else if (reader.InputDevice.IsMouse)
            {
                HandleMouseData(reader.InputDevice, e);
            }
            else if (reader.InputDevice.IsGamePad)
            {
                //TODO: handle game pad input?
            }
        }

        private void HandleKeyboardData(LinuxInputDevice keyboard, LinuxInputEventArgs e)
        {
            switch (e.Type)
            {
                case LinuxInputEventType.KEY:
                    KeyboardKey key = InputUtilities.KeyFromKeyCode((LinuxKeyboardKeyCodes) e.Code);
                    bool isDown = e.Value != 0;

                    _logger.Verbose($"Keyboard Key: {(LinuxKeyboardKeyCodes) e.Code} | Down: {isDown}");
                    
                    //TODO: identify

                    OnKeyboardDataReceived(null, key, isDown);
                    break;
                default:
                    _logger.Verbose($"Unknown keyboard event type: {e.Type}");
                    break;
            }
        }

        private void HandleMouseData(LinuxInputDevice mouse, LinuxInputEventArgs e)
        {
            switch (e.Type)
            {
                case LinuxInputEventType.KEY:
                    bool isDown = e.Value != 0;
                    MouseButton button = InputUtilities.MouseButtonFromButtonCode((LinuxKeyboardKeyCodes)e.Code);

                    _logger.Verbose($"Mouse Button: {(LinuxKeyboardKeyCodes) e.Code} | Down: {isDown}");

                    //TODO: identify

                    OnMouseButtonDataReceived(null, button, isDown);
                    break;

                case LinuxInputEventType.ABS:
                    LinuxAbsoluteAxis absoluteAxis = (LinuxAbsoluteAxis) e.Code;
                    _logger.Verbose($"Absolute mouse: axis={absoluteAxis} | value={e.Value}");
                    break;
                case LinuxInputEventType.REL:
                    LinuxRelativeAxis relativeAxis = (LinuxRelativeAxis) e.Code;
                    _logger.Verbose($"Relative mouse: axis={relativeAxis} | value={e.Value}");

                    //TODO: handle mouse movement
                    break;
                default:
                    _logger.Verbose($"Unknown mouse event type: {e.Type}");
                    break;
            }
        }

        #region Overrides of InputProvider

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = _readers.Count - 1; i >= 0; i--)
                {
                    _readers[i].InputEvent -= OnInputEvent;
                    _readers[i].Dispose();
                    _readers.RemoveAt(i);
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}