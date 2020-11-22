using System;
using System.Collections.Generic;
using System.Linq;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Services
{
    internal class InputService : IInputService
    {
        private readonly ILogger _logger;
        private readonly ISurfaceService _surfaceService;
        private readonly List<InputProvider> _inputProviders;

        public InputService(ILogger logger, ISurfaceService surfaceService)
        {
            _logger = logger;
            _surfaceService = surfaceService;
            _inputProviders = new List<InputProvider>();
            _keyboardModifier = new Dictionary<ArtemisDevice, KeyboardModifierKeys>();
            _deviceCache = new Dictionary<Tuple<InputProvider, object>, ArtemisDevice>();
            _devices = new List<ArtemisDevice>();

            _surfaceService.ActiveSurfaceConfigurationSelected += SurfaceConfigurationChanged;
            _surfaceService.SurfaceConfigurationUpdated += SurfaceConfigurationChanged;
            BustIdentifierCache();
        }

        public void AddInputProvider(InputProvider inputProvider)
        {
            inputProvider.IdentifierReceived += InputProviderOnIdentifierReceived;
            inputProvider.KeyboardDataReceived += InputProviderOnKeyboardDataReceived;
            inputProvider.MouseButtonDataReceived += InputProviderOnMouseButtonDataReceived;
            inputProvider.MouseScrollDataReceived += InputProviderOnMouseScrollDataReceived;
            inputProvider.MouseMoveDataReceived += InputProviderOnMouseMoveDataReceived;
            _inputProviders.Add(inputProvider);
        }

        public void RemoveInputProvider(InputProvider inputProvider)
        {
            if (!_inputProviders.Contains(inputProvider))
                return;

            _inputProviders.Remove(inputProvider);
            inputProvider.IdentifierReceived -= InputProviderOnIdentifierReceived;
            inputProvider.KeyboardDataReceived -= InputProviderOnKeyboardDataReceived;
            inputProvider.MouseButtonDataReceived -= InputProviderOnMouseButtonDataReceived;
            inputProvider.MouseScrollDataReceived -= InputProviderOnMouseScrollDataReceived;
            inputProvider.MouseMoveDataReceived -= InputProviderOnMouseMoveDataReceived;
        }

        #region Identification

        private readonly Dictionary<Tuple<InputProvider, object>, ArtemisDevice> _deviceCache;
        private List<ArtemisDevice> _devices;
        private ArtemisDevice? _cachedFallbackKeyboard;
        private ArtemisDevice? _cachedFallbackMouse;
        private ArtemisDevice? _identifyingDevice;

        public void IdentifyDevice(ArtemisDevice device)
        {
            _identifyingDevice = device;
            _logger.Debug("Start identifying device {device}", device);
        }

        public void StopIdentify()
        {
            _logger.Debug("Stop identifying device {device}", _identifyingDevice);

            _identifyingDevice = null;
            _surfaceService.UpdateSurfaceConfiguration(_surfaceService.ActiveSurface, true);

            BustIdentifierCache();
        }

        public ArtemisDevice? GetDeviceByIdentifier(InputProvider provider, object identifier, InputFallbackDeviceType fallbackType)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (identifier == null) throw new ArgumentNullException(nameof(identifier));

            // Try cache first
            ArtemisDevice? cacheMatch = GetDeviceFromCache(provider, identifier);
            if (cacheMatch != null)
                return cacheMatch;

            string providerName = provider.GetType().FullName!;
            ArtemisDevice? match = _devices.FirstOrDefault(m => m.InputIdentifiers.Any(i => Equals(i.InputProvider,providerName) && Equals(i.Identifier, identifier)));

            // If a match was found cache it to speed up the next event and return the match
            if (match != null)
            {
                AddDeviceToCache(match, provider, identifier);
                return match;
            }

            // If there is no match, apply our fallback type
            if (fallbackType == InputFallbackDeviceType.None)
                return null;
            if (fallbackType == InputFallbackDeviceType.Keyboard)
            {
                if (_cachedFallbackKeyboard != null)
                    return _cachedFallbackKeyboard;
                _cachedFallbackKeyboard = _surfaceService.ActiveSurface.Devices.FirstOrDefault(d => d.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Keyboard);
                return _cachedFallbackKeyboard;
            }

            if (fallbackType == InputFallbackDeviceType.Mouse)
            {
                if (_cachedFallbackMouse != null)
                    return _cachedFallbackMouse;
                _cachedFallbackMouse = _surfaceService.ActiveSurface.Devices.FirstOrDefault(d => d.RgbDevice.DeviceInfo.DeviceType == RGBDeviceType.Mouse);
                return _cachedFallbackMouse;
            }

            return null;
        }

        public void BustIdentifierCache()
        {
            _deviceCache.Clear();
            _cachedFallbackKeyboard = null;
            _cachedFallbackMouse = null;

            _devices = _surfaceService.ActiveSurface.Devices.Where(d => d.InputIdentifiers.Any()).ToList();
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

        private void SurfaceConfigurationChanged(object? sender, SurfaceConfigurationEventArgs e)
        {
            BustIdentifierCache();
        }

        private void InputProviderOnIdentifierReceived(object? sender, object identifier)
        {
            if (_identifyingDevice == null)
                return;
            if (!(sender is InputProvider inputProvider)) 
                return;

            string providerName = inputProvider.GetType().FullName!;

            // Remove existing identification
            _identifyingDevice.InputIdentifiers.RemoveAll(i => i.InputProvider == providerName);
            _identifyingDevice.InputIdentifiers.Add(new ArtemisDeviceInputIdentifier(providerName, identifier));

            StopIdentify();
            OnDeviceIdentified();
        }

        #endregion

        #region Keyboard

        private readonly Dictionary<ArtemisDevice, KeyboardModifierKeys> _keyboardModifier;
        private KeyboardModifierKeys _globalModifiers;
        
        private void InputProviderOnKeyboardDataReceived(object? sender, InputProviderKeyboardEventArgs e)
        {
            KeyboardModifierKeys keyboardModifierKeys = UpdateModifierKeys(e.Device, e.Key, e.IsDown);

            // Get the LED - TODO: leverage a lookup
            bool foundLedId = InputKeyUtilities.KeyboardKeyLedIdMap.TryGetValue(e.Key, out LedId ledId);
            ArtemisLed? led = null;
            if (foundLedId && e.Device != null)
                led = e.Device.Leds.FirstOrDefault(l => l.RgbLed.Id == ledId);

            // Create the UpDown event args because it can be used for every event
            KeyboardKeyUpDownEventArgs eventArgs = new KeyboardKeyUpDownEventArgs(e.Device, led, e.Key, keyboardModifierKeys, e.IsDown);
            OnKeyboardKeyUpDown(eventArgs);
            if (e.IsDown)
                OnKeyboardKeyDown(eventArgs);
            else
                OnKeyboardKeyUp(eventArgs);

            _logger.Verbose("Keyboard data: LED ID: {ledId}, key: {key}, is down: {isDown}, modifiers: {modifiers}, device: {device} ", ledId, e.Key, e.IsDown, keyboardModifierKeys, e.Device);
        }

        private KeyboardModifierKeys UpdateModifierKeys(ArtemisDevice? device, KeyboardKey key, in bool isDown)
        {
            KeyboardModifierKeys modifiers = _globalModifiers;
            if (device != null)
                _keyboardModifier.TryGetValue(device, out modifiers);

            if (key == KeyboardKey.LeftAlt || key == KeyboardKey.RightAlt)
            {
                if (isDown)
                    modifiers = modifiers | KeyboardModifierKeys.Alt;
                else
                    modifiers = modifiers & ~KeyboardModifierKeys.Alt;
            }
            else if (key == KeyboardKey.LeftCtrl || key == KeyboardKey.RightCtrl)
            {
                if (isDown)
                    modifiers = modifiers | KeyboardModifierKeys.Control;
                else
                    modifiers = modifiers & ~KeyboardModifierKeys.Control;
            }
            else if (key == KeyboardKey.LeftShift || key == KeyboardKey.RightShift)
            {
                if (isDown)
                    modifiers = modifiers | KeyboardModifierKeys.Shift;
                else
                    modifiers = modifiers & ~KeyboardModifierKeys.Shift;
            }
            else if (key == KeyboardKey.LWin || key == KeyboardKey.RWin)
            {
                if (isDown)
                    modifiers = modifiers | KeyboardModifierKeys.Windows;
                else
                    modifiers = modifiers & ~KeyboardModifierKeys.Windows;
            }

            if (device != null)
                _keyboardModifier[device] = modifiers;
            else
                _globalModifiers = modifiers;

            return modifiers;
        }

        #endregion

        #region Mouse

        private void InputProviderOnMouseButtonDataReceived(object? sender, InputProviderMouseButtonEventArgs e)
        {
            bool foundLedId = InputKeyUtilities.MouseButtonLedIdMap.TryGetValue(e.Button, out LedId ledId);
            // _logger.Verbose("Mouse button data: LED ID: {ledId}, button: {button}, is down: {isDown}, device: {device} ", ledId, e.Button, e.IsDown, e.Device);
        }

        private void InputProviderOnMouseScrollDataReceived(object? sender, InputProviderMouseScrollEventArgs e)
        {
            // _logger.Verbose("Mouse scroll data: Direction: {direction}, delta: {delta}, device: {device} ", e.Direction, e.Delta, e.Device);
        }

        private void InputProviderOnMouseMoveDataReceived(object? sender, InputProviderMouseMoveEventArgs e)
        {
            // _logger.Verbose("Mouse move data: XY: {X},{Y} - delta XY: {deltaX},{deltaY} - device: {device} ", e.CursorX, e.CursorY, e.DeltaX, e.DeltaY, e.Device);
        }

        #endregion

        #region Events

        public event EventHandler<KeyboardKeyUpDownEventArgs>? KeyboardKeyUpDown;
        public event EventHandler<KeyboardEventArgs>? KeyboardKeyDown;
        public event EventHandler<KeyboardEventArgs>? KeyboardKeyUp;
        public event EventHandler? DeviceIdentified;

        protected virtual void OnKeyboardKeyUpDown(KeyboardKeyUpDownEventArgs e)
        {
            KeyboardKeyUpDown?.Invoke(this, e);
        }

        protected virtual void OnKeyboardKeyDown(KeyboardEventArgs e)
        {
            KeyboardKeyDown?.Invoke(this, e);
        }

        protected virtual void OnKeyboardKeyUp(KeyboardEventArgs e)
        {
            KeyboardKeyUp?.Invoke(this, e);
        }

        protected virtual void OnDeviceIdentified()
        {
            DeviceIdentified?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            while (_inputProviders.Any())
                RemoveInputProvider(_inputProviders.First());
        }

        #endregion
    }
}