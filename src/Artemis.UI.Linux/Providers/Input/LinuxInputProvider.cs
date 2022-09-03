using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Linux.Utilities;
using Serilog;

namespace Artemis.UI.Linux.Providers.Input;

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
            LinuxInputDeviceReader? reader = new(deviceDefinition);
            reader.InputEvent += OnInputEvent;
            _readers.Add(reader);
        }
    }

    #region Overrides of InputProvider

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            for (int i = _readers.Count - 1; i >= 0; i--)
            {
                _readers[i].InputEvent -= OnInputEvent;
                _readers[i].Dispose();
                _readers.RemoveAt(i);
            }

        base.Dispose(disposing);
    }

    #endregion

    private void OnInputEvent(object? sender, LinuxInputEventArgs e)
    {
        if (sender is not LinuxInputDeviceReader reader)
            return;

        switch (reader.InputDevice.DeviceType)
        {
            case LinuxDeviceType.Keyboard:
                HandleKeyboardData(reader.InputDevice, e);
                break;
            case LinuxDeviceType.Mouse:
                HandleMouseData(reader.InputDevice, e);
                break;
            case LinuxDeviceType.Gamepad:
                break;
        }
    }

    private void HandleKeyboardData(LinuxInputDevice keyboard, LinuxInputEventArgs args)
    {
        if (args.Type != LinuxInputEventType.KEY)
            return;

        KeyboardKey key = InputUtilities.KeyFromKeyCode((LinuxKeyboardKeyCodes) args.Code);
        bool isDown = args.Value != 0;

        //_logger.Verbose($"Keyboard Key: {(LinuxKeyboardKeyCodes)args.Code} | Down: {isDown}");

        LinuxInputDevice.LinuxInputId identifier = keyboard.InputId;
        OnIdentifierReceived(identifier, InputDeviceType.Keyboard);
        ArtemisDevice? device = null;

        try
        {
            device = _inputService.GetDeviceByIdentifier(this, identifier, InputDeviceType.Keyboard);
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to retrieve input device by its identifier");
        }

        OnKeyboardDataReceived(device, key, isDown);
    }

    private void HandleMouseData(LinuxInputDevice mouse, LinuxInputEventArgs args)
    {
        LinuxInputDevice.LinuxInputId identifier = mouse.InputId;
        OnIdentifierReceived(identifier, InputDeviceType.Mouse);
        ArtemisDevice? device = null;

        try
        {
            device = _inputService.GetDeviceByIdentifier(this, identifier, InputDeviceType.Mouse);
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to retrieve input device by its identifier");
        }

        switch (args.Type)
        {
            case LinuxInputEventType.KEY:
                LinuxKeyboardKeyCodes key = (LinuxKeyboardKeyCodes) args.Code;
                if (key == LinuxKeyboardKeyCodes.BTN_TOUCH ||
                    (key >= LinuxKeyboardKeyCodes.BTN_TOOL_PEN && key <= LinuxKeyboardKeyCodes.BTN_TOOL_QUADTAP))
                    //trackpad input, ignore.
                    return;

                //0 - up
                //1 - down
                //2 - repeat input
                bool isDown = args.Value == 1;
                MouseButton button = InputUtilities.MouseButtonFromButtonCode((LinuxKeyboardKeyCodes) args.Code);

                //_logger.Verbose($"Mouse Button: {(LinuxKeyboardKeyCodes)args.Code} | Down: {isDown}");

                OnMouseButtonDataReceived(device, button, isDown);
                break;

            case LinuxInputEventType.REL:
                LinuxRelativeAxis relativeAxis = (LinuxRelativeAxis) args.Code;

                //_logger.Verbose($"Relative mouse: axis={relativeAxis} | value={args.Value}");

                switch (relativeAxis)
                {
                    case LinuxRelativeAxis.REL_WHEEL:
                        OnMouseScrollDataReceived(device, MouseScrollDirection.Vertical, args.Value);
                        break;
                    case LinuxRelativeAxis.REL_HWHEEL:
                        OnMouseScrollDataReceived(device, MouseScrollDirection.Horizontal, args.Value);
                        break;
                    case LinuxRelativeAxis.REL_X:
                        OnMouseMoveDataReceived(device, 0, 0, args.Value, 0);
                        break;
                    case LinuxRelativeAxis.REL_Y:
                        OnMouseMoveDataReceived(device, 0, 0, 0, args.Value);
                        break;
                }

                break;
            case LinuxInputEventType.ABS:
                LinuxAbsoluteAxis absoluteAxis = (LinuxAbsoluteAxis) args.Code;

                //_logger.Verbose($"Absolute mouse: axis={absoluteAxis} | value={args.Value}");

                switch (absoluteAxis)
                {
                    case LinuxAbsoluteAxis.ABS_X:
                        OnMouseMoveDataReceived(device, args.Value, 0, 0, 0);
                        break;
                    case LinuxAbsoluteAxis.ABS_Y:
                        OnMouseMoveDataReceived(device, 0, args.Value, 0, 0);
                        break;
                }

                break;
            case LinuxInputEventType.SYN:
                //ignore
                break;
        }
    }
}