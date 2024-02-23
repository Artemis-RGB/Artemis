namespace Artemis.Core.Providers;

/// <summary>
/// Represents a layout provider that loads a layout from the plugin and falls back to a default layout.
/// </summary>
public class DefaultLayoutProvider : ILayoutProvider
{
    /// <summary>
    /// The layout type of this layout provider.
    /// </summary>
    public const string LAYOUT_TYPE = "Default";

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
        return device.LayoutSelection.Type == LAYOUT_TYPE;
    }

    /// <summary>
    ///     Configures the provided device to use this layout provider.
    /// </summary>
    /// <param name="device">The device to apply the provider to.</param>
    public void ConfigureDevice(ArtemisDevice device)
    {
        device.LayoutSelection.Type = LAYOUT_TYPE;
        device.LayoutSelection.Parameter = null;
    }
}