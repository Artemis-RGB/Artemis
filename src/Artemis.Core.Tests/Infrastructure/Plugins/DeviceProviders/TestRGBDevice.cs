using RGB.NET.Core;
using RGB.NET.Layout;

namespace Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;

/// <summary>
///     A fake device, similar to RGB.NET's DebugRGBDevice.
///     <para>
///         DebugRGBDevice overrides UpdateLeds which skips applying the requested colors to its LEDs and skips color
///         corrections, this device goes through the same update path as real devices.
///     </para>
/// </summary>
internal sealed class TestRGBDevice : AbstractRGBDevice<TestRGBDeviceInfo>, IUnknownDevice
{
    private readonly Action<Led, Color> _ledsUpdated;

    public TestRGBDevice(IDeviceLayout layout, Action<Led, Color> ledsUpdated)
        : base(new TestRGBDeviceInfo(layout.Type, layout.Vendor ?? "Artemis", layout.Model ?? "Test device"), new NoOpUpdateQueue())
    {
        _ledsUpdated = ledsUpdated;
        layout.ApplyTo(this, true);
    }

    protected override void UpdateLeds(IEnumerable<Led> ledsToUpdate)
    {
        List<Led> leds = ledsToUpdate.ToList();

        // Applies the requested colors to the LEDs and hands them to the (no-op) update queue
        base.UpdateLeds(leds);

        foreach (Led led in leds)
            _ledsUpdated(led, GetUpdateData(led).color);
    }
}
