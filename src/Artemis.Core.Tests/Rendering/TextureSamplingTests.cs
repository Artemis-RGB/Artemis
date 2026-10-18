using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Rendering;

/// <summary>
///     How SKTexture turns the canvas into LED colors: every LED gets the average color of the canvas pixels under its
///     rectangle.
/// </summary>
public class TextureSamplingTests : RenderTestBase
{
    [Fact]
    public void LedColor_IsTheAverageOfTheCanvasUnderTheLed()
    {
        // A is 18 units wide, at the default render scale that is 9 canvas pixels. Fill the left 4 of them
        SKRect a = Led(LedId.Keyboard_A).AbsoluteRectangle;

        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, new SKRect(a.Left, a.Top, a.Left + 8, a.Bottom), SKColors.Red));
        SaveOutput(frame);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(255 * 4 / 9, 0, 0));
    }

    [Fact]
    public void FillingTheLedRectangle_DoesNotBleedIntoNeighbours()
    {
        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, Led(LedId.Keyboard_A).AbsoluteRectangle, SKColors.Red));
        SaveOutput(frame);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_CapsLock).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_Q).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_Z).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void Canvas_IsClearedToBlackEveryFrame()
    {
        Harness.Render(canvas => Fill(canvas, Led(LedId.Keyboard_A).AbsoluteRectangle, SKColors.Red)).Dispose();

        using RenderFrame frame = Harness.Render(_ => { });
        SaveOutput(frame);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void TransparentCanvasPixels_AreNotSentToTheDevice()
    {
        Harness.Render(canvas => Fill(canvas, Led(LedId.Keyboard_A).AbsoluteRectangle, SKColors.Red)).Dispose();

        using RenderFrame frame = Harness.Render(canvas => canvas.Clear(SKColors.Transparent));
        SaveOutput(frame);

        // RGB.NET only sends LEDs whose color has some alpha, so the device keeps showing red. Profiles can't cause this,
        // the root folder is composited onto the canvas, which is cleared to opaque black every frame
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void SemiTransparentDrawing_IsBlendedOntoBlack()
    {
        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, Led(LedId.Keyboard_A).AbsoluteRectangle, new SKColor(255, 0, 0, 128)));
        SaveOutput(frame);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(128, 0, 0));
    }

    [Fact]
    public void LedRectangles_AreSnappedToWholeCanvasPixels()
    {
        // At the default render scale of 0.5 a canvas pixel covers two surface units, LED rectangles are snapped to even
        // values so they line up with the pixels. The RGB.NET boundary of A is (40.25, 77) 19x19
        ArtemisLed a = Led(LedId.Keyboard_A);

        a.RgbLed.AbsoluteBoundary.Location.X.ShouldBe(40.25f, 0.01f);
        a.AbsoluteRectangle.ShouldBe(new SKRect(40, 76, 58, 94));
        Keyboard.Leds.ShouldAllBe(l => l.AbsoluteRectangle.Left % 2 == 0 && l.AbsoluteRectangle.Top % 2 == 0 &&
                                       l.AbsoluteRectangle.Width % 2 == 0 && l.AbsoluteRectangle.Height % 2 == 0);
    }

    private static void Fill(SKCanvas canvas, SKRect rect, SKColor color)
    {
        using SKPaint paint = new() {Color = color};
        canvas.DrawRect(rect, paint);
    }
}
