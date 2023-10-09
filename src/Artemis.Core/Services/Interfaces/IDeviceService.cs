namespace Artemis.Core.Services;

/// <summary>
///     A service that allows you manage an <see cref="ArtemisDevice" />
/// </summary>
public interface IDeviceService : IArtemisService
{
    /// <summary>
    ///     Identifies the device by making it blink white 5 times
    /// </summary>
    /// <param name="device"></param>
    void IdentifyDevice(ArtemisDevice device);
}