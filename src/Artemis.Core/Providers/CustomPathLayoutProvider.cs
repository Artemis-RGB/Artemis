namespace Artemis.Core.Providers;

/// <summary>
/// Represents a layout provider that loads a layout from a custom path.
/// </summary>
public class CustomPathLayoutProvider : ILayoutProvider
{
    /// <summary>
    /// The layout type of this layout provider.
    /// </summary>
    public const string LAYOUT_TYPE = "CustomPath";

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
        return device.LayoutSelection.Type == LAYOUT_TYPE;
    }

    /// <summary>
    ///     Configures the provided device to use this layout provider.
    /// </summary>
    /// <param name="device">The device to apply the provider to.</param>
    /// <param name="path">The path to the custom layout.</param>
    public void ConfigureDevice(ArtemisDevice device, string? path)
    {
        device.LayoutSelection.Type = LAYOUT_TYPE;
        device.LayoutSelection.Parameter = path;
    }
}