namespace Artemis.Core.Providers;

public class DefaultLayoutProvider : ILayoutProvider
{
    public static string LayoutType = "Default";
    
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
}