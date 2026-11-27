using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

public class LayerTransformTests : RenderTestBase
{
    [Fact]
    public void Opacity_IsAppliedWhenCompositingTheLayer()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Transform.Opacity.SetCurrentValue(50);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red.AtOpacity(50));
    }

    [Fact]
    public void Opacity_AppliesToPerLedBrushes()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, PerLedTestBrush brush) = Harness.AddLayer<PerLedTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        brush.ColorSelector = (_, _) => SKColors.Red;
        layer.Transform.Opacity.SetCurrentValue(50);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red.AtOpacity(50));
    }

    [Fact]
    public void Opacity_AppliesOnceToGradientBrushes()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, GradientTestBrush brush) = Harness.AddLayer<GradientTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        brush.Properties.Left.SetCurrentValue(SKColors.Red);
        brush.Properties.Right.SetCurrentValue(SKColors.Red);
        layer.Transform.Opacity.SetCurrentValue(50);

        using RenderFrame frame = Render(profile);

        // The gradient shader keeps the alpha of the paint, the brush must not get the layer opacity on top of compositing
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red.AtOpacity(50));
    }

    [Fact]
    public void Rotation_RotatesAroundTheAnchor()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<GradientTestBrush>(profile.GetRootFolder(), TopLetterRow);
        layer.Transform.Rotation.SetCurrentValue(180);

        using RenderFrame frame = Render(profile);

        // Rotating the left to right gradient half a turn around the center makes it run right to left
        TopLetterRow.Select(l => frame.LedColors[l].Red).ToArray().ShouldBeInOrder(SortDirection.Descending);
    }

    [Fact]
    public void Scale_ShrinksTowardsTheAnchor()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), TopLetterRow);
        layer.Transform.Scale.SetCurrentValue(new SKSize(50, 100));

        using RenderFrame frame = Render(profile);

        // Half the width, centered: the outer keys fall outside of the shape
        frame.GetColor(Keyboard, LedId.Keyboard_Q).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_W).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_T).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_Y).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_O).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_P).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void Position_MovesTheShapeRelativeToTheLayerSize()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), TopLetterRow);
        // The anchor sits at the center, moving the position to the right edge moves the shape right by half its width
        layer.Transform.Position.SetCurrentValue(new SKPoint(1f, 0.5f));

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_Q).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_R).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_I).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_P).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void Position_DoesNotMoveTheLedsTheLayerIsClippedTo()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), TopLetterRow);
        layer.Transform.Position.SetCurrentValue(new SKPoint(1f, 0.5f));

        using RenderFrame frame = Render(profile);

        // The shape moved beyond P but the layer remains clipped to its own LEDs
        frame.GetColor(Keyboard, LedId.Keyboard_BracketLeft).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void EllipseShape_LeavesTheCornersOut()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Keyboard.Leds);
        layer.General.ShapeType.SetCurrentValue(LayerShapeType.Ellipse);

        using RenderFrame frame = Render(profile);

        SKPoint center = new(layer.Bounds.MidX, layer.Bounds.MidY);
        ArtemisLed centerLed = Keyboard.Leds.MinBy(l => SKPoint.Distance(new SKPoint(l.AbsoluteRectangle.MidX, l.AbsoluteRectangle.MidY), center))!;
        frame.LedColors[centerLed].ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_Escape).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.Keyboard_LeftCtrl).ShouldBeColor(SKColors.Black);
        frame.GetColor(Keyboard, LedId.LedStripe1).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void BlendMode_IsUsedToCompositeTheLayer()
    {
        Profile profile = Harness.CreateProfile();
        (Layer top, SolidColorTestBrush topBrush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)], "Top");
        (_, SolidColorTestBrush bottomBrush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)], "Bottom");
        topBrush.Properties.Color.SetCurrentValue(SKColors.Blue);
        bottomBrush.Properties.Color.SetCurrentValue(SKColors.Red);
        top.General.BlendMode.SetCurrentValue(SKBlendMode.Plus);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Magenta);
    }
}
