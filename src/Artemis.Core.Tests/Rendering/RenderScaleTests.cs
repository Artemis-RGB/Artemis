using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Rendering;

/// <summary>
///     The render scale sets the size of the canvas relative to the surface. Each test creates its own harness since the
///     scale is fixed per harness.
/// </summary>
public class RenderScaleTests
{
    public static TheoryData<float, int, int> CanvasSizes => new()
    {
        // The 441x144 keyboard is first rounded up to even dimensions, then scaled and rounded to whole pixels
        {1f, 442, 144},
        {0.5f, 221, 72},
        {0.25f, 111, 36}
    };

    public static TheoryData<float> RenderScales => new(1f, 0.5f, 0.25f);

    [Theory]
    [MemberData(nameof(CanvasSizes))]
    public void CanvasSize_FollowsTheRenderScale(float renderScale, int width, int height)
    {
        using RenderHarness harness = new(renderScale);
        harness.AddDevice(TestLayouts.KeyboardAnsi);

        using RenderFrame frame = harness.Render(_ => { });

        frame.Canvas.Width.ShouldBe(width);
        frame.Canvas.Height.ShouldBe(height);
    }

    [Theory]
    [MemberData(nameof(RenderScales))]
    public void LedSizedShapes_RenderTheSameAtEveryScale(float renderScale)
    {
        using RenderHarness harness = new(renderScale);
        ArtemisDevice keyboard = harness.AddDevice(TestLayouts.KeyboardAnsi);
        (LedId Led, SKColor Color)[] fills = [(LedId.Keyboard_A, SKColors.Red), (LedId.Keyboard_S, SKColors.Lime), (LedId.Keyboard_Enter, SKColors.Blue), (LedId.Keyboard_Space, SKColors.Yellow)];

        using RenderFrame frame = harness.Render(canvas =>
        {
            foreach ((LedId led, SKColor color) in fills)
            {
                using SKPaint paint = new() {Color = color};
                canvas.DrawRect(keyboard.GetLed(led, false)!.AbsoluteRectangle, paint);
            }
        });

        foreach ((LedId led, SKColor color) in fills)
            frame.GetColor(keyboard, led).ShouldBeColor(color, 2, led.ToString());
        frame.GetColor(keyboard, LedId.Keyboard_D).ShouldBeColor(SKColors.Black);
    }

    [Theory]
    [MemberData(nameof(RenderScales))]
    public void LedRectangles_AreSnappedToTheRenderScale(float renderScale)
    {
        using RenderHarness harness = new(renderScale);
        ArtemisDevice keyboard = harness.AddDevice(TestLayouts.KeyboardAnsi);

        int multiplier = (int) (1 / renderScale);
        keyboard.Leds.ShouldAllBe(l => l.AbsoluteRectangle.Left % multiplier == 0 && l.AbsoluteRectangle.Top % multiplier == 0 &&
                                       l.AbsoluteRectangle.Width % multiplier == 0 && l.AbsoluteRectangle.Height % multiplier == 0);
    }

    [Fact]
    public void LowRenderScale_RoundsThinDetailUpToWholeCanvasPixels()
    {
        // A two unit wide line through A, at a render scale of 0.25 that is half a canvas pixel
        using RenderHarness full = new(1f);
        SKColor fullColor = RenderLine(full);
        using RenderHarness quarter = new(0.25f);
        SKColor quarterColor = RenderLine(quarter);

        // At full scale the line covers about a tenth of the LED. At a quarter A is only 4x4 canvas pixels and the line,
        // drawn without anti-aliasing, fills a whole column of them: a quarter of the LED
        fullColor.Red.ShouldBeInRange((byte) 15, (byte) 40);
        quarterColor.ShouldBeColor(new SKColor(64, 0, 0), 3);
    }

    private static SKColor RenderLine(RenderHarness harness)
    {
        ArtemisDevice keyboard = harness.AddDevice(TestLayouts.KeyboardAnsi);
        SKRect a = keyboard.GetLed(LedId.Keyboard_A, false)!.AbsoluteRectangle;
        using RenderFrame frame = harness.Render(canvas =>
        {
            using SKPaint paint = new() {Color = SKColors.Red};
            canvas.DrawRect(SKRect.Create(a.MidX, a.Top, 2, a.Height), paint);
        });
        return frame.GetColor(keyboard, LedId.Keyboard_A);
    }
}
