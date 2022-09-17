﻿using System;
using System.Collections.Generic;
using System.Linq;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Services;

internal class InputService : IInputService
{
    private readonly ILogger _logger;
    private readonly IRgbService _rgbService;
    private ArtemisDevice? _firstKeyboard;
    private ArtemisDevice? _firstMouse;
    private int _keyboardCount;
    private int _mouseCount;

    public InputService(ILogger logger, IRgbService rgbService)
    {
        _logger = logger;
        _rgbService = rgbService;

        _rgbService.DeviceAdded += RgbServiceOnDevicesModified;
        _rgbService.DeviceRemoved += RgbServiceOnDevicesModified;
        BustIdentifierCache();
    }

    protected virtual void OnKeyboardKeyUpDown(ArtemisKeyboardKeyUpDownEventArgs e)
    {
        KeyboardKeyUpDown?.Invoke(this, e);
    }

    protected virtual void OnKeyboardKeyDown(ArtemisKeyboardKeyEventArgs e)
    {
        KeyboardKeyDown?.Invoke(this, e);
    }

    protected virtual void OnKeyboardKeyUp(ArtemisKeyboardKeyEventArgs e)
    {
        KeyboardKeyUp?.Invoke(this, e);
    }

    protected virtual void OnKeyboardToggleStatusChanged(ArtemisKeyboardToggleStatusArgs e)
    {
        KeyboardToggleStatusChanged?.Invoke(this, e);
    }

    protected virtual void OnMouseButtonUpDown(ArtemisMouseButtonUpDownEventArgs e)
    {
        MouseButtonUpDown?.Invoke(this, e);
    }

    protected virtual void OnMouseButtonDown(ArtemisMouseButtonEventArgs e)
    {
        MouseButtonDown?.Invoke(this, e);
    }

    protected virtual void OnMouseButtonUp(ArtemisMouseButtonEventArgs e)
    {
        MouseButtonUp?.Invoke(this, e);
    }

    protected virtual void OnMouseScroll(ArtemisMouseScrollEventArgs e)
    {
        MouseScroll?.Invoke(this, e);
    }

    protected virtual void OnMouseMove(ArtemisMouseMoveEventArgs e)
    {
        MouseMove?.Invoke(this, e);
    }

    protected virtual void OnDeviceIdentified()
    {
        DeviceIdentified?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        while (_inputProviders.Any())
        {
            InputProvider provider = _inputProviders.First();
            RemoveInputProvider(provider);
            provider.Dispose();
        }
    }

    public event EventHandler<ArtemisKeyboardKeyUpDownEventArgs>? KeyboardKeyUpDown;
    public event EventHandler<ArtemisKeyboardKeyEventArgs>? KeyboardKeyDown;
    public event EventHandler<ArtemisKeyboardKeyEventArgs>? KeyboardKeyUp;
    public event EventHandler<ArtemisKeyboardToggleStatusArgs>? KeyboardToggleStatusChanged;

    public event EventHandler<ArtemisMouseButtonUpDownEventArgs>? MouseButtonUpDown;
    public event EventHandler<ArtemisMouseButtonEventArgs>? MouseButtonDown;
    public event EventHandler<ArtemisMouseButtonEventArgs>? MouseButtonUp;
    public event EventHandler<ArtemisMouseScrollEventArgs>? MouseScroll;
    public event EventHandler<ArtemisMouseMoveEventArgs>? MouseMove;
    public event EventHandler? DeviceIdentified;

    #region Providers

    private readonly List<InputProvider> _inputProviders = new();

    public KeyboardToggleStatus KeyboardToggleStatus { get; private set; } = new(false, false, false);

    public void AddInputProvider(InputProvider inputProvider)
    {
        inputProvider.IdentifierReceived += InputProviderOnIdentifierReceived;
        inputProvider.KeyboardDataReceived += InputProviderOnKeyboardDataReceived;
        inputProvider.KeyboardToggleStatusReceived += InputProviderOnKeyboardToggleStatusReceived;
        inputProvider.MouseButtonDataReceived += InputProviderOnMouseButtonDataReceived;
        inputProvider.MouseScrollDataReceived += InputProviderOnMouseScrollDataReceived;
        inputProvider.MouseMoveDataReceived += InputProviderOnMouseMoveDataReceived;
        _inputProviders.Add(inputProvider);

        inputProvider.OnKeyboardToggleStatusRequested();
    }


    public void RemoveInputProvider(InputProvider inputProvider)
    {
        if (!_inputProviders.Contains(inputProvider))
            return;

        _inputProviders.Remove(inputProvider);
        inputProvider.IdentifierReceived -= InputProviderOnIdentifierReceived;
        inputProvider.KeyboardDataReceived -= InputProviderOnKeyboardDataReceived;
        inputProvider.KeyboardToggleStatusReceived -= InputProviderOnKeyboardToggleStatusReceived;
        inputProvider.MouseButtonDataReceived -= InputProviderOnMouseButtonDataReceived;
        inputProvider.MouseScrollDataReceived -= InputProviderOnMouseScrollDataReceived;
        inputProvider.MouseMoveDataReceived -= InputProviderOnMouseMoveDataReceived;
    }

    #endregion

    #region Identification

    private readonly Dictionary<Tuple<InputProvider, object>, ArtemisDevice> _deviceCache = new();
    private List<ArtemisDevice> _devices = new();
    private ArtemisDevice? _identifyingDevice;

    public void IdentifyDevice(ArtemisDevice device)
    {
        if (device.DeviceType != RGBDeviceType.Keyboard && device.DeviceType != RGBDeviceType.Mouse)
            throw new ArtemisCoreException($"Cannot initialize input-identification for a device of type {device.DeviceType}. \r\n" +
                                           "Only keyboard and mouse is supported.");

        _identifyingDevice = device;
        _logger.Debug("Start identifying device {device}", device);
    }

    public void StopIdentify()
    {
        if (_identifyingDevice == null)
            return;

        _logger.Debug("Stop identifying device {device}", _identifyingDevice);
        _identifyingDevice = null;
        _rgbService.SaveDevices();

        BustIdentifierCache();
    }

    // TODO: Move the OnIdentifierReceived logic into here and get rid of OnIdentifierReceived, this and OnIdentifierReceived are always called in combination with each other
    public ArtemisDevice? GetDeviceByIdentifier(InputProvider provider, object identifier, InputDeviceType type)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        if (identifier == null) throw new ArgumentNullException(nameof(identifier));

        // We will almost always only have zero or one of each
        if (type == InputDeviceType.Keyboard)
        {
            if (_keyboardCount == 0)
                return null;
            if (_keyboardCount == 1)
                return _firstKeyboard;
        }

        if (type == InputDeviceType.Mouse)
        {
            if (_mouseCount == 0)
                return null;
            if (_mouseCount == 1)
                return _firstMouse;
        }

        // Try cache first
        ArtemisDevice? cacheMatch = GetDeviceFromCache(provider, identifier);
        if (cacheMatch != null)
            return cacheMatch;

        ArtemisDevice? match = _devices.FirstOrDefault(m => m.InputIdentifiers.Any(i => Equals(i.InputProvider, provider.ProviderName) && Equals(i.Identifier, identifier)));

        // If a match was found cache it to speed up the next event and return the match
        if (match != null)
        {
            AddDeviceToCache(match, provider, identifier);
            return match;
        }

        if (type == InputDeviceType.Keyboard)
            return _firstKeyboard;
        if (type == InputDeviceType.Mouse)
            return _firstMouse;
        return null;
    }

    public void BustIdentifierCache()
    {
        _deviceCache.Clear();
        _devices = _rgbService.EnabledDevices.Where(d => d.InputIdentifiers.Any()).ToList();
    }

    private void AddDeviceToCache(ArtemisDevice match, InputProvider provider, object identifier)
    {
        _deviceCache.TryAdd(new Tuple<InputProvider, object>(provider, identifier), match);
    }

    private ArtemisDevice? GetDeviceFromCache(InputProvider provider, object identifier)
    {
        _deviceCache.TryGetValue(new Tuple<InputProvider, object>(provider, identifier), out ArtemisDevice? device);
        return device;
    }
    
    private void InputProviderOnIdentifierReceived(object? sender, InputProviderIdentifierEventArgs e)
    {
        // Don't match if there is no device or if the device type differs from the event device type
        if (_identifyingDevice == null ||
            (_identifyingDevice.DeviceType == RGBDeviceType.Keyboard && e.DeviceType == InputDeviceType.Mouse) ||
            (_identifyingDevice.DeviceType == RGBDeviceType.Mouse && e.DeviceType == InputDeviceType.Keyboard))
            return;
        if (!(sender is InputProvider inputProvider))
            return;

        // Remove existing identification
        _identifyingDevice.InputIdentifiers.RemoveAll(i => i.InputProvider == inputProvider.ProviderName);
        _identifyingDevice.InputIdentifiers.Add(new ArtemisDeviceInputIdentifier(inputProvider.ProviderName, e.Identifier));

        StopIdentify();
        OnDeviceIdentified();
    }

    private void RgbServiceOnDevicesModified(object? sender, DeviceEventArgs args)
    {
        _firstKeyboard = _rgbService.Devices.FirstOrDefault(d => d.DeviceType == RGBDeviceType.Keyboard);
        _firstMouse = _rgbService.Devices.FirstOrDefault(d => d.DeviceType == RGBDeviceType.Mouse);
        _keyboardCount = _rgbService.Devices.Count(d => d.DeviceType == RGBDeviceType.Keyboard);
        _mouseCount = _rgbService.Devices.Count(d => d.DeviceType == RGBDeviceType.Mouse);

        BustIdentifierCache();
    }

    #endregion

    #region Keyboard

    private readonly Dictionary<ArtemisDevice, HashSet<KeyboardKey>> _pressedKeys = new();
    private readonly Dictionary<ArtemisDevice, KeyboardModifierKey> _keyboardModifier = new();
    private KeyboardModifierKey _globalModifiers;

    private void InputProviderOnKeyboardDataReceived(object? sender, InputProviderKeyboardEventArgs e)
    {
        KeyboardModifierKey keyboardModifierKey = UpdateModifierKeys(e.Device, e.Key, e.IsDown);

        // if UpdatePressedKeys is true, the key is already pressed, then we skip this event
        if (UpdatePressedKeys(e.Device, e.Key, e.IsDown))
            return;

        // Get the LED
        bool foundLedId = InputKeyUtilities.KeyboardKeyLedIdMap.TryGetValue(e.Key, out LedId ledId);
        ArtemisLed? led = null;
        if (foundLedId && e.Device != null)
            led = e.Device.GetLed(ledId, true);

        // Create the UpDown event args because it can be used for every event
        ArtemisKeyboardKeyUpDownEventArgs eventArgs = new(e.Device, led, e.Key, keyboardModifierKey, e.IsDown);
        OnKeyboardKeyUpDown(eventArgs);
        if (e.IsDown)
            OnKeyboardKeyDown(eventArgs);
        else
            OnKeyboardKeyUp(eventArgs);

        // _logger.Verbose("Keyboard data: LED ID: {ledId}, key: {key}, is down: {isDown}, modifiers: {modifiers}, device: {device} ", ledId, e.Key, e.IsDown, keyboardModifierKey, e.Device);
    }

    private void InputProviderOnKeyboardToggleStatusReceived(object? sender, InputProviderKeyboardToggleEventArgs e)
    {
        KeyboardToggleStatus old = KeyboardToggleStatus;
        if (KeyboardToggleStatus.CapsLock == e.KeyboardToggleStatus.CapsLock &&
            KeyboardToggleStatus.NumLock == e.KeyboardToggleStatus.NumLock &&
            KeyboardToggleStatus.ScrollLock == e.KeyboardToggleStatus.ScrollLock)
            return;

        KeyboardToggleStatus = e.KeyboardToggleStatus;
        OnKeyboardToggleStatusChanged(new ArtemisKeyboardToggleStatusArgs(old, KeyboardToggleStatus));
    }

    private bool UpdatePressedKeys(ArtemisDevice? device, KeyboardKey key, bool isDown)
    {
        if (device != null)
        {
            // Ensure the device is in the dictionary
            _pressedKeys.TryAdd(device, new HashSet<KeyboardKey>());
            // Get the hash set of the device
            HashSet<KeyboardKey> pressedDeviceKeys = _pressedKeys[device];
            // See if the key is already pressed
            bool alreadyPressed = pressedDeviceKeys.Contains(key);

            // Prevent triggering already down keys again. When a key is held, we don't want to spam events like Windows does
            if (isDown && alreadyPressed)
                return true;

            // Either add or remove the key depending on its status
            if (isDown && !alreadyPressed)
                pressedDeviceKeys.Add(key);
            else if (!isDown && alreadyPressed)
                pressedDeviceKeys.Remove(key);
        }

        return false;
    }

    private KeyboardModifierKey UpdateModifierKeys(ArtemisDevice? device, KeyboardKey key, in bool isDown)
    {
        KeyboardModifierKey modifiers = _globalModifiers;
        if (device != null)
            _keyboardModifier.TryGetValue(device, out modifiers);

        if (key == KeyboardKey.LeftAlt || key == KeyboardKey.RightAlt)
        {
            if (isDown)
                modifiers |= KeyboardModifierKey.Alt;
            else
                modifiers &= ~KeyboardModifierKey.Alt;
        }
        else if (key == KeyboardKey.LeftCtrl || key == KeyboardKey.RightCtrl)
        {
            if (isDown)
                modifiers |= KeyboardModifierKey.Control;
            else
                modifiers &= ~KeyboardModifierKey.Control;
        }
        else if (key == KeyboardKey.LeftShift || key == KeyboardKey.RightShift)
        {
            if (isDown)
                modifiers |= KeyboardModifierKey.Shift;
            else
                modifiers &= ~KeyboardModifierKey.Shift;
        }
        else if (key == KeyboardKey.LWin || key == KeyboardKey.RWin)
        {
            if (isDown)
                modifiers |= KeyboardModifierKey.Windows;
            else
                modifiers &= ~KeyboardModifierKey.Windows;
        }

        if (device != null)
            _keyboardModifier[device] = modifiers;
        else
            _globalModifiers = modifiers;

        return modifiers;
    }

    public void ReleaseAll()
    {
        foreach ((ArtemisDevice? device, HashSet<KeyboardKey>? keys) in _pressedKeys.ToList())
        {
            foreach (KeyboardKey keyboardKey in keys)
                InputProviderOnKeyboardDataReceived(this, new InputProviderKeyboardEventArgs(device, keyboardKey, false));
        }
    }

    public bool IsKeyDown(KeyboardKey key)
    {
        return _pressedKeys.Any(p => p.Value.Contains(key));
    }

    #endregion

    #region Mouse

    private readonly HashSet<MouseButton> _pressedButtons = new();


    private void InputProviderOnMouseButtonDataReceived(object? sender, InputProviderMouseButtonEventArgs e)
    {
        bool foundLedId = InputKeyUtilities.MouseButtonLedIdMap.TryGetValue(e.Button, out LedId ledId);
        ArtemisLed? led = null;
        if (foundLedId && e.Device != null)
            led = e.Device.Leds.FirstOrDefault(l => l.RgbLed.Id == ledId);

        // Create the UpDown event args because it can be used for every event
        ArtemisMouseButtonUpDownEventArgs eventArgs = new(e.Device, led, e.Button, e.IsDown);
        OnMouseButtonUpDown(eventArgs);
        if (e.IsDown)
        {
            if (!_pressedButtons.Contains(e.Button))
                _pressedButtons.Add(e.Button);
            OnMouseButtonDown(eventArgs);
        }
        else
        {
            if (_pressedButtons.Contains(e.Button))
                _pressedButtons.Remove(e.Button);
            OnMouseButtonUp(eventArgs);
        }

        // _logger.Verbose("Mouse button data: LED ID: {ledId}, button: {button}, is down: {isDown}, device: {device} ", ledId, e.Button, e.IsDown, e.Device);
    }

    private void InputProviderOnMouseScrollDataReceived(object? sender, InputProviderMouseScrollEventArgs e)
    {
        OnMouseScroll(new ArtemisMouseScrollEventArgs(e.Device, e.Direction, e.Delta));
        // _logger.Verbose("Mouse scroll data: Direction: {direction}, delta: {delta}, device: {device} ", e.Direction, e.Delta, e.Device);
    }

    private void InputProviderOnMouseMoveDataReceived(object? sender, InputProviderMouseMoveEventArgs e)
    {
        OnMouseMove(new ArtemisMouseMoveEventArgs(e.Device, e.CursorX, e.CursorY, e.DeltaX, e.DeltaY));
        // _logger.Verbose("Mouse move data: XY: {X},{Y} - delta XY: {deltaX},{deltaY} - device: {device} ", e.CursorX, e.CursorY, e.DeltaX, e.DeltaY, e.Device);
    }

    public bool IsButtonDown(MouseButton button)
    {
        return _pressedButtons.Contains(button);
    }

    #endregion
}