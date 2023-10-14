using System.Linq;
using System.Text;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core;

internal static class RgbDeviceExtensions
{
    public static string GetDeviceIdentifier(this IRGBDevice rgbDevice)
    {
        StringBuilder builder = new();
        builder.Append(rgbDevice.DeviceInfo.DeviceName);
        builder.Append('-');
        builder.Append(rgbDevice.DeviceInfo.Manufacturer);
        builder.Append('-');
        builder.Append(rgbDevice.DeviceInfo.Model);
        builder.Append('-');
        builder.Append(rgbDevice.DeviceInfo.DeviceType);
        return builder.ToString();
    }

    public static void EnsureValidDimensions(this IRGBDevice rgbDevice)
    {
        if (rgbDevice.Location == Point.Invalid)
            rgbDevice.Location = new Point(0, 0);
        
        if (rgbDevice.Size == Size.Invalid)
        {
            Rectangle ledRectangle = new(rgbDevice.Select(x => x.Boundary));
            rgbDevice.Size = ledRectangle.Size + new Size(ledRectangle.Location.X, ledRectangle.Location.Y);
        }
    }
}

internal static class RgbRectangleExtensions
{
    public static SKRect ToSKRect(this Rectangle rectangle)
    {
        return SKRect.Create(
            rectangle.Location.X,
            rectangle.Location.Y,
            rectangle.Size.Width,
            rectangle.Size.Height
        );
    }

    public static SKRectI ToSKRectI(this Rectangle rectangle)
    {
        return SKRectI.Round(ToSKRect(rectangle));
    }
}