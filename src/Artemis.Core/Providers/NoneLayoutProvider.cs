using Artemis.Core.Services;

namespace Artemis.Core.Providers;

public class NoneLayoutProvider : ILayoutProvider
{
    private readonly IDeviceService _deviceService;
    public static string LayoutType = "None";

    public NoneLayoutProvider(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }
    
    /// <inheritdoc />
    public ArtemisLayout? GetDeviceLayout(ArtemisDevice device)
    {
        return null;
    }

    /// <inheritdoc />
    public void ApplyLayout(ArtemisDevice device, ArtemisLayout layout)
    {
        device.ApplyLayout(null, false, false);
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