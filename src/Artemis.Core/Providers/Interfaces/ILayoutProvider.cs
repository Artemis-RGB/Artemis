namespace Artemis.Core.Providers;

/// <summary>
///     Represents a class that can provide Artemis layouts for devices.
/// </summary>
public interface ILayoutProvider
{
    /// <summary>
    ///     If available, loads an Artemis layout for the provided device.
    /// </summary>
    /// <param name="device">The device to load the layout for.</param>
    /// <returns>The resulting layout if one was available; otherwise <see langword="null" />.</returns>
    ArtemisLayout? GetDeviceLayout(ArtemisDevice device);

    /// <summary>
    /// Applies the layout to the provided device.
    /// </summary>
    /// <param name="device">The device to apply to.</param>
    /// <param name="layout">The layout to apply.</param>
    void ApplyLayout(ArtemisDevice device, ArtemisLayout layout);

    /// <summary>
    /// Determines whether the provided device is configured to use this layout provider.
    /// </summary>
    /// <param name="device">The device to check.</param>
    /// <returns>A value indicating whether the provided device is configured to use this layout provider.</returns>
    bool IsMatch(ArtemisDevice device);
}