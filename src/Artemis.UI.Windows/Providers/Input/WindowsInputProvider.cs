using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Windows.Extensions;
using Artemis.UI.Windows.Utilities;
using Avalonia.Controls.Platform;
using Avalonia.Platform;
using Linearstar.Windows.RawInput;
using Linearstar.Windows.RawInput.Native;
using Serilog;

namespace Artemis.UI.Windows.Providers.Input;

public class WindowsInputProvider : InputProvider
{
    private const int GWL_WNDPROC = -4;
    private const int WM_INPUT = 0x00FF;

    private readonly IWindowImpl _window;
    private readonly nint _hWndProcHook;
    private readonly WndProc? _fnWndProcHook;
    private readonly IInputService _inputService;
    private readonly ILogger _logger;
    private readonly Timer _taskManagerTimer;

    private int _lastProcessId;
    delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);

    private nint CustomWndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        OnWndProcCalled(hWnd, msg, wParam, lParam);
        return CallWindowProc(_hWndProcHook, hWnd, msg, wParam, lParam);
    }

    public WindowsInputProvider(ILogger logger, IInputService inputService)
    {
        _logger = logger;
        _inputService = inputService;

        _taskManagerTimer = new Timer(500);
        _taskManagerTimer.Elapsed += TaskManagerTimerOnElapsed;
        _taskManagerTimer.Start();

        _window = PlatformManager.CreateWindow();

        IPlatformHandle handle = _window.GetHandle(); 
        _hWndProcHook = GetWindowLongPtr(handle.Handle, GWL_WNDPROC);
        _fnWndProcHook = CustomWndProc;
        nint newLong = Marshal.GetFunctionPointerForDelegate(_fnWndProcHook);
        SetWindowLongPtr(handle.Handle, GWL_WNDPROC, newLong);

        RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink, handle.Handle);
        RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, RawInputDeviceFlags.InputSink, handle.Handle);
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
            _taskManagerTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnWndProcCalled(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg != WM_INPUT)
            return;

        RawInputData data = RawInputData.FromHandle(lParam);
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
        KeyboardKey key = KeyboardKey.None;
        try
        {
            key = InputUtilities.CorrectVirtualKeyAndScanCode(keyboardData.Keyboard.VirutalKey, keyboardData.Keyboard.ScanCode, (uint)keyboardData.Keyboard.Flags);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to convert virtual key to Artemis key, please share this log with the developers. ScanCode: {scanCode} VK: {virtualKey} Flags: {flags}",
        keyboardData.Keyboard.ScanCode, keyboardData.Keyboard.VirutalKey, keyboardData.Keyboard.Flags);
        }
        // Debug.WriteLine($"VK: {key} ({keyboardData.Keyboard.VirutalKey}), Flags: {keyboardData.Keyboard.Flags}, Scan code: {keyboardData.Keyboard.ScanCode}");

        if (key == KeyboardKey.None)
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

        bool isDown = (keyboardData.Keyboard.Flags & RawKeyboardFlags.Up) == 0;

        OnKeyboardDataReceived(device, key, isDown);
        UpdateToggleStatus();
    }

    private void UpdateToggleStatus()
    {
        OnKeyboardToggleStatusReceived(new KeyboardToggleStatus(
            InputUtilities.IsKeyToggled(KeyboardKey.NumLock),
            InputUtilities.IsKeyToggled(KeyboardKey.CapsLock),
            InputUtilities.IsKeyToggled(KeyboardKey.ScrollLock)
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
        (MouseButton button, bool isDown) =  mouseData.Mouse.Buttons switch
        {
            RawMouseButtonFlags.LeftButtonDown => (MouseButton.Left, true),
            RawMouseButtonFlags.LeftButtonUp => (MouseButton.Left, false),
            RawMouseButtonFlags.MiddleButtonDown => (MouseButton.Middle, true),
            RawMouseButtonFlags.MiddleButtonUp => (MouseButton.Middle, false),
            RawMouseButtonFlags.RightButtonDown => (MouseButton.Right, true),
            RawMouseButtonFlags.RightButtonUp => (MouseButton.Right, false),
            RawMouseButtonFlags.Button4Down => (MouseButton.Button4, true),
            RawMouseButtonFlags.Button4Up => (MouseButton.Button4, false),
            RawMouseButtonFlags.Button5Down => (MouseButton.Button5, true),
            RawMouseButtonFlags.Button5Up => (MouseButton.Button5, false),
            _ => (MouseButton.Left, false)
        };

        OnMouseButtonDataReceived(device, button, isDown);
    }

    #endregion

    #region Native

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr CallWindowProc(nint lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Unicode)]
    private static extern IntPtr SetWindowLongPtr(nint hWnd, int nIndex, IntPtr dwNewLong);

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