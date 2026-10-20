using RGB.NET.Core;

namespace Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;

internal sealed record TestRGBDeviceInfo(RGBDeviceType DeviceType, string Manufacturer, string Model) : IRGBDeviceInfo
{
    public string DeviceName => $"{Manufacturer} {Model}";
    public object? LayoutMetadata { get; set; }
}
