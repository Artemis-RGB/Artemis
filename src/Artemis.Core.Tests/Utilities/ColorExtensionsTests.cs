using Artemis.Core.Tests.Infrastructure;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Utilities;

public class ColorExtensionsTests
{
    [Fact]
    public void ToRgbColor_KeepsEveryChannel()
    {
        Color color = new SKColor(10, 20, 30, 40).ToRgbColor();

        color.GetR().ShouldBe((byte) 10);
        color.GetG().ShouldBe((byte) 20);
        color.GetB().ShouldBe((byte) 30);
        color.GetA().ShouldBe((byte) 40);
    }

    [Theory]
    [InlineData(0f, 0, 0, 0, 0)]
    [InlineData(0.5f, 50, 100, 127, 127)]
    [InlineData(1f, 100, 200, 255, 255)]
    public void Interpolate_BlendsEveryChannel(float progress, byte red, byte green, byte blue, byte alpha)
    {
        SKColor from = new(0, 0, 0, 0);
        SKColor to = new(100, 200, 255, 255);

        from.Interpolate(to, progress).ShouldBe(new SKColor(red, green, blue, alpha));
    }

    [Fact]
    public void Interpolate_ClampsOutsideOfTheRange()
    {
        SKColor from = new(100, 100, 100);
        SKColor to = new(200, 200, 200);

        from.Interpolate(to, 2f).ShouldBe(new SKColor(255, 255, 255));
        from.Interpolate(to, -2f).ShouldBe(new SKColor(0, 0, 0));
    }

    [Fact]
    public void Interpolate_CanGoDown()
    {
        SKColors.White.Interpolate(SKColors.Black, 0.25f).ShouldBe(new SKColor(191, 191, 191));
    }

    [Fact]
    public void Sum_AddsEveryChannelAndClamps()
    {
        new SKColor(100, 200, 50, 100).Sum(new SKColor(100, 100, 10, 100)).ShouldBe(new SKColor(200, 255, 60, 200));
    }

    [Fact]
    public void Darken_LowersTheLightness()
    {
        SKColors.Red.Darken(0.5f).ShouldBeColor(new SKColor(128, 0, 0), 1);
        SKColors.Red.Darken(0f).ShouldBe(SKColors.Red);
        SKColors.Red.Darken(1f).ShouldBe(SKColors.Black);
    }

    [Fact]
    public void Darken_KeepsTheAlpha()
    {
        SKColors.Red.WithAlpha(100).Darken(0.5f).Alpha.ShouldBe((byte) 100);
    }

    [Theory]
    [InlineData(0.4f, 0)]
    [InlineData(0.5f, 1)]
    [InlineData(1.5f, 2)]
    [InlineData(2.5f, 3)]
    [InlineData(-0.5f, -1)]
    [InlineData(-1.6f, -2)]
    public void RoundToInt_RoundsHalfwayAwayFromZero(float number, int expected)
    {
        number.RoundToInt().ShouldBe(expected);
        ((double) number).RoundToInt().ShouldBe(expected);
    }

    [Fact]
    public void IndexOf_FindsTheFirstEqualElement()
    {
        IReadOnlyCollection<string?> collection = ["a", "b", null, "b"];

        collection.IndexOf("b").ShouldBe(1);
        collection.IndexOf(null).ShouldBe(2);
        collection.IndexOf("c").ShouldBe(-1);
    }

    [Fact]
    public void ToSKRect_KeepsTheRectangle()
    {
        Rectangle rectangle = new(1.4f, 2.6f, 3f, 4f);

        rectangle.ToSKRect().ShouldBe(SKRect.Create(1.4f, 2.6f, 3f, 4f));
        rectangle.ToSKRectI().ShouldBe(new SKRectI(1, 3, 4, 7));
    }

    [Fact]
    public void DisposeSelfAndProperties_DisposesTheShader()
    {
        SKShader shader = SKShader.CreateColor(SKColors.Red);
        SKPaint paint = new() {Shader = shader};

        paint.DisposeSelfAndProperties();

        shader.Handle.ShouldBe(IntPtr.Zero);
        paint.Handle.ShouldBe(IntPtr.Zero);
    }
}
