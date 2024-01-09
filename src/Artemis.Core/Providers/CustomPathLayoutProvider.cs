using RGB.NET.Layout;

namespace Artemis.Core.Providers;

public class CustomPathLayoutProvider : ILayoutProvider
{
    public static string LayoutType = "CustomPath";
    
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
}