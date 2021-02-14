using System;
using System.Collections.Generic;
using System.Linq;
using RGB.NET.Core;
using RGB.NET.Groups;
using RGB.NET.Layout;
using Serilog;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides wrapped access the RGB.NET
    /// </summary>
    internal class RgbService : IRgbService
    {
        private readonly List<IRGBDevice> _loadedDevices;
        private readonly ILogger _logger;
        private readonly PluginSetting<double> _renderScaleSetting;
        private readonly PluginSetting<int> _sampleSizeSetting;
        private readonly PluginSetting<int> _targetFrameRateSetting;
        private ListLedGroup? _surfaceLedGroup;

        public RgbService(ILogger logger, ISettingsService settingsService)
        {
            _logger = logger;
            _renderScaleSetting = settingsService.GetSetting("Core.RenderScale", 0.5);
            _targetFrameRateSetting = settingsService.GetSetting("Core.TargetFrameRate", 25);
            _sampleSizeSetting = settingsService.GetSetting("Core.SampleSize", 1);

            Surface = new RGBSurface();

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;
            _renderScaleSetting.SettingChanged += RenderScaleSettingOnSettingChanged;
            _targetFrameRateSetting.SettingChanged += TargetFrameRateSettingOnSettingChanged;
            _loadedDevices = new List<IRGBDevice>();
            UpdateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / _targetFrameRateSetting.Value};
            Surface.RegisterUpdateTrigger(UpdateTrigger);
        }

        /// <inheritdoc />
        public RGBSurface Surface { get; set; }

        public TimerUpdateTrigger UpdateTrigger { get; }
        public BitmapBrush? BitmapBrush { get; private set; }
        public IReadOnlyCollection<IRGBDevice> LoadedDevices => _loadedDevices.AsReadOnly();
        public double RenderScale => _renderScaleSetting.Value;
        public bool IsRenderPaused { get; set; }

        public void AddDeviceProvider(IRGBDeviceProvider deviceProvider)
        {
            try
            {
                List<IRGBDevice> toRemove = deviceProvider.Devices.Where(d => Surface.Devices.Contains(d)).ToList();
                Surface.Detach(deviceProvider.Devices);
                foreach (IRGBDevice rgbDevice in toRemove) 
                    OnDeviceRemoved(new DeviceEventArgs(rgbDevice));

                deviceProvider.Initialize(RGBDeviceType.All, true);
                Surface.Attach(deviceProvider.Devices);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception during device loading for device provider {deviceProvider}", deviceProvider.GetType().Name);
                throw;
            }

            if (deviceProvider.Devices == null || !deviceProvider.Devices.Any())
            {
                _logger.Warning("Device provider {deviceProvider} has no devices", deviceProvider.GetType().Name);
                return;
            }

            foreach (IRGBDevice surfaceDevice in deviceProvider.Devices)
            {
                _logger.Debug("Device provider {deviceProvider} added {deviceName}", deviceProvider.GetType().Name, surfaceDevice.DeviceInfo?.DeviceName);
                if (!_loadedDevices.Contains(surfaceDevice))
                {
                    _loadedDevices.Add(surfaceDevice);
                    OnDeviceLoaded(new DeviceEventArgs(surfaceDevice));
                }
                else
                    OnDeviceReloaded(new DeviceEventArgs(surfaceDevice));
            }
        }

        public void Dispose()
        {
            Surface.UnregisterUpdateTrigger(UpdateTrigger);

            UpdateTrigger.Dispose();
            Surface.Dispose();
        }

        private void RenderScaleSettingOnSettingChanged(object? sender, EventArgs e)
        {
            // The surface hasn't changed so we can safely reuse it
            UpdateSurfaceLedGroup(BitmapBrush?.Surface);
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

        #region Events

        public event EventHandler<DeviceEventArgs>? DeviceLoaded;
        public event EventHandler<DeviceEventArgs>? DeviceReloaded;
        public event EventHandler<DeviceEventArgs> DeviceRemoved;

        public void UpdateSurfaceLedGroup(ArtemisSurface? artemisSurface)
        {
            if (artemisSurface == null)
                return;

            if (_surfaceLedGroup == null || BitmapBrush == null)
            {
                // Apply the application wide brush and decorator
                BitmapBrush = new BitmapBrush(new Scale(_renderScaleSetting.Value), _sampleSizeSetting);
                _surfaceLedGroup = new ListLedGroup(Surface, artemisSurface.LedMap.Select(l => l.Key)) {Brush = BitmapBrush};
                return;
            }

            lock (_surfaceLedGroup)
            {
                // Clean up the old background
                _surfaceLedGroup.Detach(Surface);

                // Apply the application wide brush and decorator
                BitmapBrush.Scale = new Scale(_renderScaleSetting.Value);
                BitmapBrush.Surface = artemisSurface;
                _surfaceLedGroup = new ListLedGroup(Surface, artemisSurface.LedMap.Select(l => l.Key)) {Brush = BitmapBrush};
            }
        }

        private void OnDeviceLoaded(DeviceEventArgs e)
        {
            DeviceLoaded?.Invoke(this, e);
        }

        private void OnDeviceReloaded(DeviceEventArgs e)
        {
            DeviceReloaded?.Invoke(this, e);
        }

        protected virtual void OnDeviceRemoved(DeviceEventArgs e)
        {
            DeviceRemoved?.Invoke(this, e);
        }
        #endregion

    }
}