using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Rendering;

/// <summary>
///     How SurfaceManager keeps the texture in sync with the devices on the surface, and how device placement and color
///     correction affect what each LED receives.
/// </summary>
public class SurfaceTests : RenderTestBase
{
    #region Placement

    [Fact]
    public void DevicePosition_MovesWhereItsLedsSample()
    {
        Keyboard.X = 200;
        Keyboard.Y = 100;
        SKRect a = Led(LedId.Keyboard_A).AbsoluteRectangle;

        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, a, SKColors.Red));
        SaveOutput(frame);

        a.Left.ShouldBe(240);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        // The surface always starts at the origin, so the canvas grows with the offset
        frame.Canvas.Width.ShouldBe(321);
    }

    [Fact]
    public void MovingADevice_RecreatesTheTexture()
    {
        SKRect originalA = Led(LedId.Keyboard_A).AbsoluteRectangle;
        using (RenderFrame before = Harness.Render(canvas => Fill(canvas, originalA, SKColors.Red)))
            before.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);

        Keyboard.X = 100;
        using RenderFrame after = Harness.Render(canvas => Fill(canvas, originalA, SKColors.Red));
        SaveOutput(after);

        // A moved away from the red rectangle, if the texture still used the old LED positions A would remain red
        after.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
        after.Canvas.Width.ShouldBe(271);
    }

    [Fact]
    public void AddingADevice_GrowsTheTextureAndSamplesBothDevices()
    {
        ArtemisDevice second = Harness.AddDevice(TestLayouts.KeyboardIso, 500, 0);

        using RenderFrame frame = Harness.Render(canvas =>
        {
            Fill(canvas, Led(LedId.Keyboard_A).AbsoluteRectangle, SKColors.Red);
            Fill(canvas, second.GetLed(LedId.Keyboard_A, false)!.AbsoluteRectangle, SKColors.Blue);
        });
        SaveOutput(frame);

        frame.Canvas.Width.ShouldBeGreaterThan(450);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(second, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void RemovedDevice_KeepsShowingItsLastColors()
    {
        ArtemisDevice second = Harness.AddDevice(TestLayouts.KeyboardIso, 500, 0);
        SKRect secondA = second.GetLed(LedId.Keyboard_A, false)!.AbsoluteRectangle;
        Harness.Render(canvas => Fill(canvas, secondA, SKColors.Blue)).Dispose();

        Harness.RemoveDevice(second);
        using RenderFrame frame = Harness.Render(_ => { });
        SaveOutput(frame);

        // Nothing sends the device new colors, like real hardware it keeps whatever it received last
        frame.LedColors.Keys.ShouldNotContain(second.Leds[0]);
        frame.Canvas.Width.ShouldBe(221);
    }

    [Fact]
    public void OverlappingDevices_BothSampleTheSameCanvasRegardlessOfZIndex()
    {
        ArtemisDevice overlapping = Harness.AddDevice(TestLayouts.KeyboardAnsi);
        overlapping.ZIndex = 10;
        Keyboard.ZIndex = 1;

        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, Led(LedId.Keyboard_A).AbsoluteRectangle, SKColors.Red));
        SaveOutput(frame);

        // The Z-index only orders devices in the device list, rendering doesn't use it
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(overlapping, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void RotatingADevice_DoesNotUpdateItsLedsUntilItIsMoved()
    {
        ArtemisLed a = Led(LedId.Keyboard_A);
        SKRect beforeRotation = a.AbsoluteRectangle;

        Keyboard.Rotation = 90;

        // Pinned as-is: the Rotation and Scale setters only update the RGB.NET device. The Artemis LEDs, and with them the
        // texture, keep their old positions until X or Y is set. The device properties dialog gets away with this because
        // it reloads the device layout after saving
        a.AbsoluteRectangle.ShouldBe(beforeRotation);
        Math.Abs(a.RgbLed.AbsoluteBoundary.Location.X - beforeRotation.Left).ShouldBeGreaterThan(1);

        Keyboard.X = Keyboard.X;
        a.AbsoluteRectangle.ShouldNotBe(beforeRotation);
    }

    [Fact]
    public void RotatedDevice_SamplesFromItsRotatedLedPositions()
    {
        Keyboard.Rotation = 90;
        Keyboard.X = Keyboard.X;
        SKRect rotatedA = Led(LedId.Keyboard_A).AbsoluteRectangle;

        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, rotatedA, SKColors.Red));
        SaveOutput(frame);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void ScaledDevice_SamplesFromItsScaledLedPositions()
    {
        Keyboard.Scale = 2;
        Keyboard.X = Keyboard.X;
        SKRect scaledA = Led(LedId.Keyboard_A).AbsoluteRectangle;

        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, scaledA, SKColors.Red));
        SaveOutput(frame);

        scaledA.Width.ShouldBeInRange(36, 40);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void DeviceAtANegativePosition_LedsOutsideTheTextureReceiveNothing()
    {
        ArtemisDevice outside = Harness.AddDevice(TestLayouts.KeyboardIso, -500, 0);

        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, SKRect.Create(-1000, -1000, 3000, 3000), SKColors.Red));
        SaveOutput(frame);

        // The texture starts at the origin, sample rectangles are clamped to it so LEDs beyond it get no color (#906).
        // Before that they made SKTexture throw, which broke the frame for every device
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        outside.Leds.Where(l => l.AbsoluteRectangle.Right <= 0).ShouldAllBe(l => frame.LedColors[l] == SKColors.Transparent);
    }

    [Fact]
    public void SurfaceBeyondTheMaximumTextureSize_LedsOutsideTheTextureReceiveNothing()
    {
        // At a render scale of 0.5 the 4096 pixel texture limit is reached at 8192 surface units
        ArtemisDevice outside = Harness.AddDevice(TestLayouts.KeyboardIso, 9000, 0);

        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, SKRect.Create(0, 0, 20000, 1000), SKColors.Red));

        frame.Canvas.Width.ShouldBe(4096);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        outside.Leds.ShouldAllBe(l => frame.LedColors[l] == SKColors.Transparent);
    }

    #endregion

    #region Color correction

    [Theory]
    [InlineData(0.5f, 1f, 1f, 127, 255, 255)]
    [InlineData(1f, 0f, 1f, 255, 0, 255)]
    [InlineData(1f, 1f, 0.25f, 255, 255, 63)]
    public void ColorScales_AreAppliedToWhatTheDeviceReceives(float red, float green, float blue, byte expectedRed, byte expectedGreen, byte expectedBlue)
    {
        Keyboard.RedScale = red;
        Keyboard.GreenScale = green;
        Keyboard.BlueScale = blue;

        using RenderFrame frame = Harness.Render(canvas => Fill(canvas, Led(LedId.Keyboard_A).AbsoluteRectangle, SKColors.White));

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(expectedRed, expectedGreen, expectedBlue));
        // The canvas itself is not corrected
        SKRectI pixel = SKRectI.Round(Led(LedId.Keyboard_A).AbsoluteRectangle);
        frame.Canvas.GetPixel(pixel.MidX / 2, pixel.MidY / 2).ShouldBe(SKColors.White);
    }

    [Fact]
    public void ColorScalesAboveOne_AreClampedToFullBrightness()
    {
        Keyboard.RedScale = 2;

        using RenderFrame frame = Harness.Render(canvas =>
        {
            Fill(canvas, Led(LedId.Keyboard_A).AbsoluteRectangle, new SKColor(100, 0, 0));
            Fill(canvas, Led(LedId.Keyboard_S).AbsoluteRectangle, new SKColor(200, 0, 0));
        });

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(200, 0, 0));
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(new SKColor(255, 0, 0));
    }

    [Fact]
    public void ColorScales_OnlyAffectTheirOwnDevice()
    {
        ArtemisDevice second = Harness.AddDevice(TestLayouts.KeyboardIso, 500, 0);
        Keyboard.RedScale = 0;

        using RenderFrame frame = Harness.Render(canvas =>
        {
            Fill(canvas, Led(LedId.Keyboard_A).AbsoluteRectangle, SKColors.Red);
            Fill(canvas, second.GetLed(LedId.Keyboard_A, false)!.AbsoluteRectangle, SKColors.Red);
        });

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
        frame.GetColor(second, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    #endregion

    private static void Fill(SKCanvas canvas, SKRect rect, SKColor color)
    {
        using SKPaint paint = new() {Color = color};
        canvas.DrawRect(rect, paint);
    }
}
