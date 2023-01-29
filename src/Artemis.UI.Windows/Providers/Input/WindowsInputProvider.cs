using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Windows.Utilities;
using Linearstar.Windows.RawInput;
using Linearstar.Windows.RawInput.Native;
using Serilog;

namespace Artemis.UI.Windows.Providers.Input;

public class WindowsInputProvider : InputProvider
{
    private const int WM_INPUT = 0x00FF;

    private readonly IInputService _inputService;
    private readonly ILogger _logger;
    private readonly SpongeWindow _sponge;
    private readonly Timer _taskManagerTimer;
    private int _lastProcessId;

    public WindowsInputProvider(ILogger logger, IInputService inputService)
    {
        _logger = logger;
        _inputService = inputService;

        _sponge = new SpongeWindow();
        _sponge.WndProcCalled += SpongeOnWndProcCalled;

        _taskManagerTimer = new Timer(500);
        _taskManagerTimer.Elapsed += TaskManagerTimerOnElapsed;
        _taskManagerTimer.Start();

        RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink, _sponge.Handle.Handle);
        RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, _sponge.Handle.Handle);
    }

    public static Guid Id { get; } = new("6737b204-ffb1-4cd9-8776-9fb851db303a");


    #region Overrides of InputProvider

    /// <inheritdoc />
    public override void OnKeyboardToggleStatusRequested()
    {
        UpdateToggleStatus();
    }

    #endregion

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sponge.Dispose();
            _taskManagerTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void SpongeOnWndProcCalled(object? sender, SpongeWindowEventArgs message)
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

    private void TaskManagerTimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        int processId = WindowUtilities.GetActiveProcessId();
        if (processId == _lastProcessId)
            return;

        _lastProcessId = processId;

        // If task manager has focus then we can't track keys properly, release everything to avoid them getting stuck
        // Same goes for Idle which is what you get when you press Ctrl+Alt+Del
        Process active = Process.GetProcessById(processId);
        if (active?.ProcessName == "Taskmgr" || active?.ProcessName == "Idle")
            _inputService.ReleaseAll();
    }

    #region Keyboard

    private void HandleKeyboardData(RawInputData data, RawInputKeyboardData keyboardData)
    {
        KeyboardKey key = InputUtilities.KeyFromVirtualKey(keyboardData.Keyboard.VirutalKey);
        // Debug.WriteLine($"VK: {key} ({keyboardData.Keyboard.VirutalKey}), Flags: {keyboardData.Keyboard.Flags}, Scan code: {keyboardData.Keyboard.ScanCode}");

        // Sometimes we get double hits and they resolve to None, ignore those
        if (key == KeyboardKey.None)
            return;

        // Right alt triggers LeftCtrl with a different scan code for some reason, ignore those
        if (key == KeyboardKey.LeftCtrl && keyboardData.Keyboard.ScanCode == 56)
            return;

        string? identifier = data.Device?.DevicePath;

        // Let the core know there is an identifier so it can store new identifications if applicable
        if (identifier != null)
            OnIdentifierReceived(identifier, InputDeviceType.Keyboard);

        ArtemisDevice? device = null;
        if (identifier != null)
            try
            {
                device = _inputService.GetDeviceByIdentifier(this, identifier, InputDeviceType.Keyboard);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to retrieve input device by its identifier");
            }

        // Duplicate keys with different positions can be identified by the LeftKey flag (even though its set of the key that's physically on the right)
        if (keyboardData.Keyboard.Flags == RawKeyboardFlags.KeyE0 || keyboardData.Keyboard.Flags == (RawKeyboardFlags.KeyE0 | RawKeyboardFlags.Up))
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
                      keyboardData.Keyboard.Flags != (RawKeyboardFlags.Up | RawKeyboardFlags.KeyE0) &&
                      keyboardData.Keyboard.Flags != (RawKeyboardFlags.Up | RawKeyboardFlags.KeyE1);

        OnKeyboardDataReceived(device, key, isDown);
        UpdateToggleStatus();
    }

    private void UpdateToggleStatus()
    {
        OnKeyboardToggleStatusReceived(new KeyboardToggleStatus(
            InputUtilities.IsKeyToggled(KeyboardKey.NumLock),
            InputUtilities.IsKeyToggled(KeyboardKey.CapsLock),
            InputUtilities.IsKeyToggled(KeyboardKey.Scroll)
        ));
    }

    #endregion

    #region Mouse

    private int _previousMouseX;
    private int _previousMouseY;

    private void HandleMouseData(RawInputData data, RawInputMouseData mouseData)
    {
        // Only submit mouse movement 25 times per second but increment the delta
        // This can create a small inaccuracy of course, but Artemis is not a shooter :')
        if (mouseData.Mouse.Buttons == RawMouseButtonFlags.None)
        {
            _previousMouseX += mouseData.Mouse.LastX;
            _previousMouseY += mouseData.Mouse.LastY;
        }

        ArtemisDevice? device = null;
        string? identifier = data.Device?.DevicePath;
        if (identifier != null)
            try
            {
                device = _inputService.GetDeviceByIdentifier(this, identifier, InputDeviceType.Mouse);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to retrieve input device by its identifier");
            }

        // Debug.WriteLine($"Buttons: {mouseData.Mouse.Buttons}, Data: {mouseData.Mouse.ButtonData}, Flags: {mouseData.Mouse.Flags}, XY: {mouseData.Mouse.LastX},{mouseData.Mouse.LastY}");

        // Movement
        if (mouseData.Mouse.Buttons == RawMouseButtonFlags.None)
        {
            Win32Point cursorPosition = GetCursorPosition();
            OnMouseMoveDataReceived(device, cursorPosition.X, cursorPosition.Y, cursorPosition.X - _previousMouseX, cursorPosition.Y - _previousMouseY);
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
        Win32Point w32Mouse = new();
        GetCursorPos(ref w32Mouse);

        return w32Mouse;
    }

    #endregion
}