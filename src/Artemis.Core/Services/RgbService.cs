using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services.Models;
using Artemis.Core.SkiaSharp;
using Artemis.Storage.Entities.Surface;
using Artemis.Storage.Repositories.Interfaces;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides wrapped access the RGB.NET
    /// </summary>
    internal class RgbService : IRgbService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly List<ArtemisDevice> _devices;
        private readonly List<ArtemisDevice> _enabledDevices;
        private readonly ILogger _logger;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly PluginSetting<double> _renderScaleSetting;
        private readonly PluginSetting<int> _targetFrameRateSetting;
        private readonly TextureBrush _textureBrush = new(ITexture.Empty) {CalculationMode = RenderMode.Absolute};
        private Dictionary<Led, ArtemisLed> _ledMap;
        private bool _modifyingProviders;
        private ListLedGroup? _surfaceLedGroup;
        private SKTexture? _texture;

        public RgbService(ILogger logger, ISettingsService settingsService, IPluginManagementService pluginManagementService, IDeviceRepository deviceRepository)
        {
            _logger = logger;
            _pluginManagementService = pluginManagementService;
            _deviceRepository = deviceRepository;
            _targetFrameRateSetting = settingsService.GetSetting("Core.TargetFrameRate", 25);
            _renderScaleSetting = settingsService.GetSetting("Core.RenderScale", 0.5);

            Surface = new RGBSurface();

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;
            Surface.SurfaceLayoutChanged += SurfaceOnLayoutChanged;
            _targetFrameRateSetting.SettingChanged += TargetFrameRateSettingOnSettingChanged;
            _renderScaleSetting.SettingChanged += RenderScaleSettingOnSettingChanged;
            _enabledDevices = new List<ArtemisDevice>();
            _devices = new List<ArtemisDevice>();
            _ledMap = new Dictionary<Led, ArtemisLed>();

            UpdateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / _targetFrameRateSetting.Value};
            Surface.RegisterUpdateTrigger(UpdateTrigger);

            Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
        }

        public TimerUpdateTrigger UpdateTrigger { get; }

        protected virtual void OnDeviceRemoved(DeviceEventArgs e)
        {
            DeviceRemoved?.Invoke(this, e);
        }

        protected virtual void OnLedsChanged()
        {
            LedsChanged?.Invoke(this, EventArgs.Empty);
            _texture?.Invalidate();
        }

        private void UtilitiesOnShutdownRequested(object? sender, EventArgs e)
        {
            IsRenderPaused = true;
        }

        private void SurfaceOnLayoutChanged(SurfaceLayoutChangedEventArgs args)
        {
            UpdateLedGroup();
        }

        private void UpdateLedGroup()
        {
            lock (_devices)
            {
                if (_modifyingProviders)
                    return;

                _ledMap = new Dictionary<Led, ArtemisLed>(_devices.SelectMany(d => d.Leds).ToDictionary(l => l.RgbLed));

                if (_surfaceLedGroup == null)
                {
                    _surfaceLedGroup = new ListLedGroup(Surface, LedMap.Select(l => l.Key)) {Brush = _textureBrush};
                    OnLedsChanged();
                    return;
                }

                lock (_surfaceLedGroup)
                {
                    // Clean up the old background
                    _surfaceLedGroup.Detach();

                    // Apply the application wide brush and decorator
                    _surfaceLedGroup = new ListLedGroup(Surface, LedMap.Select(l => l.Key)) {Brush = _textureBrush};
                    OnLedsChanged();
                }
            }
        }

        private void TargetFrameRateSettingOnSettingChanged(object? sender, EventArgs e)
        {
            UpdateTrigger.UpdateFrequency = 1.0 / _targetFrameRateSetting.Value;
        }

        private void SurfaceOnException(ExceptionEventArgs args)
        {
            _logger.Warning(args.Exception, "Surface caught exception");
            throw args.Exception;
        }

        private void OnDeviceAdded(DeviceEventArgs e)
        {
            DeviceAdded?.Invoke(this, e);
        }

        private void RenderScaleSettingOnSettingChanged(object? sender, EventArgs e)
        {
            _texture?.Invalidate();
        }

        public IReadOnlyCollection<ArtemisDevice> EnabledDevices => _enabledDevices.AsReadOnly();
        public IReadOnlyCollection<ArtemisDevice> Devices => _devices.AsReadOnly();
        public IReadOnlyDictionary<Led, ArtemisLed> LedMap => new ReadOnlyDictionary<Led, ArtemisLed>(_ledMap);

        public RGBSurface Surface { get; set; }
        public bool IsRenderPaused { get; set; }
        public bool RenderOpen { get; private set; }

        public void AddDeviceProvider(IRGBDeviceProvider deviceProvider)
        {
            lock (_devices)
            {
                try
                {
                    _modifyingProviders = true;

                    List<ArtemisDevice> toRemove = _devices.Where(a => deviceProvider.Devices.Any(d => a.RgbDevice == d)).ToList();
                    Surface.Detach(toRemove.Select(d => d.RgbDevice));
                    foreach (ArtemisDevice device in toRemove)
                        RemoveDevice(device);

                    List<Exception> providerExceptions = new();

                    void DeviceProviderOnException(object? sender, ExceptionEventArgs e)
                    {
                        if (e.IsCritical)
                            providerExceptions.Add(e.Exception);
                        else
                            _logger.Warning(e.Exception, "Device provider {deviceProvider} threw non-critical exception", deviceProvider.GetType().Name);
                    }

                    deviceProvider.Exception += DeviceProviderOnException;
                    deviceProvider.Initialize();
                    Surface.Attach(deviceProvider.Devices);
                    deviceProvider.Exception -= DeviceProviderOnException;
                    if (providerExceptions.Count == 1)
                        throw new ArtemisPluginException("RGB.NET threw exception: " + providerExceptions.First().Message, providerExceptions.First());
                    if (providerExceptions.Count > 1)
                        throw new ArtemisPluginException("RGB.NET threw multiple exceptions", new AggregateException(providerExceptions));

                    if (!deviceProvider.Devices.Any())
                    {
                        _logger.Warning("Device provider {deviceProvider} has no devices", deviceProvider.GetType().Name);
                        return;
                    }

                    foreach (IRGBDevice rgbDevice in deviceProvider.Devices)
                    {
                        ArtemisDevice artemisDevice = GetArtemisDevice(rgbDevice);
                        AddDevice(artemisDevice);
                        _logger.Debug("Device provider {deviceProvider} added {deviceName}", deviceProvider.GetType().Name, rgbDevice.DeviceInfo.DeviceName);
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
                    UpdateLedGroup();
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
                    Surface.Detach(toRemove.Select(d => d.RgbDevice));
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
                    UpdateLedGroup();
                }
            }
        }

        public void Dispose()
        {
            Surface.UnregisterUpdateTrigger(UpdateTrigger);

            UpdateTrigger.Dispose();
            Surface.Dispose();
        }

        public event EventHandler<DeviceEventArgs>? DeviceAdded;
        public event EventHandler<DeviceEventArgs>? DeviceRemoved;
        public event EventHandler? LedsChanged;

        #region Rendering

        private IManagedGraphicsContext? _newGraphicsContext;

        public SKTexture OpenRender()
        {
            if (RenderOpen)
                throw new ArtemisCoreException("Render pipeline is already open");

            if (_texture == null || _texture.IsInvalid)
                CreateTexture();

            RenderOpen = true;
            return _texture!;
        }

        public void CloseRender()
        {
            if (!RenderOpen)
                throw new ArtemisCoreException("Render pipeline is already closed");

            RenderOpen = false;
            _texture?.CopyPixelData();
        }

        public void CreateTexture()
        {
            if (RenderOpen)
                throw new ArtemisCoreException("Cannot update the texture while rendering");

            IManagedGraphicsContext? graphicsContext = Constants.ManagedGraphicsContext = _newGraphicsContext;
            if (!ReferenceEquals(graphicsContext, _newGraphicsContext))
                graphicsContext = _newGraphicsContext;

            if (graphicsContext != null)
                _logger.Debug("Creating SKTexture with graphics context {graphicsContext}", graphicsContext.GetType().Name);
            else
                _logger.Debug("Creating SKTexture with software-based graphics context");

            float evenWidth = Surface.Boundary.Size.Width;
            if (evenWidth % 2 != 0)
                evenWidth++;
            float evenHeight = Surface.Boundary.Size.Height;
            if (evenHeight % 2 != 0)
                evenHeight++;

            float renderScale = (float) _renderScaleSetting.Value;
            int width = Math.Max(1, MathF.Min(evenWidth * renderScale, 4096).RoundToInt());
            int height = Math.Max(1, MathF.Min(evenHeight * renderScale, 4096).RoundToInt());

            _texture?.Dispose();
            _texture = new SKTexture(graphicsContext, width, height, renderScale);
            _textureBrush.Texture = _texture;


            if (!ReferenceEquals(_newGraphicsContext, Constants.ManagedGraphicsContext = _newGraphicsContext))
            {
                Constants.ManagedGraphicsContext?.Dispose();
                Constants.ManagedGraphicsContext = _newGraphicsContext;
                _newGraphicsContext = null;
            }
        }

        public void UpdateGraphicsContext(IManagedGraphicsContext? managedGraphicsContext)
        {
            if (ReferenceEquals(managedGraphicsContext, Constants.ManagedGraphicsContext))
                return;

            _newGraphicsContext = managedGraphicsContext;
            _texture?.Invalidate();
        }

        #endregion

        #region EnabledDevices

        public void AutoArrangeDevices()
        {
            SurfaceArrangement surfaceArrangement = SurfaceArrangement.GetDefaultArrangement();
            surfaceArrangement.Arrange(_devices);
            foreach (ArtemisDevice artemisDevice in _devices)
                artemisDevice.ApplyDefaultCategories();

            SaveDevices();
        }

        public ArtemisLayout? ApplyBestDeviceLayout(ArtemisDevice device)
        {
            ArtemisLayout? layout;

            // Configured layout path takes precedence over all other options
            if (device.CustomLayoutPath != null)
            {
                layout = new ArtemisLayout(device.CustomLayoutPath, LayoutSource.Configured);
                if (layout.IsValid)
                {
                    ApplyDeviceLayout(device, layout);
                    return layout;
                }
            }

            // Look for a layout provided by the user
            layout = device.DeviceProvider.LoadUserLayout(device);
            if (layout.IsValid)
            {
                ApplyDeviceLayout(device, layout);
                return layout;
            }

            // Look for a layout provided by the plugin
            layout = device.DeviceProvider.LoadLayout(device);
            if (layout.IsValid)
            {
                ApplyDeviceLayout(device, layout);
                return layout;
            }

            // Finally fall back to a default layout
            layout = ArtemisLayout.GetDefaultLayout(device);
            if (layout != null)
                ApplyDeviceLayout(device, layout);
            return layout;
        }

        public void ApplyDeviceLayout(ArtemisDevice device, ArtemisLayout layout)
        {
            if (layout.Source == LayoutSource.Default)
                device.ApplyLayout(layout, false, false);
            else
                device.ApplyLayout(layout, device.DeviceProvider.CreateMissingLedsSupported, device.DeviceProvider.RemoveExcessiveLedsSupported);
            
            UpdateLedGroup();
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
    }
}