using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Plugins.Models;
using Artemis.Core.RGB.NET;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Services.Storage;
using RGB.NET.Brushes;
using RGB.NET.Core;
using RGB.NET.Groups;
using Serilog;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides wrapped access the RGB.NET
    /// </summary>
    public class RgbService : IRgbService, IDisposable
    {
        private readonly List<IRGBDevice> _loadedDevices;
        private readonly ILogger _logger;
        private readonly TimerUpdateTrigger _updateTrigger;
        private ListLedGroup _background;
        private readonly PluginSetting<double> _renderScaleSetting;
        private readonly PluginSetting<int> _targetFrameRateSetting;

        internal RgbService(ILogger logger, ISettingsService settingsService)
        {
            _logger = logger;
            _renderScaleSetting = settingsService.GetSetting("RenderScale", 1.0);
            _targetFrameRateSetting = settingsService.GetSetting("TargetFrameRate", 25);

            Surface = RGBSurface.Instance;

            // Let's throw these for now
            Surface.Exception += SurfaceOnException;
            _renderScaleSetting.SettingChanged += RenderScaleSettingOnSettingChanged;
            _targetFrameRateSetting.SettingChanged += TargetFrameRateSettingOnSettingChanged;
            _loadedDevices = new List<IRGBDevice>();
            _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / _targetFrameRateSetting.Value};
            Surface.RegisterUpdateTrigger(_updateTrigger);
            
        }

        /// <inheritdoc />
        public RGBSurface Surface { get; set; }

        public GraphicsDecorator GraphicsDecorator { get; private set; }

        public IReadOnlyCollection<IRGBDevice> LoadedDevices => _loadedDevices.AsReadOnly();

        public void AddDeviceProvider(IRGBDeviceProvider deviceProvider)
        {
            Surface.LoadDevices(deviceProvider);

            if (deviceProvider.Devices == null)
            {
                _logger.Warning("RgbDevice provider {deviceProvider} has no devices", deviceProvider.GetType().Name);
                return;
            }

            foreach (var surfaceDevice in deviceProvider.Devices)
            {
                if (!_loadedDevices.Contains(surfaceDevice))
                {
                    _loadedDevices.Add(surfaceDevice);
                    OnDeviceLoaded(new DeviceEventArgs(surfaceDevice));
                }
                else
                {
                    OnDeviceReloaded(new DeviceEventArgs(surfaceDevice));
                }
            }
        }

        public void Dispose()
        {
            Surface.UnregisterUpdateTrigger(_updateTrigger);

            _updateTrigger.Dispose();
            Surface.Dispose();
        }

        private void RenderScaleSettingOnSettingChanged(object sender, EventArgs e)
        {
            UpdateGraphicsDecorator();
        }

        private void TargetFrameRateSettingOnSettingChanged(object sender, EventArgs e)
        {
            _updateTrigger.UpdateFrequency = 1.0 / _targetFrameRateSetting.Value;
        }
        
        private void SurfaceOnException(ExceptionEventArgs args)
        {
            throw args.Exception;
        }

        #region Events

        public event EventHandler<DeviceEventArgs> DeviceLoaded;
        public event EventHandler<DeviceEventArgs> DeviceReloaded;

        public void UpdateGraphicsDecorator()
        {
            // TODO: Create new one first, then clean up the old one for a smoother transition

            // Clean up the old background if present
            if (_background != null)
            {
                _background.Brush?.RemoveAllDecorators();
                _background.Detach();
            }

            // Apply the application wide brush and decorator
            _background = new ListLedGroup(Surface.Leds) {Brush = new SolidColorBrush(new Color(255, 255, 255, 255))};
            GraphicsDecorator = new GraphicsDecorator(_background, _renderScaleSetting.Value);
            _background.Brush.RemoveAllDecorators();

            _background.Brush.AddDecorator(GraphicsDecorator);
        }

        private void OnDeviceLoaded(DeviceEventArgs e)
        {
            DeviceLoaded?.Invoke(this, e);
        }

        private void OnDeviceReloaded(DeviceEventArgs e)
        {
            DeviceReloaded?.Invoke(this, e);
        }

        #endregion
    }
}