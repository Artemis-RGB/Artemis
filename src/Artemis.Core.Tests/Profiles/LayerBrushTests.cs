using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

public class LayerBrushTests : RenderTestBase
{
    [Fact]
    public void SolidBrush_DefaultsToItsDefaultValue()
    {
        Profile profile = Harness.CreateProfile();
        Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void GradientBrush_SpansTheLayerBounds()
    {
        Profile profile = Harness.CreateProfile();
        Harness.AddLayer<GradientTestBrush>(profile.GetRootFolder(), TopLetterRow);

        using RenderFrame frame = Render(profile);

        byte[] brightness = TopLetterRow.Select(l => frame.LedColors[l].Red).ToArray();
        brightness.ShouldBeInOrder(SortDirection.Ascending);
        brightness.First().ShouldBeLessThan((byte) 32);
        brightness.Last().ShouldBeGreaterThan((byte) 223);
    }

    [Fact]
    public void PerLedBrush_ColorsEachLedIndividually()
    {
        Profile profile = Harness.CreateProfile();
        (_, PerLedTestBrush brush) = Harness.AddLayer<PerLedTestBrush>(profile.GetRootFolder(), Leds(LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D));
        brush.ColorSelector = (led, _) => led.RgbLed.Id switch
        {
            LedId.Keyboard_A => SKColors.Red,
            LedId.Keyboard_S => SKColors.Lime,
            _ => SKColors.Blue
        };

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Lime);
        frame.GetColor(Keyboard, LedId.Keyboard_D).ShouldBeColor(SKColors.Blue);
        frame.GetColor(Keyboard, LedId.Keyboard_F).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void PerLedBrush_ReceivesPointsRelativeToTheLayer()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, PerLedTestBrush brush) = Harness.AddLayer<PerLedTestBrush>(profile.GetRootFolder(), TopLetterRow);
        Dictionary<LedId, SKPoint> points = [];
        brush.ColorSelector = (led, point) =>
        {
            points[led.RgbLed.Id] = point;
            return SKColors.White;
        };

        using RenderFrame frame = Render(profile);

        // The point is the top-left of the LED relative to the top-left of the layer
        points[LedId.Keyboard_Q].ShouldBe(new SKPoint(Led(LedId.Keyboard_Q).AbsoluteRectangle.Left - layer.Bounds.Left, Led(LedId.Keyboard_Q).AbsoluteRectangle.Top - layer.Bounds.Top));
        points[LedId.Keyboard_P].X.ShouldBeGreaterThan(points[LedId.Keyboard_Q].X);
    }

    [Fact]
    public void ChangeLayerBrush_ReplacesTheBrush()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);

        PerLedTestBrush replacement = Harness.ChangeBrush<PerLedTestBrush>(layer);
        replacement.ColorSelector = (_, _) => SKColors.Blue;

        using RenderFrame frame = Render(profile);

        layer.LayerBrush.ShouldBe(replacement);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }
}
