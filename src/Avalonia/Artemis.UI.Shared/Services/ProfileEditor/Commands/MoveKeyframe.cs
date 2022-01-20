using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to change the position of a keyframe.
/// </summary>
public class MoveKeyframe : IProfileEditorCommand
{
    private readonly ILayerPropertyKeyframe _keyframe;
    private readonly TimeSpan _originalPosition;
    private readonly TimeSpan _position;

    /// <summary>
    ///     Creates a new instance of the <see cref="MoveKeyframe" /> class.
    /// </summary>
    public MoveKeyframe(ILayerPropertyKeyframe keyframe, TimeSpan position)
    {
        _keyframe = keyframe;
        _position = position;
        _originalPosition = keyframe.Position;
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Move keyframe";

    /// <inheritdoc />
    public void Execute()
    {
        _keyframe.Position = _position;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _keyframe.Position = _originalPosition;
    }

    #endregion
}