using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;

namespace Artemis.Core.Tests.Profiles.Adaption;

/// <summary>
///     Base class for adaption tests, gives each test its own harness so tests can pick their own devices
/// </summary>
public abstract class AdaptionTestBase : IDisposable
{
    protected RenderHarness Harness { get; } = new();

    protected ArtemisDevice AddDevice(string layoutPath, float x = 0, float y = 0)
    {
        return Harness.AddDevice(layoutPath, x, y);
    }

    protected List<ArtemisDevice> Devices => Harness.Devices.ToList();

    protected Layer AddLayer(Profile profile, IEnumerable<ArtemisLed> leds, string name = "Test layer")
    {
        return Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), leds, name).Layer;
    }

    protected Layer AddLayer(Profile profile, string name = "Test layer")
    {
        return AddLayer(profile, [], name);
    }

    protected static ArtemisLed Led(ArtemisDevice device, LedId ledId)
    {
        return device.GetLed(ledId, false) ?? throw new ArgumentException($"{device.RgbDevice.DeviceInfo.Model} has no LED {ledId}", nameof(ledId));
    }

    protected static ArtemisLed[] Leds(ArtemisDevice device, params LedId[] ledIds)
    {
        return ledIds.Select(id => Led(device, id)).ToArray();
    }

    /// <summary>
    ///     Gets the IDs of the layer's LEDs on the given device, sorted so they can be compared
    /// </summary>
    protected static LedId[] LedIdsOn(Layer layer, ArtemisDevice device)
    {
        return layer.Leds.Where(l => l.Device == device).Select(l => l.RgbLed.Id).Order().ToArray();
    }

    /// <summary>
    ///     Gets the IDs of all LEDs on the given device, sorted so they can be compared
    /// </summary>
    protected static LedId[] AllLedIds(ArtemisDevice device)
    {
        return device.Leds.Select(l => l.RgbLed.Id).Order().ToArray();
    }

    public void Dispose()
    {
        Harness.Dispose();
        GC.SuppressFinalize(this);
    }
}
