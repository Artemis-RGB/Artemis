using Artemis.Core;
using Humanizer;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to change the easing function of a keyframe.
/// </summary>
public class ChangeKeyframeEasing : IProfileEditorCommand
{
    private readonly Easings.Functions _easingFunction;
    private readonly ILayerPropertyKeyframe _keyframe;
    private readonly Easings.Functions _originalEasingFunction;

    /// <summary>
    ///     Creates a new instance of the <see cref="ChangeKeyframeEasing" /> class.
    /// </summary>
    public ChangeKeyframeEasing(ILayerPropertyKeyframe keyframe, Easings.Functions easingFunction)
    {
        _keyframe = keyframe;
        _easingFunction = easingFunction;
        _originalEasingFunction = keyframe.EasingFunction;
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Change easing to " + _easingFunction.Humanize(LetterCasing.LowerCase);

    /// <inheritdoc />
    public void Execute()
    {
        _keyframe.EasingFunction = _easingFunction;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _keyframe.EasingFunction = _originalEasingFunction;
    }

    #endregion
}