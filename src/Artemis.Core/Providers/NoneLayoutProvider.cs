namespace Artemis.Core.Providers;

/// <summary>
/// Represents a layout provider that does not load a layout.
/// </summary>
public class NoneLayoutProvider : ILayoutProvider
{
    /// <summary>
    /// The layout type of this layout provider.
    /// </summary>
    public const string LAYOUT_TYPE = "None";

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