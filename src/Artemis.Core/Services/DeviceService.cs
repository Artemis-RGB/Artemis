using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Providers;
using Artemis.Core.Services.Models;
using Artemis.Storage.Entities.Surface;
using Artemis.Storage.Repositories.Interfaces;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Services;

internal class DeviceService : IDeviceService
{
    private readonly ILogger _logger;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IDeviceRepository _deviceRepository;
    private readonly Lazy<IRenderService> _renderService;
    private readonly Func<List<ILayoutProvider>> _getLayoutProviders;
    private readonly List<ArtemisDevice> _enabledDevices = [];
    private readonly List<ArtemisDevice> _devices = [];
    private readonly List<DeviceProvider> _suspendedDeviceProviders = [];
    private readonly object _suspensionLock = new();

    public DeviceService(ILogger logger,
        IPluginManagementService pluginManagementService,
        IDeviceRepository deviceRepository,
        Lazy<IRenderService> renderService,
        Func<List<ILayoutProvider>> getLayoutProviders)
    {
        _logger = logger;
        _pluginManagementService = pluginManagementService;
        _deviceRepository = deviceRepository;
        _renderService = renderService;
        _getLayoutProviders = getLayoutProviders;

        EnabledDevices = new ReadOnlyCollection<ArtemisDevice>(_enabledDevices);
        Devices = new ReadOnlyCollection<ArtemisDevice>(_devices);

        RenderScale.RenderScaleMultiplierChanged += RenderScaleOnRenderScaleMultiplierChanged;
    }

    public IReadOnlyCollection<ArtemisDevice> EnabledDevices { get; }
    public IReadOnlyCollection<ArtemisDevice> Devices { get; }

    /// <inheritdoc />
    public void IdentifyDevice(ArtemisDevice device)
    {
        BlinkDevice(device, 0);
    }

    /// <inheritdoc />
    public void AddDeviceProvider(DeviceProvider deviceProvider)
    {
        _logger.Verbose("[AddDeviceProvider] Adding {DeviceProvider}", deviceProvider.GetType().Name);
        IRGBDeviceProvider rgbDeviceProvider = deviceProvider.RgbDeviceProvider;

        try
        {
            // Can't see why this would happen, RgbService used to do this though
            List<ArtemisDevice> toRemove = _devices.Where(a => a.DeviceProvider.Id == deviceProvider.Id).ToList();
            _logger.Verbose("[AddDeviceProvider] Removing {Count} old device(s)", toRemove.Count);
            foreach (ArtemisDevice device in toRemove)
            {
                _devices.Remove(device);
                _enabledDevices.Remove(device);
                OnDeviceRemoved(new DeviceEventArgs(device));
            }

            List<Exception> providerExceptions = [];

            void DeviceProviderOnException(object? sender, ExceptionEventArgs e)
            {
                if (e.IsCritical)
                    providerExceptions.Add(e.Exception);
                else
                    _logger.Warning(e.Exception, "Device provider {deviceProvider} threw non-critical exception", deviceProvider.GetType().Name);
            }

            _logger.Verbose("[AddDeviceProvider] Initializing device provider");
            rgbDeviceProvider.Exception += DeviceProviderOnException;
            rgbDeviceProvider.Initialize();
            _logger.Verbose("[AddDeviceProvider] Attaching devices of device provider");
            rgbDeviceProvider.Exception -= DeviceProviderOnException;
            if (providerExceptions.Count == 1)
                throw new ArtemisPluginException("RGB.NET threw exception: " + providerExceptions.First().Message, providerExceptions.First());
            if (providerExceptions.Count > 1)
                throw new ArtemisPluginException("RGB.NET threw multiple exceptions", new AggregateException(providerExceptions));

            if (!rgbDeviceProvider.Devices.Any())
            {
                _logger.Warning("Device provider {deviceProvider} has no devices", deviceProvider.GetType().Name);
                return;
            }

            List<ArtemisDevice> addedDevices = [];
            foreach (IRGBDevice rgbDevice in rgbDeviceProvider.Devices)
            {
                ArtemisDevice artemisDevice = GetArtemisDevice(rgbDevice);
                addedDevices.Add(artemisDevice);
                _devices.Add(artemisDevice);
                if (artemisDevice.IsEnabled)
                    _enabledDevices.Add(artemisDevice);

                _logger.Debug("Device provider {deviceProvider} added {deviceName}", deviceProvider.GetType().Name, rgbDevice.DeviceInfo.DeviceName);
            }

            _devices.Sort((a, b) => a.ZIndex - b.ZIndex);
            _enabledDevices.Sort((a, b) => a.ZIndex - b.ZIndex);

            OnDeviceProviderAdded(new DeviceProviderEventArgs(deviceProvider, addedDevices));
            foreach (ArtemisDevice artemisDevice in addedDevices)
                OnDeviceAdded(new DeviceEventArgs(artemisDevice));

            UpdateLeds();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Exception during device loading for device provider {deviceProvider}", deviceProvider.GetType().Name);
            throw;
        }
    }

    /// <inheritdoc />
    public void RemoveDeviceProvider(DeviceProvider deviceProvider)
    {
        _logger.Verbose("[RemoveDeviceProvider] Pausing rendering to remove {DeviceProvider}", deviceProvider.GetType().Name);
        List<ArtemisDevice> toRemove = _devices.Where(a => a.DeviceProvider.Id == deviceProvider.Id).ToList();

        try
        {
            _logger.Verbose("[RemoveDeviceProvider] Removing {Count} old device(s)", toRemove.Count);
            foreach (ArtemisDevice device in toRemove)
            {
                _devices.Remove(device);
                _enabledDevices.Remove(device);
            }

            _devices.Sort((a, b) => a.ZIndex - b.ZIndex);

            OnDeviceProviderRemoved(new DeviceProviderEventArgs(deviceProvider, toRemove));
            foreach (ArtemisDevice artemisDevice in toRemove)
                OnDeviceRemoved(new DeviceEventArgs(artemisDevice));

            UpdateLeds();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Exception during device removal for device provider {deviceProvider}", deviceProvider.GetType().Name);
            throw;
        }
    }

    /// <inheritdoc />
    public void AutoArrangeDevices()
    {
        SurfaceArrangement surfaceArrangement = SurfaceArrangement.GetDefaultArrangement();
        surfaceArrangement.Arrange(_devices);
        foreach (ArtemisDevice artemisDevice in _devices)
            artemisDevice.ApplyDefaultCategories();

        SaveDevices();
    }

    /// <inheritdoc />
    public void LoadDeviceLayout(ArtemisDevice device)
    {
        ILayoutProvider? provider = _getLayoutProviders().FirstOrDefault(p => p.IsMatch(device));
        if (provider == null)
            _logger.Warning("Could not find a layout provider for type {LayoutType} of device {Device}", device.LayoutSelection.Type, device);

        ArtemisLayout? layout = provider?.GetDeviceLayout(device);
        if (layout != null && !layout.IsValid)
        {
            _logger.Warning("Got an invalid layout {Layout} from {LayoutProvider}", layout, provider!.GetType().FullName);
            layout = null;
        }

        try
        {
            if (layout == null)
                device.ApplyLayout(null, false, false);
            else
                provider?.ApplyLayout(device, layout);

            UpdateLeds();
        }
        catch (Exception e)
        {
            device.LayoutSelection.ErrorState = e.Message;
            _logger.Error(e, "Failed to apply device layout");
        }
    }

    /// <inheritdoc />
    public void EnableDevice(ArtemisDevice device)
    {
        if (device.IsEnabled)
            return;

        _enabledDevices.Add(device);
        device.IsEnabled = true;
        device.Save();
        _deviceRepository.Save(device.DeviceEntity);

        OnDeviceEnabled(new DeviceEventArgs(device));
        UpdateLeds();
    }

    /// <inheritdoc />
    public void DisableDevice(ArtemisDevice device)
    {
        if (!device.IsEnabled)
            return;

        _enabledDevices.Remove(device);
        device.IsEnabled = false;
        device.Save();
        _deviceRepository.Save(device.DeviceEntity);

        OnDeviceDisabled(new DeviceEventArgs(device));
        UpdateLeds();
    }

    /// <inheritdoc />
    public void SaveDevice(ArtemisDevice artemisDevice)
    {
        artemisDevice.Save();
        _deviceRepository.Save(artemisDevice.DeviceEntity);
        UpdateLeds();
    }

    /// <inheritdoc />
    public void SaveDevices()
    {
        foreach (ArtemisDevice artemisDevice in _devices)
            artemisDevice.Save();
        _deviceRepository.SaveRange(_devices.Select(d => d.DeviceEntity));
        UpdateLeds();
    }

    /// <inheritdoc />
    public void SuspendDeviceProviders()
    {
        lock (_suspensionLock)
        {
            _logger.Information("Suspending all device providers");

            bool wasPaused = _renderService.Value.IsPaused;
            try
            {
                _renderService.Value.IsPaused = true;
                foreach (DeviceProvider deviceProvider in _pluginManagementService.GetFeaturesOfType<DeviceProvider>().Where(d => d.SuspendSupported))
                    SuspendDeviceProvider(deviceProvider);
            }
            finally
            {
                _renderService.Value.IsPaused = wasPaused;
            }
        }
    }

    /// <inheritdoc />
    public void ResumeDeviceProviders()
    {
        lock (_suspensionLock)
        {
            _logger.Information("Resuming all device providers");

            bool wasPaused = _renderService.Value.IsPaused;
            try
            {
                _renderService.Value.IsPaused = true;
                foreach (DeviceProvider deviceProvider in _suspendedDeviceProviders.ToList())
                    ResumeDeviceProvider(deviceProvider);
            }
            finally
            {
                _renderService.Value.IsPaused = wasPaused;
            }
        }
    }

    private void SuspendDeviceProvider(DeviceProvider deviceProvider)
    {
        if (_suspendedDeviceProviders.Contains(deviceProvider))
        {
            _logger.Warning("Device provider {DeviceProvider} is already suspended", deviceProvider.Info.Name);
            return;
        }

        try
        {
            _pluginManagementService.DisablePluginFeature(deviceProvider, false);
            deviceProvider.Suspend();
            _suspendedDeviceProviders.Add(deviceProvider);
            _logger.Information("Device provider {DeviceProvider} suspended", deviceProvider.Info.Name);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Device provider {DeviceProvider} failed to suspend", deviceProvider.Info.Name);
        }
    }

    private void ResumeDeviceProvider(DeviceProvider deviceProvider)
    {
        try
        {
            _pluginManagementService.EnablePluginFeature(deviceProvider, false, true);
            _suspendedDeviceProviders.Remove(deviceProvider);
            _logger.Information("Device provider {DeviceProvider} resumed", deviceProvider.Info.Name);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Device provider {DeviceProvider} failed to resume", deviceProvider.Info.Name);
        }
    }

    private ArtemisDevice GetArtemisDevice(IRGBDevice rgbDevice)
    {
        string deviceIdentifier = rgbDevice.GetDeviceIdentifier();
        DeviceEntity? deviceEntity = _deviceRepository.Get(deviceIdentifier);
        DeviceProvider deviceProvider = _pluginManagementService.GetDeviceProviderByDevice(rgbDevice);

        ArtemisDevice device;
        if (deviceEntity != null)
            device = new ArtemisDevice(rgbDevice, deviceProvider, deviceEntity);
        // Fall back on creating a new device
        else
        {
            _logger.Information("No device config found for {DeviceInfo}, device hash: {DeviceHashCode}. Adding a new entry", rgbDevice.DeviceInfo, deviceIdentifier);
            device = new ArtemisDevice(rgbDevice, deviceProvider);
            _deviceRepository.Add(device.DeviceEntity);
        }

        LoadDeviceLayout(device);
        return device;
    }

    private void BlinkDevice(ArtemisDevice device, int blinkCount)
    {
        RGBSurface surface = _renderService.Value.Surface;

        // Create a LED group way at the top
        ListLedGroup ledGroup = new(surface, device.Leds.Select(l => l.RgbLed))
        {
            Brush = new SolidColorBrush(new Color(255, 255, 255)),
            ZIndex = 999
        };

        // After 200ms, detach the LED group
        Task.Run(async () =>
        {
            await Task.Delay(200);
            ledGroup.Detach();

            if (blinkCount < 5)
            {
                // After another 200ms, start over, repeat six times
                await Task.Delay(200);
                BlinkDevice(device, blinkCount + 1);
            }
        });
    }

    private void CalculateRenderProperties()
    {
        foreach (ArtemisDevice artemisDevice in Devices)
            artemisDevice.CalculateRenderProperties();
        UpdateLeds();
    }

    private void UpdateLeds()
    {
        OnLedsChanged();
    }

    private void RenderScaleOnRenderScaleMultiplierChanged(object? sender, EventArgs e)
    {
        CalculateRenderProperties();
    }

    #region Events

    /// <inheritdoc />
    public event EventHandler<DeviceEventArgs>? DeviceAdded;

    /// <inheritdoc />
    public event EventHandler<DeviceEventArgs>? DeviceRemoved;

    /// <inheritdoc />
    public event EventHandler<DeviceEventArgs>? DeviceEnabled;

    /// <inheritdoc />
    public event EventHandler<DeviceEventArgs>? DeviceDisabled;

    /// <inheritdoc />
    public event EventHandler<DeviceProviderEventArgs>? DeviceProviderAdded;

    /// <inheritdoc />
    public event EventHandler<DeviceProviderEventArgs>? DeviceProviderRemoved;

    /// <inheritdoc />
    public event EventHandler? LedsChanged;

    protected virtual void OnDeviceAdded(DeviceEventArgs e)
    {
        DeviceAdded?.Invoke(this, e);
    }

    protected virtual void OnDeviceRemoved(DeviceEventArgs e)
    {
        DeviceRemoved?.Invoke(this, e);
    }

    protected virtual void OnDeviceEnabled(DeviceEventArgs e)
    {
        DeviceEnabled?.Invoke(this, e);
    }

    protected virtual void OnDeviceDisabled(DeviceEventArgs e)
    {
        DeviceDisabled?.Invoke(this, e);
    }

    protected virtual void OnDeviceProviderAdded(DeviceProviderEventArgs e)
    {
        DeviceProviderAdded?.Invoke(this, e);
    }

    protected virtual void OnDeviceProviderRemoved(DeviceProviderEventArgs e)
    {
        DeviceProviderRemoved?.Invoke(this, e);
    }

    protected virtual void OnLedsChanged()
    {
        LedsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}