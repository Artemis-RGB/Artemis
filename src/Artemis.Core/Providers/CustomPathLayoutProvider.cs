using Artemis.Core.Services;
using RGB.NET.Layout;

namespace Artemis.Core.Providers;

public class CustomPathLayoutProvider : ILayoutProvider
{
    public static string LayoutType = "CustomPath";
    private readonly IDeviceService _deviceService;

    public CustomPathLayoutProvider(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <inheritdoc />
    public ArtemisLayout? GetDeviceLayout(ArtemisDevice device)
    {
        if (device.LayoutSelection.Parameter == null)
            return null;
        return new ArtemisLayout(device.LayoutSelection.Parameter);
    }

    /// <inheritdoc />
    public void ApplyLayout(ArtemisDevice device, ArtemisLayout layout)
    {
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
    /// <param name="path">The path to the custom layout.</param>
    public void ConfigureDevice(ArtemisDevice device, string? path)
    {
        device.LayoutSelection.Type = LayoutType;
        device.LayoutSelection.Parameter = path;
        _deviceService.SaveDevice(device);
        _deviceService.LoadDeviceLayout(device);
    }
}