using RGB.NET.Core;
using RGB.NET.Layout;

namespace Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;

internal class TestRGBDeviceProvider : AbstractRGBDeviceProvider
{
    public List<(IDeviceLayout Layout, Action<Led, Color> LedsUpdated)> Definitions { get; } = [];

    protected override void InitializeSDK()
    {
    }

    protected override IEnumerable<IRGBDevice> LoadDevices()
    {
        return Definitions.Select(d => new TestRGBDevice(d.Layout, d.LedsUpdated));
    }
}
