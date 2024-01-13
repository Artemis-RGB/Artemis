using Artemis.Core.Services;

namespace Artemis.Core.Providers;

public class DefaultLayoutProvider : ILayoutProvider
{
    public static string LayoutType = "Default";
    private readonly IDeviceService _deviceService;

    public DefaultLayoutProvider(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }
    
    /// <inheritdoc />
    public ArtemisLayout? GetDeviceLayout(ArtemisDevice device)
    {
        // Look for a layout provided by the plugin
        ArtemisLayout layout = device.DeviceProvider.LoadLayout(device);

        // Finally fall back to a default layout
        return layout.IsValid ? layout : ArtemisLayout.GetDefaultLayout(device);
    }

    /// <inheritdoc />
    public void ApplyLayout(ArtemisDevice device, ArtemisLayout layout)
    {
        if (layout.IsDefaultLayout)
            device.ApplyLayout(layout, false, false);
        else
            device.ApplyLayout(layout, device.DeviceProvider.CreateMissingLedsSupported, device.DeviceProvider.RemoveExcessiveLedsSupported);
    }
    
    /// <inheritdoc />
    public bool IsMatch(ArtemisDevice device)
    {
        return device.LayoutSelection.Type == LayoutType;
    }
    
    /// <summary>
    /// Configures the provided device to use this layout provider.
    /// </summary>
    /// <param name="device">The device to apply the provider to.</param>
    public void ConfigureDevice(ArtemisDevice device)
    {
        device.LayoutSelection.Type = LayoutType;
        device.LayoutSelection.Parameter = null;
        _deviceService.SaveDevice(device);
        _deviceService.LoadDeviceLayout(device);
    }
}