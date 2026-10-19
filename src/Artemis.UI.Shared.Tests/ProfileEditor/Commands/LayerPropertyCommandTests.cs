using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.UI.Shared.Tests.ProfileEditor.Commands;

/// <summary>
///     Changing and resetting property values, data bindings, color gradients and timeline segments
/// </summary>
public class LayerPropertyCommandTests : ProfileEditorTestBase
{
    private readonly Layer _layer;
    private readonly FloatLayerProperty _opacity;

    public LayerPropertyCommandTests()
    {
        _layer = AddLayer("Layer", null, LedId.Keyboard_A);
        _opacity = _layer.Transform.Opacity;
    }

    private LayerPropertyKeyframe<float> AddKeyframe(float value, double seconds)
    {
        LayerPropertyKeyframe<float> keyframe = new(value, TimeSpan.FromSeconds(seconds), Easings.Functions.Linear, _opacity);
        _opacity.AddKeyframe(keyframe);
        return keyframe;
    }

    [Fact]
    public void UpdateLayerProperty_WithoutKeyframes_ChangesTheValue()
    {
        string before = Snapshot();
        UpdateLayerProperty<float> command = new(_opacity, 40, TimeSpan.FromSeconds(1));

        command.Execute();
        command.DisplayName.ShouldBe("Update Opacity");
        _opacity.BaseValue.ShouldBe(40);
        _opacity.CurrentValue.ShouldBe(40);

        command.Undo();
        _opacity.BaseValue.ShouldBe(100);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void UpdateLayerProperty_WithKeyframes_AddsAKeyframeAtTheTime()
    {
        AddKeyframe(0, 0);
        string before = Snapshot();
        UpdateLayerProperty<float> command = new(_opacity, 40, TimeSpan.FromSeconds(1));

        command.Execute();
        command.DisplayName.ShouldBe("Add Opacity keyframe");
        LayerPropertyKeyframe<float> added = _opacity.Keyframes.Last();
        added.Position.ShouldBe(TimeSpan.FromSeconds(1));
        added.Value.ShouldBe(40);

        command.Undo();
        _opacity.Keyframes.Count.ShouldBe(1);
        Snapshot().ShouldBe(before);

        command.Execute();
        _opacity.Keyframes.Last().ShouldBe(added);
    }

    [Fact]
    public void UpdateLayerProperty_OnAKeyframe_ChangesItsValue()
    {
        AddKeyframe(0, 0);
        LayerPropertyKeyframe<float> keyframe = AddKeyframe(60, 1);
        _layer.OverrideTimelineAndApply(TimeSpan.FromSeconds(1));
        string before = Snapshot();
        UpdateLayerProperty<float> command = new(_opacity, 40, TimeSpan.FromSeconds(1));

        command.Execute();
        command.DisplayName.ShouldBe("Update Opacity");
        _opacity.Keyframes.Count.ShouldBe(2);
        keyframe.Value.ShouldBe(40);

        command.Undo();
        keyframe.Value.ShouldBe(60);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void UpdateLayerProperty_OnAKeyframe_UndoesToTheValueAtTheCurrentTime()
    {
        AddKeyframe(0, 0);
        LayerPropertyKeyframe<float> keyframe = AddKeyframe(60, 2);
        // The property shows the value of the timeline's position, which is between the two keyframes
        _layer.OverrideTimelineAndApply(TimeSpan.FromSeconds(1));
        UpdateLayerProperty<float> command = new(_opacity, 40, TimeSpan.FromSeconds(2));

        command.Execute();
        command.Undo();

        // The original value is the value the property had at the time the command was created, not the keyframe's own
        // value. The editor always edits keyframes at the current time, where the two are the same
        keyframe.Value.ShouldBe(30, 0.01f);
    }

    [Fact]
    public void UpdateLayerProperty_WithOriginalValue_ChangesTheValue()
    {
        // The property inputs show a preview while dragging and pass the value from before the preview
        _opacity.SetCurrentValue(55);
        UpdateLayerProperty<float> command = new(_opacity, 40, 100, TimeSpan.FromSeconds(1));

        command.Execute();
        _opacity.BaseValue.ShouldBe(40);

        command.Undo();
        _opacity.BaseValue.ShouldBe(100);
    }

    [Fact]
    public void UpdateLayerProperty_WithOriginalValue_OnAKeyframe_RemovesTheKeyframeOnUndo()
    {
        AddKeyframe(0, 0);
        LayerPropertyKeyframe<float> keyframe = AddKeyframe(60, 1);
        UpdateLayerProperty<float> command = new(_opacity, 40, 60, TimeSpan.FromSeconds(1));

        command.Execute();
        keyframe.Value.ShouldBe(40);
        command.Undo();

        // Pinned as-is: this constructor never checks whether there already is a keyframe at the time. When keyframes are
        // enabled, Execute treats the existing keyframe that SetCurrentValue updated as a new one, and Undo removes it
        // instead of restoring its value. Changing a keyframe through a property input and undoing deletes the keyframe
        _opacity.Keyframes.ShouldNotContain(keyframe);
        _opacity.Keyframes.Count.ShouldBe(1);
        command.DisplayName.ShouldBe("Add Opacity keyframe");
    }

    [Fact]
    public void UpdateLayerProperty_WithoutTime_ChangesTheBaseValueEvenWithKeyframes()
    {
        AddKeyframe(0, 0);
        UpdateLayerProperty<float> command = new(_opacity, 40, null);

        command.Execute();

        _opacity.BaseValue.ShouldBe(40);
        _opacity.Keyframes.Count.ShouldBe(1);
        // Pinned as-is: with keyframes enabled the command expects to add a keyframe and renames itself, even though
        // without a time no keyframe is added
        command.DisplayName.ShouldBe("Add Opacity keyframe");

        command.Undo();
        _opacity.BaseValue.ShouldBe(0, 0.01f);
    }

    [Fact]
    public void ResetLayerProperty_RestoresTheDefaultAndTurnsOffKeyframes()
    {
        _opacity.SetCurrentValue(40);
        AddKeyframe(10, 0);
        string before = Snapshot();
        ResetLayerProperty<float> command = new(_opacity);

        command.Execute();
        command.DisplayName.ShouldBe("Reset layer property");
        _opacity.BaseValue.ShouldBe(100);
        _opacity.KeyframesEnabled.ShouldBeFalse();
        _opacity.Keyframes.Count.ShouldBe(1);

        command.Undo();
        _opacity.BaseValue.ShouldBe(40);
        _opacity.KeyframesEnabled.ShouldBeTrue();
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void ResetLayerProperty_WithoutDefault_Throws()
    {
        _layer.General.BrushReference.DefaultValue.ShouldBeNull();

        Should.Throw<ArtemisSharedUIException>(() => new ResetLayerProperty<LayerBrushReference>(_layer.General.BrushReference));
    }

    [Fact]
    public void ChangeDataBindingEnabled_TogglesAndUndoRestores()
    {
        string before = Snapshot();
        ChangeDataBindingEnabled enable = new(_opacity, true);

        enable.Execute();
        enable.DisplayName.ShouldBe("Enable data binding");
        _opacity.BaseDataBinding.IsEnabled.ShouldBeTrue();

        enable.Undo();
        _opacity.BaseDataBinding.IsEnabled.ShouldBeFalse();
        Snapshot().ShouldBe(before);

        new ChangeDataBindingEnabled(_opacity, false).DisplayName.ShouldBe("Disable data binding");
    }

    [Fact]
    public void UpdateColorGradient_ReplacesTheStopsAndUndoRestoresThem()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)];
        UpdateColorGradient command = new(gradient, [new ColorGradientStop(SKColors.Lime, 0.2f), new ColorGradientStop(SKColors.White, 0.5f), new ColorGradientStop(SKColors.Black, 0.9f)], null);

        command.Execute();
        command.DisplayName.ShouldBe("Update color gradient");
        gradient.Select(s => (s.Color, s.Position)).ShouldBe([(SKColors.Lime, 0.2f), (SKColors.White, 0.5f), (SKColors.Black, 0.9f)]);

        command.Undo();
        gradient.Select(s => (s.Color, s.Position)).ShouldBe([(SKColors.Red, 0f), (SKColors.Blue, 1f)]);
    }

    [Fact]
    public void UpdateColorGradient_UpdatesExistingStopsInPlace()
    {
        ColorGradientStop first = new(SKColors.Red, 0);
        ColorGradientStop second = new(SKColors.Blue, 1);
        ColorGradient gradient = [first, second];
        UpdateColorGradient command = new(gradient, [new ColorGradientStop(SKColors.Lime, 0.3f)], null);

        command.Execute();
        gradient.ShouldBe([first]);
        first.Color.ShouldBe(SKColors.Lime);

        command.Undo();
        gradient[0].ShouldBe(first);
        gradient.Count.ShouldBe(2);
        gradient[1].ShouldNotBeSameAs(second);
    }

    [Fact]
    public void UpdateColorGradient_KeepsItsOwnCopyOfTheStops()
    {
        ColorGradient gradient = [new ColorGradientStop(SKColors.Red, 0)];
        ColorGradientStop newStop = new(SKColors.Lime, 0.5f);
        UpdateColorGradient command = new(gradient, [newStop], null);

        newStop.Color = SKColors.Black;
        command.Execute();

        gradient[0].Color.ShouldBe(SKColors.Lime);
    }

    [Fact]
    public void UpdateColorGradient_WithOriginalStops_UndoesToThoseStops()
    {
        // The gradient editor changes the gradient while editing and passes the stops from before
        ColorGradient gradient = [new ColorGradientStop(SKColors.Lime, 0.5f)];
        UpdateColorGradient command = new(gradient, [new ColorGradientStop(SKColors.Lime, 0.5f)], [new ColorGradientStop(SKColors.Red, 0), new ColorGradientStop(SKColors.Blue, 1)]);

        command.Execute();
        command.Undo();

        gradient.Select(s => (s.Color, s.Position)).ShouldBe([(SKColors.Red, 0f), (SKColors.Blue, 1f)]);
    }

    [Theory]
    [InlineData(ResizeTimelineSegment.SegmentType.Start, "Resize start segment")]
    [InlineData(ResizeTimelineSegment.SegmentType.Main, "Resize main segment")]
    [InlineData(ResizeTimelineSegment.SegmentType.End, "Resize end segment")]
    public void ResizeTimelineSegment_ResizesAndUndoRestoresTheLength(ResizeTimelineSegment.SegmentType segmentType, string displayName)
    {
        _layer.Timeline.StartSegmentLength = TimeSpan.FromSeconds(1);
        _layer.Timeline.EndSegmentLength = TimeSpan.FromSeconds(2);
        string before = Snapshot();
        ResizeTimelineSegment command = new(segmentType, _layer, TimeSpan.FromSeconds(7));

        command.Execute();
        command.DisplayName.ShouldBe(displayName);
        SegmentLength(segmentType).ShouldBe(TimeSpan.FromSeconds(7));

        command.Undo();
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void ResizeTimelineSegment_WithOriginalLength_UndoesToThatLength()
    {
        // The timeline resizes segments while dragging and passes the length from before the drag
        _layer.Timeline.MainSegmentLength = TimeSpan.FromSeconds(6);
        ResizeTimelineSegment command = new(ResizeTimelineSegment.SegmentType.Main, _layer, TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(5));

        command.Execute();
        command.Undo();

        _layer.Timeline.MainSegmentLength.ShouldBe(TimeSpan.FromSeconds(5));
    }

    private TimeSpan SegmentLength(ResizeTimelineSegment.SegmentType segmentType)
    {
        return segmentType switch
        {
            ResizeTimelineSegment.SegmentType.Start => _layer.Timeline.StartSegmentLength,
            ResizeTimelineSegment.SegmentType.Main => _layer.Timeline.MainSegmentLength,
            _ => _layer.Timeline.EndSegmentLength
        };
    }
}
