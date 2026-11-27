using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Tests.Infrastructure;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Tests.ProfileEditor.Commands;

/// <summary>
///     Deleting, duplicating, moving and easing keyframes, and turning keyframes on and off
/// </summary>
public class KeyframeCommandTests : ProfileEditorTestBase
{
    private readonly FloatLayerProperty _opacity;
    private readonly LayerPropertyKeyframe<float> _first;
    private readonly LayerPropertyKeyframe<float> _second;

    public KeyframeCommandTests()
    {
        Layer layer = AddLayer("Layer", null, LedId.Keyboard_A);
        _opacity = layer.Transform.Opacity;
        _opacity.KeyframesEnabled = true;
        _first = new LayerPropertyKeyframe<float>(20, TimeSpan.FromSeconds(1), Easings.Functions.Linear, _opacity);
        _second = new LayerPropertyKeyframe<float>(80, TimeSpan.FromSeconds(2), Easings.Functions.Linear, _opacity);
        _opacity.AddKeyframe(_first);
        _opacity.AddKeyframe(_second);
    }

    [Fact]
    public void DeleteKeyframe_RemovesTheKeyframe()
    {
        DeleteKeyframe command = new(_first);

        command.Execute();

        command.DisplayName.ShouldBe("Delete keyframe");
        _opacity.Keyframes.ShouldBe([_second]);
    }

    [Fact]
    public void DeleteKeyframe_Undo_PutsTheSameKeyframeBack()
    {
        string before = Snapshot();
        DeleteKeyframe command = new(_first);

        command.Execute();
        command.Undo();

        _opacity.Keyframes.ShouldBe([_first, _second]);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void DeleteKeyframe_Undo_OfTheLastKeyframe_TurnsKeyframesBackOn()
    {
        DeleteKeyframe deleteFirst = new(_first);
        DeleteKeyframe deleteSecond = new(_second);
        deleteFirst.Execute();
        deleteSecond.Execute();
        _opacity.KeyframesEnabled = false;

        deleteSecond.Undo();

        // Adding a keyframe always turns keyframes on
        _opacity.KeyframesEnabled.ShouldBeTrue();
    }

    [Fact]
    public void DuplicateKeyframe_AddsACopyAtThePosition()
    {
        _first.EasingFunction = Easings.Functions.CubicEaseIn;
        DuplicateKeyframe command = new(_first, TimeSpan.FromSeconds(3));

        command.Execute();

        command.DisplayName.ShouldBe("Duplicate keyframe");
        LayerPropertyKeyframe<float> copy = command.Duplication.ShouldBeOfType<LayerPropertyKeyframe<float>>();
        _opacity.Keyframes.ShouldBe([_first, _second, copy]);
        copy.Value.ShouldBe(20);
        copy.Position.ShouldBe(TimeSpan.FromSeconds(3));
        copy.EasingFunction.ShouldBe(Easings.Functions.CubicEaseIn);
    }

    [Fact]
    public void DuplicateKeyframe_UndoAndRedo_ReuseTheSameCopy()
    {
        string before = Snapshot();
        DuplicateKeyframe command = new(_first, TimeSpan.FromSeconds(3));
        command.Execute();
        ILayerPropertyKeyframe copy = command.Duplication!;

        command.Undo();
        Snapshot().ShouldBe(before);

        command.Execute();
        command.Duplication.ShouldBe(copy);
        _opacity.Keyframes.ShouldContain((LayerPropertyKeyframe<float>) copy);
    }

    [Fact]
    public void DuplicateKeyframe_Undo_BeforeExecute_DoesNothing()
    {
        new DuplicateKeyframe(_first, TimeSpan.FromSeconds(3)).Undo();

        _opacity.Keyframes.ShouldBe([_first, _second]);
    }

    [Fact]
    public void MoveKeyframe_MovesAndUndoRestoresThePosition()
    {
        string before = Snapshot();
        MoveKeyframe command = new(_first, TimeSpan.FromSeconds(3));

        command.Execute();
        command.DisplayName.ShouldBe("Move keyframe");
        _first.Position.ShouldBe(TimeSpan.FromSeconds(3));
        // Keyframes stay sorted by position
        _opacity.Keyframes.ShouldBe([_second, _first]);

        command.Undo();
        _first.Position.ShouldBe(TimeSpan.FromSeconds(1));
        _opacity.Keyframes.ShouldBe([_first, _second]);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void MoveKeyframe_WithAnOriginalPosition_UndoesToThatPosition()
    {
        // The timeline moves keyframes while dragging and passes where the drag started
        _first.Position = TimeSpan.FromSeconds(1.5);
        MoveKeyframe command = new(_first, TimeSpan.FromSeconds(1.75), TimeSpan.FromSeconds(1));

        command.Execute();
        _first.Position.ShouldBe(TimeSpan.FromSeconds(1.75));

        command.Undo();
        _first.Position.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ChangeKeyframeEasing_ChangesAndUndoRestoresTheEasing()
    {
        string before = Snapshot();
        ChangeKeyframeEasing command = new(_first, Easings.Functions.CubicEaseInOut);

        command.Execute();
        command.DisplayName.ShouldBe("Change easing to cubic ease in out");
        _first.EasingFunction.ShouldBe(Easings.Functions.CubicEaseInOut);

        command.Undo();
        _first.EasingFunction.ShouldBe(Easings.Functions.Linear);
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void ToggleLayerPropertyKeyframes_Enable_AddsAKeyframeWithTheCurrentValue()
    {
        FloatLayerProperty rotation = ((Layer) _opacity.ProfileElement).Transform.Rotation;
        rotation.SetCurrentValue(45);
        ToggleLayerPropertyKeyframes<float> command = new(rotation, true, TimeSpan.FromSeconds(2));

        command.Execute();

        command.DisplayName.ShouldBe("Enable keyframes");
        rotation.KeyframesEnabled.ShouldBeTrue();
        LayerPropertyKeyframe<float> keyframe = rotation.Keyframes.ShouldHaveSingleItem();
        keyframe.Value.ShouldBe(45);
        keyframe.Position.ShouldBe(TimeSpan.FromSeconds(2));
        keyframe.EasingFunction.ShouldBe(Easings.Functions.Linear);
    }

    [Fact]
    public void ToggleLayerPropertyKeyframes_Enable_UndoAndRedo()
    {
        FloatLayerProperty rotation = ((Layer) _opacity.ProfileElement).Transform.Rotation;
        string before = Snapshot();
        ToggleLayerPropertyKeyframes<float> command = new(rotation, true, TimeSpan.FromSeconds(2));
        command.Execute();
        LayerPropertyKeyframe<float> keyframe = rotation.Keyframes.Single();

        command.Undo();
        rotation.KeyframesEnabled.ShouldBeFalse();
        rotation.Keyframes.ShouldBeEmpty();
        Snapshot().ShouldBe(before);

        command.Execute();
        rotation.Keyframes.ShouldBe([keyframe]);
    }

    [Fact]
    public void ToggleLayerPropertyKeyframes_Enable_WithKeyframesAlreadyThere_AddsNone()
    {
        _opacity.KeyframesEnabled = false;
        string before = Snapshot();
        ToggleLayerPropertyKeyframes<float> command = new(_opacity, true, TimeSpan.FromSeconds(3));

        command.Execute();
        _opacity.KeyframesEnabled.ShouldBeTrue();
        _opacity.Keyframes.ShouldBe([_first, _second]);

        command.Undo();
        _opacity.KeyframesEnabled.ShouldBeFalse();
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void ToggleLayerPropertyKeyframes_Disable_UndoTurnsThemBackOn()
    {
        string before = Snapshot();
        ToggleLayerPropertyKeyframes<float> command = new(_opacity, false, TimeSpan.Zero);

        command.Execute();
        command.DisplayName.ShouldBe("Disable keyframes");
        _opacity.KeyframesEnabled.ShouldBeFalse();
        // The keyframes themselves are kept
        _opacity.Keyframes.ShouldBe([_first, _second]);

        command.Undo();
        _opacity.KeyframesEnabled.ShouldBeTrue();
        Snapshot().ShouldBe(before);
    }

    [Fact]
    public void ToggleLayerPropertyKeyframes_Undo_RestoresWhatItWasBefore()
    {
        ToggleLayerPropertyKeyframes<float> enable = new(_opacity, true, TimeSpan.Zero);

        enable.Execute();
        enable.Undo();

        // "Enabling" keyframes that were already enabled leaves them enabled when undone
        _opacity.KeyframesEnabled.ShouldBeTrue();
        _opacity.Keyframes.ShouldBe([_first, _second]);
    }
}
