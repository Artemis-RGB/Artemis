using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Rendering;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

public class LayerEffectTests : RenderTestBase
{
    [Fact]
    public void PreProcessEffect_ModifiesThePaintTheLayerIsCompositedWith()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        Harness.AddEffect<OpacityTestEffect>(layer);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
    }

    [Fact]
    public void PreProcessEffect_StacksWithTheLayerOpacity()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Transform.Opacity.SetCurrentValue(50);
        Harness.AddEffect<OpacityTestEffect>(layer);

        using RenderFrame frame = Render(profile);

        // 50% of 50%
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(63, 0, 0));
    }

    [Fact]
    public void PreProcessColorFilter_IsAppliedOnceOnLayers()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        Harness.AddEffect<ChannelSwapTestEffect>(layer);

        using RenderFrame frame = Render(profile);

        // Swapping red and blue twice would cancel out
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void PreProcessColorFilter_IsAppliedOnceOnPerLedBrushes()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, PerLedTestBrush brush) = Harness.AddLayer<PerLedTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        brush.ColorSelector = (_, _) => SKColors.Red;
        Harness.AddEffect<ChannelSwapTestEffect>(layer);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void PostProcessEffect_DrawsOverTheBrushOutput()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Leds(LedId.Keyboard_A, LedId.Keyboard_S));
        brush.Properties.Color.SetCurrentValue(SKColors.White);
        Harness.AddEffect<TintTestEffect>(layer);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void PostProcessEffect_IsClippedToTheLayer()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush tinted) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)], "Tinted");
        (_, SolidColorTestBrush neighbour) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_S)], "Neighbour");
        tinted.Properties.Color.SetCurrentValue(SKColors.White);
        neighbour.Properties.Color.SetCurrentValue(SKColors.White);
        Harness.AddEffect<TintTestEffect>(layer);

        using RenderFrame frame = Render(profile);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        frame.GetColor(Keyboard, LedId.Keyboard_S).ShouldBeColor(SKColors.White);
    }

    [Fact]
    public void PostProcessEffect_RunsBeforeThePreProcessedPaintIsApplied()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        brush.Properties.Color.SetCurrentValue(SKColors.White);
        Harness.AddEffect<TintTestEffect>(layer);
        Harness.AddEffect<ChannelSwapTestEffect>(layer);

        using RenderFrame frame = Render(profile);

        // The tint turns white into red inside the layer, the filter of the composited layer then turns red into blue
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void DisabledEffect_IsSkipped()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        ChannelSwapTestEffect effect = Harness.AddEffect<ChannelSwapTestEffect>(layer);
        effect.Properties.IsEnabled.SetCurrentValue(false);

        using RenderFrame frame = Render(profile);

        effect.Suspended.ShouldBeTrue();
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void RemovedEffect_IsNoLongerApplied()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        ChannelSwapTestEffect effect = Harness.AddEffect<ChannelSwapTestEffect>(layer);
        Harness.Render(profile, TimeSpan.FromSeconds(0.1)).Dispose();

        layer.RemoveLayerEffect(effect);

        using RenderFrame frame = Render(profile);
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void EffectProperties_CanBeAnimatedWithKeyframes()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        OpacityTestEffect effect = Harness.AddEffect<OpacityTestEffect>(layer);
        FloatLayerProperty opacity = effect.Properties.Opacity;
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(100, TimeSpan.Zero, Easings.Functions.Linear, opacity));
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(0, TimeSpan.FromSeconds(1), Easings.Functions.Linear, opacity));
        profile.Reset();

        using RenderTimeline timeline = RenderTimeline(profile, 0.25, 0.75);

        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(191, 0, 0));
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(63, 0, 0));
    }

    [Fact]
    public void AddLayerEffect_InsertsNewEffectsFirst()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);

        OpacityTestEffect first = Harness.AddEffect<OpacityTestEffect>(layer);
        TintTestEffect second = Harness.AddEffect<TintTestEffect>(layer);

        // New effects have an order of 0 so OrderEffects sorts them before the existing ones, the editor adds them the same way
        layer.LayerEffects.ShouldBe([second, first]);
        layer.LayerEffects.Select(e => e.Order).ShouldBe([1, 2]);
    }
}
