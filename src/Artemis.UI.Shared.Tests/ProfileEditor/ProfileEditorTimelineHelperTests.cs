using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor;

/// <summary>
///     Keyframe selection, snapping and rounding used by the timeline
/// </summary>
public class ProfileEditorTimelineHelperTests : ProfileEditorTestBase
{
    private readonly Layer _layer;
    private readonly LayerPropertyKeyframe<float> _first;
    private readonly LayerPropertyKeyframe<float> _second;
    private readonly LayerPropertyKeyframe<float> _third;

    public ProfileEditorTimelineHelperTests()
    {
        _layer = AddLayer("Layer", null, LedId.Keyboard_A);
        FloatLayerProperty opacity = _layer.Transform.Opacity;
        opacity.KeyframesEnabled = true;
        _first = new LayerPropertyKeyframe<float>(0, TimeSpan.Zero, Easings.Functions.Linear, opacity);
        _second = new LayerPropertyKeyframe<float>(50, TimeSpan.FromSeconds(1), Easings.Functions.Linear, opacity);
        _third = new LayerPropertyKeyframe<float>(100, TimeSpan.FromSeconds(2), Easings.Functions.Linear, opacity);
        opacity.AddKeyframe(_first);
        opacity.AddKeyframe(_second);
        opacity.AddKeyframe(_third);
    }

    [Fact]
    public void SelectKeyframe_ReplacesTheSelection()
    {
        Service.SelectKeyframe(_first, false, false);
        Service.SelectKeyframe(_second, false, false);

        Service.SelectedKeyframes.ShouldBe([_second]);
    }

    [Fact]
    public void SelectKeyframe_Expanding_AddsToTheSelection()
    {
        Service.SelectKeyframe(_first, false, false);
        Service.SelectKeyframe(_second, true, false);

        Service.SelectedKeyframes.ShouldBe([_first, _second]);
    }

    [Fact]
    public void SelectKeyframe_ExpandingWithAKeyframeAlreadySelected_SelectsItTwice()
    {
        Service.SelectKeyframe(_first, false, false);
        Service.SelectKeyframe(_first, true, false);

        // Pinned as-is: unlike SelectKeyframes, expanding the selection does not check whether the keyframe is already
        // selected, so it ends up in the selection twice
        Service.SelectedKeyframes.ShouldBe([_first, _first]);
    }

    [Fact]
    public void SelectKeyframe_Toggling_FlipsOnlyThatKeyframe()
    {
        Service.SelectKeyframe(_first, false, false);

        Service.SelectKeyframe(_second, false, true);
        Service.SelectedKeyframes.ShouldBe([_first, _second]);

        Service.SelectKeyframe(_first, false, true);
        Service.SelectedKeyframes.ShouldBe([_second]);
    }

    [Fact]
    public void SelectKeyframe_Nothing_ClearsTheSelectionUnlessExpanding()
    {
        Service.SelectKeyframe(_first, false, false);

        Service.SelectKeyframe(null, true, false);
        Service.SelectedKeyframes.ShouldBe([_first]);

        Service.SelectKeyframe(null, false, false);
        Service.SelectedKeyframes.ShouldBeEmpty();
    }

    [Fact]
    public void SelectKeyframes_ReplacesTheSelection()
    {
        Service.SelectKeyframe(_first, false, false);

        Service.SelectKeyframes([_second, _third], false);

        Service.SelectedKeyframes.ShouldBe([_second, _third]);
    }

    [Fact]
    public void SelectKeyframes_Expanding_AddsKeyframesNotYetSelected()
    {
        Service.SelectKeyframe(_first, false, false);

        Service.SelectKeyframes([_first, _second], true);

        Service.SelectedKeyframes.ShouldBe([_first, _second]);
    }

    [Theory]
    [InlineData(1050, 1000)] // close to the end of the start segment
    [InlineData(5950, 6000)] // close to the end of the main segment
    [InlineData(7090, 7000)] // close to the end of the end segment
    [InlineData(1100, 1100)] // the tolerance is exclusive
    [InlineData(3000, 3000)] // nowhere near a segment
    public void SnapToTimeline_SnapsToTheEndsOfTheSelectedElementsSegments(int time, int expected)
    {
        _layer.Timeline.StartSegmentLength = TimeSpan.FromSeconds(1);
        _layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(5);
        _layer.Timeline.EndSegmentLength = TimeSpan.FromSeconds(1);
        Service.ChangeCurrentProfileElement(_layer);

        TimeSpan result = Service.SnapToTimeline(TimeSpan.FromMilliseconds(time), TimeSpan.FromMilliseconds(100), true, false);

        result.ShouldBe(TimeSpan.FromMilliseconds(expected));
    }

    [Fact]
    public void SnapToTimeline_WithoutSnappingToSegments_IgnoresSegments()
    {
        _layer.Timeline.StartSegmentLength = TimeSpan.FromSeconds(1);
        Service.ChangeCurrentProfileElement(_layer);

        Service.SnapToTimeline(TimeSpan.FromMilliseconds(1050), TimeSpan.FromMilliseconds(100), false, false)
            .ShouldBe(TimeSpan.FromMilliseconds(1050));
    }

    [Fact]
    public void SnapToTimeline_WithoutSelection_IgnoresSegments()
    {
        Service.SnapToTimeline(TimeSpan.FromMilliseconds(4950), TimeSpan.FromMilliseconds(100), true, false)
            .ShouldBe(TimeSpan.FromMilliseconds(4950));
    }

    [Fact]
    public void SnapToTimeline_SnapsToTheCurrentTime()
    {
        Service.ChangeTime(TimeSpan.FromSeconds(3));

        Service.SnapToTimeline(TimeSpan.FromMilliseconds(3040), TimeSpan.FromMilliseconds(100), false, true).ShouldBe(TimeSpan.FromSeconds(3));
        Service.SnapToTimeline(TimeSpan.FromMilliseconds(3040), TimeSpan.FromMilliseconds(100), false, false).ShouldBe(TimeSpan.FromMilliseconds(3040));
    }

    [Fact]
    public void SnapToTimeline_SnapsToTheFirstCloseSnapTime()
    {
        List<TimeSpan> snapTimes = [TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(560), TimeSpan.FromMilliseconds(540)];

        Service.SnapToTimeline(TimeSpan.FromMilliseconds(550), TimeSpan.FromMilliseconds(100), false, false, snapTimes)
            .ShouldBe(TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void SnapToTimeline_NeverSnapsToASnapTimeOfZero()
    {
        List<TimeSpan> snapTimes = [TimeSpan.Zero];

        // Pinned as-is: a snap time of zero can't be told apart from "no snap time found" (the default of TimeSpan), so
        // a keyframe at the very start of the timeline is never snapped to
        Service.SnapToTimeline(TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(100), false, false, snapTimes)
            .ShouldBe(TimeSpan.FromMilliseconds(30));
    }

    [Theory]
    [InlineData(40, 1290, 1200)] // zoomed far out: 200 ms steps
    [InlineData(80, 1260, 1300)] // 100 ms steps
    [InlineData(120, 1260, 1250)] // the default zoom level: 50 ms steps
    [InlineData(300, 1269, 1260)] // 20 ms steps
    [InlineData(600, 1269.4, 1269)] // zoomed far in: whole milliseconds
    public void RoundTime_RoundsToStepsThatFitTheZoomLevel(int pixelsPerSecond, double milliseconds, double expected)
    {
        Service.ChangePixelsPerSecond(pixelsPerSecond);

        Service.RoundTime(TimeSpan.FromMilliseconds(milliseconds)).ShouldBe(TimeSpan.FromMilliseconds(expected));
    }
}
