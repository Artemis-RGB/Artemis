using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services.Models;
using Artemis.Storage.Entities.Surface;
using Artemis.Storage.Repositories.Interfaces;
using RGB.NET.Core;
using RGB.NET.Groups;
using Serilog;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides wrapped access the RGB.NET
    /// </summary>
    internal class RgbService : IRgbService
    {
        private readonly List<ArtemisDevice> _devices;
        private readonly List<ArtemisDevice> _enabledDevices;
        private Dictionary<Led, ArtemisLed> _ledMap;

        private readonly ILogger _logger;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IDeviceRepository _deviceRepository;
        private readonly PluginSetting<double> _renderScaleSetting;
        private readonly PluginSetting<int> _targetFrameRateSetting;
        private readonly PluginSetting<int> _sampleSizeSetting;
        private ListLedGroup? _surfaceLedGroup;
        private bool _modifyingProviders;

        public RgbService(ILogger logger, ISettingsService settingsService, IPluginManagementService pluginManagementService, IDeviceRepository deviceRepository)
        {
            _logger = logger;
            _pluginManagementService = pluginManagementService;
            _deviceRepository = deviceRepository;
            _renderScaleSetting = settingsService.GetSetting("Core.RenderScale", 0.5);
            _targetFrameRateSetting = settingsService.GetSetting("Core.TargetFrameRate", 25);
            _sampleSizeSetting = settingsService.GetSetting("Core.SampleSize", 1);

            Surface = new RGBSurface();

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;
            _renderScaleSetting.SettingChanged += RenderScaleSettingOnSettingChanged;
            _targetFrameRateSetting.SettingChanged += TargetFrameRateSettingOnSettingChanged;
            _enabledDevices = new List<ArtemisDevice>();
            _devices = new List<ArtemisDevice>();
            _ledMap = new Dictionary<Led, ArtemisLed>();

            UpdateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / _targetFrameRateSetting.Value};
            Surface.RegisterUpdateTrigger(UpdateTrigger);
        }

        public IReadOnlyCollection<ArtemisDevice> EnabledDevices => _enabledDevices.AsReadOnly();
        public IReadOnlyCollection<ArtemisDevice> Devices => _devices.AsReadOnly();
        public IReadOnlyDictionary<Led, ArtemisLed> LedMap => new ReadOnlyDictionary<Led, ArtemisLed>(_ledMap);

        /// <inheritdoc />
        public RGBSurface Surface { get; set; }

        public TimerUpdateTrigger UpdateTrigger { get; }
        public BitmapBrush? BitmapBrush { get; private set; }

        public bool IsRenderPaused { get; set; }

        public void AddDeviceProvider(IRGBDeviceProvider deviceProvider)
        {
            lock (_devices)
            {
                try
                {
                    _modifyingProviders = true;

                    List<ArtemisDevice> toRemove = _devices.Where(a => deviceProvider.Devices.Any(d => a.RgbDevice == d)).ToList();
                    Surface.Detach(deviceProvider.Devices);
                    foreach (ArtemisDevice device in toRemove)
                        RemoveDevice(device);

                    deviceProvider.Initialize(RGBDeviceType.All, true);
                    Surface.Attach(deviceProvider.Devices);

                    if (!deviceProvider.Devices.Any())
                    {
                        _logger.Warning("Device provider {deviceProvider} has no devices", deviceProvider.GetType().Name);
                        return;
                    }

                    foreach (IRGBDevice rgbDevice in deviceProvider.Devices)
                    {
                        ArtemisDevice artemisDevice = GetArtemisDevice(rgbDevice);
                        AddDevice(artemisDevice);
                        _logger.Debug("Device provider {deviceProvider} added {deviceName}", deviceProvider.GetType().Name, rgbDevice.DeviceInfo?.DeviceName);
                    }

                    _devices.Sort((a, b) => a.ZIndex - b.ZIndex);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Exception during device loading for device provider {deviceProvider}", deviceProvider.GetType().Name);
                    throw;
                }
                finally
                {
                    _modifyingProviders = false;
                    UpdateBitmapBrush();
                }
            }
        }

        public void RemoveDeviceProvider(IRGBDeviceProvider deviceProvider)
        {
            lock (_devices)
            {
                try
                {
                    _modifyingProviders = true;

                    List<ArtemisDevice> toRemove = _devices.Where(a => deviceProvider.Devices.Any(d => a.RgbDevice == d)).ToList();
                    Surface.Detach(deviceProvider.Devices);
                    foreach (ArtemisDevice device in toRemove)
                        RemoveDevice(device);

                    _devices.Sort((a, b) => a.ZIndex - b.ZIndex);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Exception during device removal for device provider {deviceProvider}", deviceProvider.GetType().Name);
                    throw;
                }
                finally
                {
                    _modifyingProviders = false;
                    UpdateBitmapBrush();
                }
            }
        }

        private void UpdateBitmapBrush()
        {
            lock (_devices)
            {
                if (_modifyingProviders)
                    return;

                _ledMap = new Dictionary<Led, ArtemisLed>(_devices.SelectMany(d => d.Leds).ToDictionary(l => l.RgbLed));

                if (_surfaceLedGroup == null || BitmapBrush == null)
                {
                    // Apply the application wide brush and decorator
                    BitmapBrush = new BitmapBrush(new Scale(_renderScaleSetting.Value), _sampleSizeSetting, this);
                    _surfaceLedGroup = new ListLedGroup(Surface, LedMap.Select(l => l.Key)) {Brush = BitmapBrush};
                    OnLedsChanged();
                    return;
                }

                lock (_surfaceLedGroup)
                {
                    // Clean up the old background
                    _surfaceLedGroup.Detach(Surface);

                    // Apply the application wide brush and decorator
                    BitmapBrush.Scale = new Scale(_renderScaleSetting.Value);
                    _surfaceLedGroup = new ListLedGroup(Surface, LedMap.Select(l => l.Key)) {Brush = BitmapBrush};
                    OnLedsChanged();
                }
            }
        }

        #region IDisposable

        public void Dispose()
        {
            Surface.UnregisterUpdateTrigger(UpdateTrigger);

            UpdateTrigger.Dispose();
            Surface.Dispose();
        }

        #endregion

        #region EnabledDevices

        public void AutoArrangeDevices()
        {
            SurfaceArrangement surfaceArrangement = SurfaceArrangement.GetDefaultArrangement();
            surfaceArrangement.Arrange(_devices);
            SaveDevices();
        }

        public ArtemisLayout ApplyBestDeviceLayout(ArtemisDevice device)
        {
            // Look for a user layout
            // ... here

            // Try loading a device provider layout, if that comes back valid we use that
            ArtemisLayout layout = device.DeviceProvider.LoadLayout(device);

            // Finally fall back to a default layout
            // .. do it!

            ApplyDeviceLayout(device, layout);
            return layout;
        }

        public void ApplyDeviceLayout(ArtemisDevice device, ArtemisLayout layout)
        {
            device.ApplyLayout(layout);
            // Applying layouts can affect LEDs, update LED group
            UpdateBitmapBrush();
        }

        public ArtemisDevice? GetDevice(IRGBDevice rgbDevice)
        {
            return _devices.FirstOrDefault(d => d.RgbDevice == rgbDevice);
        }

        public ArtemisLed? GetLed(Led led)
        {
            LedMap.TryGetValue(led, out ArtemisLed? artemisLed);
            return artemisLed;
        }

        public void EnableDevice(ArtemisDevice device)
        {
            if (device.IsEnabled)
                return;

            _enabledDevices.Add(device);
            device.IsEnabled = true;
            device.ApplyToEntity();
            _deviceRepository.Save(device.DeviceEntity);

            UpdateBitmapBrush();
            OnDeviceAdded(new DeviceEventArgs(device));
        }

        public void DisableDevice(ArtemisDevice device)
        {
            if (!device.IsEnabled)
                return;

            _enabledDevices.Remove(device);
            device.IsEnabled = false;
            device.ApplyToEntity();
            _deviceRepository.Save(device.DeviceEntity);

            UpdateBitmapBrush();
            OnDeviceRemoved(new DeviceEventArgs(device));
        }

        private void AddDevice(ArtemisDevice device)
        {
            if (_devices.Any(d => d.RgbDevice == device.RgbDevice))
                throw new ArtemisCoreException("Attempted to add a duplicate device to the RGB Service");

            device.ApplyToRgbDevice();
            _devices.Add(device);
            if (device.IsEnabled)
                _enabledDevices.Add(device);

            // Will call UpdateBitmapBrush()
            ApplyBestDeviceLayout(device);
            OnDeviceAdded(new DeviceEventArgs(device));
        }

        private void RemoveDevice(ArtemisDevice device)
        {
            _devices.Remove(device);
            if (device.IsEnabled)
                _enabledDevices.Remove(device);

            UpdateBitmapBrush();
            OnDeviceRemoved(new DeviceEventArgs(device));
        }

        #endregion

        #region Storage

        private ArtemisDevice GetArtemisDevice(IRGBDevice rgbDevice)
        {
            string deviceIdentifier = rgbDevice.GetDeviceIdentifier();
            DeviceEntity? deviceEntity = _deviceRepository.Get(deviceIdentifier);
            DeviceProvider deviceProvider = _pluginManagementService.GetDeviceProviderByDevice(rgbDevice);

            if (deviceEntity != null)
                return new ArtemisDevice(rgbDevice, deviceProvider, deviceEntity);

            // Fall back on creating a new device
            _logger.Information(
                "No device config found for {deviceInfo}, device hash: {deviceHashCode}. Adding a new entry.",
                rgbDevice.DeviceInfo,
                deviceIdentifier
            );
            return new ArtemisDevice(rgbDevice, deviceProvider);
        }

        public void SaveDevice(ArtemisDevice artemisDevice)
        {
            artemisDevice.ApplyToEntity();
            artemisDevice.ApplyToRgbDevice();

            _deviceRepository.Save(artemisDevice.DeviceEntity);
            OnLedsChanged();
        }

        public void SaveDevices()
        {
            foreach (ArtemisDevice artemisDevice in _devices)
            {
                artemisDevice.ApplyToEntity();
                artemisDevice.ApplyToRgbDevice();
            }

            _deviceRepository.Save(_devices.Select(d => d.DeviceEntity));
            OnLedsChanged();
        }

        #endregion

        #region Event handlers

        private void RenderScaleSettingOnSettingChanged(object? sender, EventArgs e)
        {
            // The surface hasn't changed so we can safely reuse it
            UpdateBitmapBrush();
        }

        private void TargetFrameRateSettingOnSettingChanged(object? sender, EventArgs e)
        {
            UpdateTrigger.UpdateFrequency = 1.0 / _targetFrameRateSetting.Value;
        }

        private void SurfaceOnException(ExceptionEventArgs args)
        {
            _logger.Warning("Surface threw e");
            throw args.Exception;
        }

        #endregion

        #region Events

        public event EventHandler<DeviceEventArgs>? DeviceAdded;
        public event EventHandler<DeviceEventArgs>? DeviceRemoved;
        public event EventHandler? LedsChanged;

        private void OnDeviceAdded(DeviceEventArgs e)
        {
            DeviceAdded?.Invoke(this, e);
        }

        protected virtual void OnDeviceRemoved(DeviceEventArgs e)
        {
            DeviceRemoved?.Invoke(this, e);
        }

        protected virtual void OnLedsChanged()
        {
            LedsChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}