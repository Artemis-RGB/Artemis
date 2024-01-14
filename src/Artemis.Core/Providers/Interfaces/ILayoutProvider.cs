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

    void ApplyLayout(ArtemisDevice device, ArtemisLayout layout);
    bool IsMatch(ArtemisDevice device);
}