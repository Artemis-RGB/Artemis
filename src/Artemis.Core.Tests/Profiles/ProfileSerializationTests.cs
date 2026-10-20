using Artemis.Core.LayerEffects;
using Artemis.Core.LayerEffects.Placeholder;
using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using Artemis.Storage.Entities.Profile;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

public class ProfileSerializationTests : RenderTestBase
{
    [Fact]
    public void RoundTrip_RendersIdentically()
    {
        Profile profile = CreateComplexProfile();
        Profile reloaded = Harness.SaveAndReload(profile);

        // New layers start at the end of their timeline while loaded layers start at the beginning, reset both
        profile.Reset();
        reloaded.Reset();
        using RenderTimeline original = Harness.RenderTimeline(profile, TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.75), TimeSpan.FromSeconds(1.5));
        using RenderTimeline loaded = RenderTimeline(reloaded, 0.25, 0.75, 1.5);

        for (int i = 0; i < original.Frames.Count; i++)
        {
            foreach ((ArtemisLed led, SKColor color) in original[i].LedColors)
                loaded[i].LedColors[led].ShouldBeColor(color);
        }
    }

    [Fact]
    public void RoundTrip_PreservesTheTree()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder(), "Folder");
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)], "Nested layer");
        (Layer suspended, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_S)], "Suspended layer");
        Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Leds(LedId.Keyboard_D, LedId.Keyboard_F), "Last layer");
        suspended.Suspended = true;
        folder.Suspended = true;

        Profile reloaded = Harness.SaveAndReload(profile);

        Folder root = reloaded.GetRootFolder();
        root.Children.Select(c => c.Name).ShouldBe(["Folder", "Suspended layer", "Last layer"]);
        root.Children.Select(c => c.Order).ShouldBe([1, 2, 3]);
        root.Children.Select(c => c.Suspended).ShouldBe([true, true, false]);
        root.Children[0].Children.Single().Name.ShouldBe("Nested layer");
        ((Layer) root.Children[2]).Leds.Select(l => l.RgbLed.Id).ShouldBe([LedId.Keyboard_D, LedId.Keyboard_F], ignoreOrder: true);
    }

    [Fact]
    public void RoundTrip_PreservesPropertiesAndKeyframes()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Transform.Rotation.SetCurrentValue(45);
        layer.General.ShapeType.SetCurrentValue(LayerShapeType.Ellipse);
        SKColorLayerProperty color = brush.Properties.Color;
        color.AddKeyframe(new LayerPropertyKeyframe<SKColor>(SKColors.Red, TimeSpan.Zero, Easings.Functions.CubicEaseIn, color));
        color.AddKeyframe(new LayerPropertyKeyframe<SKColor>(SKColors.Blue, TimeSpan.FromSeconds(1), Easings.Functions.Linear, color));

        Profile reloaded = Harness.SaveAndReload(profile);

        Layer reloadedLayer = reloaded.GetAllLayers().Single();
        reloadedLayer.Transform.Rotation.CurrentValue.ShouldBe(45);
        reloadedLayer.General.ShapeType.CurrentValue.ShouldBe(LayerShapeType.Ellipse);
        SKColorLayerProperty reloadedColor = reloadedLayer.LayerBrush.ShouldBeOfType<SolidColorTestBrush>().Properties.Color;
        reloadedColor.KeyframesEnabled.ShouldBeTrue();
        reloadedColor.Keyframes.Select(k => (k.Value, k.Position, k.EasingFunction)).ShouldBe([
            (SKColors.Red, TimeSpan.Zero, Easings.Functions.CubicEaseIn),
            (SKColors.Blue, TimeSpan.FromSeconds(1), Easings.Functions.Linear)
        ]);
    }

    [Fact]
    public void RoundTrip_PreservesTimelinesAndDisplayConditions()
    {
        Profile profile = Harness.CreateProfile();
        (Layer playOnce, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)], "Play once");
        (Layer staticCondition, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_S)], "Static");
        playOnce.Timeline.StartSegmentLength = TimeSpan.FromSeconds(1);
        playOnce.Timeline.MainSegmentLength = TimeSpan.FromSeconds(2);
        playOnce.Timeline.EndSegmentLength = TimeSpan.FromSeconds(3);
        TestConditions.UsePlayOnceCondition(playOnce);
        staticCondition.DisplayCondition = new StaticCondition(staticCondition) {PlayMode = StaticPlayMode.Once, StopMode = StaticStopMode.SkipToEnd};

        Profile reloaded = Harness.SaveAndReload(profile);

        Layer reloadedPlayOnce = reloaded.GetAllLayers().Single(l => l.Name == "Play once");
        reloadedPlayOnce.Timeline.StartSegmentLength.ShouldBe(TimeSpan.FromSeconds(1));
        reloadedPlayOnce.Timeline.MainSegmentLength.ShouldBe(TimeSpan.FromSeconds(2));
        reloadedPlayOnce.Timeline.EndSegmentLength.ShouldBe(TimeSpan.FromSeconds(3));
        reloadedPlayOnce.DisplayCondition.ShouldBeOfType<PlayOnceCondition>();
        StaticCondition reloadedStatic = reloaded.GetAllLayers().Single(l => l.Name == "Static").DisplayCondition.ShouldBeOfType<StaticCondition>();
        reloadedStatic.PlayMode.ShouldBe(StaticPlayMode.Once);
        reloadedStatic.StopMode.ShouldBe(StaticStopMode.SkipToEnd);
    }

    [Fact]
    public void RoundTrip_PreservesEffects()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)]);
        OpacityTestEffect opacity = Harness.AddEffect<OpacityTestEffect>(layer);
        TintTestEffect tint = Harness.AddEffect<TintTestEffect>(layer);
        Harness.AddEffect<ChannelSwapTestEffect>(folder);
        opacity.Properties.Opacity.SetCurrentValue(25);
        tint.Properties.IsEnabled.SetCurrentValue(false);

        Profile reloaded = Harness.SaveAndReload(profile);

        Layer reloadedLayer = reloaded.GetAllLayers().Single();
        reloadedLayer.LayerEffects.Select(e => e.GetType()).ShouldBe([typeof(TintTestEffect), typeof(OpacityTestEffect)]);
        reloadedLayer.LayerEffects.OfType<OpacityTestEffect>().Single().Properties.Opacity.CurrentValue.ShouldBe(25);
        reloadedLayer.LayerEffects.OfType<TintTestEffect>().Single().Suspended.ShouldBeTrue();
        reloaded.GetAllFolders().Single(f => !f.IsRootFolder).LayerEffects.Single().ShouldBeOfType<ChannelSwapTestEffect>();
    }

    [Fact]
    public void RoundTrip_KeepsTheNameOfRenamedEffects()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        TintTestEffect tint = Harness.AddEffect<TintTestEffect>(layer);
        tint.Name = "Renamed tint";
        tint.HasBeenRenamed = true;

        Profile reloaded = Harness.SaveAndReload(profile);

        BaseLayerEffect reloadedTint = reloaded.GetAllLayers().Single().LayerEffects.Single();
        reloadedTint.HasBeenRenamed.ShouldBeTrue();
        reloadedTint.Name.ShouldBe("Renamed tint");
    }

    [Fact]
    public void RoundTrip_UsesTheDisplayNameOfEffectsThatWereNotRenamed()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        Harness.AddEffect<TintTestEffect>(layer);

        Profile reloaded = Harness.SaveAndReload(profile);

        BaseLayerEffect reloadedTint = reloaded.GetAllLayers().Single().LayerEffects.Single();
        reloadedTint.HasBeenRenamed.ShouldBeFalse();
        reloadedTint.Name.ShouldBe("Tint");
    }

    [Fact]
    public void MissingBrushPlugin_BreaksTheLayerWithoutBreakingTheProfile()
    {
        Profile profile = Harness.CreateProfile();
        Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)], "Broken");
        ProfileEntity entity = Harness.Save(profile);
        // Disposing the harness unregisters its test plugin, the same plugin is not available in the next harness
        Harness.Dispose();

        using RenderHarness other = new();
        ArtemisDevice keyboard = other.AddDevice(TestLayouts.KeyboardAnsi);
        Profile reloaded = other.Load(entity);
        using RenderFrame frame = other.Render(reloaded, TimeSpan.FromSeconds(0.1));
        SaveOutput(frame, TestName);

        Layer layer = reloaded.GetAllLayers().Single();
        layer.LayerBrush.ShouldBeNull();
        layer.BrokenState.ShouldNotBeNull();
        frame.GetColor(keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void MissingEffectPlugin_KeepsTheEffectAsAPlaceholder()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<PerLedTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        Harness.AddEffect<ChannelSwapTestEffect>(layer);
        ProfileEntity entity = Harness.Save(profile);
        string providerId = entity.Layers.Single().LayerEffects.Single().ProviderId;
        Harness.Dispose();

        using RenderHarness other = new();
        other.AddDevice(TestLayouts.KeyboardAnsi);
        Profile reloaded = other.Load(entity);

        BaseLayerEffect placeholder = reloaded.GetAllLayers().Single().LayerEffects.Single();
        placeholder.ShouldBeOfType<PlaceholderLayerEffect>();
        // Saving again keeps the original effect so it comes back once the plugin is available again
        other.Save(reloaded).Layers.Single().LayerEffects.Single().ProviderId.ShouldBe(providerId);
    }

    private Profile CreateComplexProfile()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder(), "Folder");
        Harness.AddEffect<ChannelSwapTestEffect>(folder);

        (Layer gradient, GradientTestBrush gradientBrush) = Harness.AddLayer<GradientTestBrush>(folder, TopLetterRow, "Gradient");
        gradientBrush.Properties.Left.SetCurrentValue(SKColors.Red);
        gradientBrush.Properties.Right.SetCurrentValue(SKColors.Lime);
        gradient.Transform.Rotation.SetCurrentValue(180);
        FloatLayerProperty opacity = gradient.Transform.Opacity;
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(20, TimeSpan.Zero, Easings.Functions.QuadraticEaseOut, opacity));
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(100, TimeSpan.FromSeconds(1), Easings.Functions.Linear, opacity));

        (Layer tinted, SolidColorTestBrush tintedBrush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Leds(LedId.Keyboard_A, LedId.Keyboard_S, LedId.Keyboard_D), "Tinted");
        tintedBrush.Properties.Color.SetCurrentValue(SKColors.White);
        tinted.General.ShapeType.SetCurrentValue(LayerShapeType.Ellipse);
        TintTestEffect tint = Harness.AddEffect<TintTestEffect>(tinted);
        tint.Properties.Color.SetCurrentValue(new SKColor(0, 128, 255));

        (Layer suspended, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), Keyboard.Leds, "Suspended");
        suspended.Suspended = true;

        return profile;
    }
}
