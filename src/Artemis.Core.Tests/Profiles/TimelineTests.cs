using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Tests.Profiles;

public class TimelineTests : RenderTestBase
{
    private static readonly SKColor Purple = new(128, 0, 128);

    #region Keyframes

    [Fact]
    public void Keyframes_InterpolateLinearly()
    {
        Profile profile = Harness.CreateProfile();
        (_, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        AddKeyframes(brush.Properties.Color, (0, SKColors.Red), (1, SKColors.Blue));

        using RenderTimeline timeline = RenderTimeline(profile, 0.25, 0.5, 1);

        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(191, 0, 64));
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(Purple);
        timeline[2].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void Keyframes_UseTheEasingOfTheEarlierKeyframe()
    {
        Profile profile = Harness.CreateProfile();
        (_, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        SKColorLayerProperty color = brush.Properties.Color;
        color.AddKeyframe(new LayerPropertyKeyframe<SKColor>(SKColors.Black, TimeSpan.Zero, Easings.Functions.CubicEaseIn, color));
        color.AddKeyframe(new LayerPropertyKeyframe<SKColor>(SKColors.White, TimeSpan.FromSeconds(1), Easings.Functions.Linear, color));

        using RenderFrame frame = Render(profile, 0.5);

        // Cubic ease-in at the halfway point is 0.5^3 = 0.125
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(31, 31, 31));
    }

    [Fact]
    public void Keyframes_HoldTheirValueBeforeTheFirstAndAfterTheLast()
    {
        Profile profile = Harness.CreateProfile();
        (_, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        AddKeyframes(brush.Properties.Color, (1, SKColors.Red), (2, SKColors.Blue));

        using RenderTimeline timeline = RenderTimeline(profile, 0.5, 3);

        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Blue);
    }

    [Fact]
    public void Keyframes_AddedOutOfOrder_AreSortedByPosition()
    {
        Profile profile = Harness.CreateProfile();
        (_, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        AddKeyframes(brush.Properties.Color, (1, SKColors.Blue), (0, SKColors.Red));

        using RenderFrame frame = Render(profile, 0.5);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(Purple);
    }

    [Fact]
    public void Keyframes_AreIgnoredWhenDisabled()
    {
        Profile profile = Harness.CreateProfile();
        (_, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        AddKeyframes(brush.Properties.Color, (0, SKColors.Red), (1, SKColors.Blue));
        brush.Properties.Color.KeyframesEnabled = false;
        brush.Properties.Color.SetCurrentValue(SKColors.Lime);

        using RenderFrame frame = Render(profile, 0.5);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Lime);
    }

    [Fact]
    public void Keyframes_AnimateLayerProperties()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        FloatLayerProperty opacity = layer.Transform.Opacity;
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(0, TimeSpan.Zero, Easings.Functions.Linear, opacity));
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(100, TimeSpan.FromSeconds(1), Easings.Functions.Linear, opacity));

        using RenderFrame frame = Render(profile, 0.5);

        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red.AtOpacity(50));
    }

    #endregion

    #region Segments

    [Fact]
    public void AlwaysOn_RepeatsTheMainSegment()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(2);
        AddKeyframes(brush.Properties.Color, (0, SKColors.Red), (2, SKColors.Blue));

        // Small steps so wrapping around does not depend on the frame size, see below
        using RenderTimeline timeline = RenderTimeline(profile, 1, 1.9, 2, 2.1, 3);

        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(Purple);
        timeline[3].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(242, 0, 13));
        timeline[4].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(Purple);
        layer.Timeline.IsFinished.ShouldBeFalse();
    }

    [Fact]
    public void AlwaysOn_WrapsAroundByTheOvershoot()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(2);
        AddKeyframes(brush.Properties.Color, (0, SKColors.Red), (2, SKColors.Blue));

        // Going from 1.5s to 2.5s overshoots the 2s main segment by 0.5s, so the timeline lands on 0.5s
        using RenderTimeline timeline = RenderTimeline(profile, 1.5, 2.5);

        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(0.5));
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(191, 0, 64));
    }

    [Fact]
    public void NewLayer_StartsWithAFinishedTimeline()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);

        // Layer.Initialize calls Reset while the display condition is not met yet, which jumps to the end
        layer.Timeline.Position.ShouldBe(layer.Timeline.Length);
    }

    [Fact]
    public void NewFolder_StartsAtTheBeginningOfItsTimeline()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());

        // Unlike layers, new folders are not reset on creation
        folder.Timeline.Position.ShouldBe(TimeSpan.Zero);
        profile.GetRootFolder().Timeline.Position.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void LoadedLayers_StartAtTheBeginningOfTheirTimeline()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)]);

        Profile reloaded = Harness.SaveAndReload(profile);

        // Folder.Load resets the folder before loading its children, so the parent condition is met when the layer resets
        reloaded.GetAllLayers().Single().Timeline.Position.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void LoadedFolders_StartWithAFinishedTimeline()
    {
        Profile profile = Harness.CreateProfile();
        Harness.AddFolder(profile.GetRootFolder());

        Profile reloaded = Harness.SaveAndReload(profile);

        // Folder.Load replaces the timeline after resetting, and Timeline.Load jumps to the end
        foreach (Folder folder in reloaded.GetAllFolders())
            folder.Timeline.Position.ShouldBe(folder.Timeline.Length);
    }

    [Fact]
    public void AlwaysOn_NewLayer_SkipsTheStartSegment()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.StartSegmentLength = TimeSpan.FromSeconds(1);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(1);
        AddKeyframes(brush.Properties.Color, (0, SKColors.Black), (1, SKColors.Red));

        using RenderFrame frame = Render(profile, 0.5);

        // The timeline starts finished and wraps straight into the main segment, skipping the start segment.
        // Only applies to layers created in code, the editor resets the profile and loaded layers start at zero
        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(1.5));
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    [Fact]
    public void AlwaysOn_AfterReset_PlaysTheStartSegmentOnce()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.StartSegmentLength = TimeSpan.FromSeconds(1);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(1);
        // Fades in from black during the start segment, then holds red during the main segment
        AddKeyframes(brush.Properties.Color, (0, SKColors.Black), (1, SKColors.Red));
        profile.Reset();

        using RenderTimeline timeline = RenderTimeline(profile, 0.5, 1.5, 2.1, 2.5, 3.5);

        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        timeline[2].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        timeline[3].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        timeline[4].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        layer.Timeline.Position.ShouldBeGreaterThanOrEqualTo(layer.Timeline.MainSegmentStartPosition);
    }

    #endregion

    #region Display conditions

    [Fact]
    public void PlayOnce_NewLayer_NeverPlays()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(1);
        TestConditions.UsePlayOnceCondition(layer);

        using RenderFrame frame = Render(profile, 0.5);

        // The timeline starts finished and play once never restarts it.
        // Only applies to layers created in code, the editor resets the profile and loaded layers start at zero
        layer.Timeline.IsFinished.ShouldBeTrue();
        frame.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void PlayOnce_LoadedLayer_PlaysOnceThenStops()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(1);
        TestConditions.UsePlayOnceCondition(layer);

        Profile reloaded = Harness.SaveAndReload(profile);
        using RenderTimeline timeline = RenderTimeline(reloaded, 0.5, 1.5);

        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void PlayOnce_AfterReset_StopsRenderingAfterTheTimeline()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(1);
        TestConditions.UsePlayOnceCondition(layer);
        profile.Reset();

        using RenderTimeline timeline = RenderTimeline(profile, 0.5, 1.5);

        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
        layer.Timeline.IsFinished.ShouldBeTrue();
    }

    [Fact]
    public void StaticCondition_RendersOnlyWhileMet()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        TestBooleanNode condition = TestConditions.UseStaticCondition(layer, false, stopMode: StaticStopMode.SkipToEnd);

        using (RenderFrame notMet = Harness.Render(profile, TimeSpan.FromSeconds(0.1)))
            notMet.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);

        condition.Value = true;
        using (RenderFrame met = Harness.Render(profile, TimeSpan.FromSeconds(0.1)))
            met.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);

        condition.Value = false;
        using RenderFrame metAgain = Render(profile);
        metAgain.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void StaticCondition_StopModeFinish_FinishesTheMainSegmentFirst()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(2);
        TestBooleanNode condition = TestConditions.UseStaticCondition(layer, true, stopMode: StaticStopMode.Finish);

        using (RenderFrame met = Harness.Render(profile, TimeSpan.FromSeconds(0.5)))
            met.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);

        condition.Value = false;
        using RenderTimeline timeline = RenderTimeline(profile, 1, 2.5);

        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void StaticCondition_StopModeSkipToEnd_PlaysTheEndSegment()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(2);
        layer.Timeline.EndSegmentLength = TimeSpan.FromSeconds(1);
        // Red during the main segment, fading to black during the end segment
        AddKeyframes(brush.Properties.Color, (2, SKColors.Red), (3, SKColors.Black));
        TestBooleanNode condition = TestConditions.UseStaticCondition(layer, true, stopMode: StaticStopMode.SkipToEnd);

        using (RenderFrame met = Harness.Render(profile, TimeSpan.FromSeconds(0.5)))
            met.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);

        condition.Value = false;
        using RenderTimeline timeline = RenderTimeline(profile, 1, 1.6);

        // Jumped to the start of the end segment (2s) and advanced 0.5s into it
        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(127, 0, 0));
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
        layer.Timeline.IsFinished.ShouldBeTrue();
    }

    [Fact]
    public void StaticCondition_PlayModeOnce_StopsAfterTheMainSegmentWhileStillMet()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(1);
        TestConditions.UseStaticCondition(layer, true, StaticPlayMode.Once);

        using RenderTimeline timeline = RenderTimeline(profile, 0.5, 1.5);

        timeline[0].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);
    }

    [Fact]
    public void StaticCondition_RestartsTheTimelineWhenMetAgainAfterFinishing()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, SolidColorTestBrush brush) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(2);
        AddKeyframes(brush.Properties.Color, (0, SKColors.Red), (2, SKColors.Blue));
        TestBooleanNode condition = TestConditions.UseStaticCondition(layer, true, stopMode: StaticStopMode.SkipToEnd);

        RenderTimeline(profile, 1).Dispose();
        condition.Value = false;
        RenderTimeline(profile, 1.1).Dispose();
        layer.Timeline.IsFinished.ShouldBeTrue();

        condition.Value = true;
        using RenderTimeline timeline = RenderTimeline(profile, 1.2, 2.2);

        // Starts over from the beginning of the timeline, 1.1s in after the second frame
        layer.Timeline.Position.ShouldBe(TimeSpan.FromSeconds(1.1));
        timeline[1].GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(new SKColor(115, 0, 140));
    }

    [Fact]
    public void ParentFolderCondition_AppliesToItsChildren()
    {
        Profile profile = Harness.CreateProfile();
        Folder folder = Harness.AddFolder(profile.GetRootFolder());
        Harness.AddLayer<SolidColorTestBrush>(folder, [Led(LedId.Keyboard_A)]);
        TestBooleanNode condition = TestConditions.UseStaticCondition(folder, false);

        using (RenderFrame notMet = Harness.Render(profile, TimeSpan.FromSeconds(0.1)))
            notMet.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Black);

        condition.Value = true;
        using RenderFrame met = Render(profile);
        met.GetColor(Keyboard, LedId.Keyboard_A).ShouldBeColor(SKColors.Red);
    }

    #endregion

    private static void AddKeyframes(SKColorLayerProperty property, params (double Seconds, SKColor Color)[] keyframes)
    {
        foreach ((double seconds, SKColor color) in keyframes)
            property.AddKeyframe(new LayerPropertyKeyframe<SKColor>(color, TimeSpan.FromSeconds(seconds), Easings.Functions.Linear, property));
    }
}
