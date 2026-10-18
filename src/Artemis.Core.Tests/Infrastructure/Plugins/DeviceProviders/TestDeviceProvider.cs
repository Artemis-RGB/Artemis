using Artemis.Core.DeviceProviders;
using RGB.NET.Core;
using RGB.NET.Layout;

namespace Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;

/// <summary>
///     A device provider feature that provides fake devices created from layout files.
/// </summary>
public class TestDeviceProvider : DeviceProvider
{
    private readonly TestRGBDeviceProvider _rgbDeviceProvider = new();

    public override IRGBDeviceProvider RgbDeviceProvider => _rgbDeviceProvider;

    /// <summary>
    ///     Creates a fake device from the provided layout.
    /// </summary>
    /// <param name="layout">The layout describing the device and its LEDs</param>
    /// <param name="ledsUpdated">Called whenever the device sends colors to the "hardware", after color corrections</param>
    public IRGBDevice CreateDevice(IDeviceLayout layout, Action<Led, Color> ledsUpdated)
    {
        _rgbDeviceProvider.Definitions.Add((layout, ledsUpdated));
        _rgbDeviceProvider.Initialize(throwExceptions: true);
        return _rgbDeviceProvider.Devices.Last();
    }

    public override void Enable()
    {
    }

    public override void Disable()
    {
        _rgbDeviceProvider.Dispose();
    }
}
