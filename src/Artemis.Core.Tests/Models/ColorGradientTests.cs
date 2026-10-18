using SkiaSharp;

namespace Artemis.Core.Tests.Models;

public class ColorGradientTests
{
    [Theory]
    [InlineData(0f, 255, 0, 0)]
    [InlineData(0.5f, 127, 0, 127)]
    [InlineData(1f, 0, 0, 255)]
    public void GetColor_InterpolatesBetweenStops(float position, byte red, byte green, byte blue)
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];

        gradient.GetColor(position).ShouldBe(new SKColor(red, green, blue));
    }

    [Fact]
    public void GetColor_ClampsOutsideOfStops()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0.25f), new ColorGradientStop(SKColors.Blue, 0.75f)];

        gradient.GetColor(0f).ShouldBe(SKColors.Red);
        gradient.GetColor(1f).ShouldBe(SKColors.Blue);
    }
}
