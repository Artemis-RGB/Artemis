using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to duplicate a keyframe at a new position.
/// </summary>
public class DuplicateKeyframe : IProfileEditorCommand
{
    private readonly ILayerPropertyKeyframe _keyframe;
    private readonly TimeSpan _position;

    /// <summary>
    ///     Creates a new instance of the <see cref="DeleteKeyframe" /> class.
    /// </summary>
    /// <param name="keyframe">The keyframe to duplicate.</param>
    /// <param name="position">The position of the duplicated keyframe.</param>
    public DuplicateKeyframe(ILayerPropertyKeyframe keyframe, TimeSpan position)
    {
        _keyframe = keyframe;
        _position = position;
    }

    /// <summary>
    ///     Gets the duplicated keyframe, only available after the command has been executed.
    /// </summary>
    public ILayerPropertyKeyframe? Duplication { get; private set; }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Duplicate keyframe";

    /// <inheritdoc />
    public void Execute()
    {
        if (Duplication == null)
        {
            Duplication = _keyframe.CreateCopy();
            Duplication.Position = _position;
        }

        _keyframe.UntypedLayerProperty.AddUntypedKeyframe(Duplication);
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (Duplication != null)
            _keyframe.UntypedLayerProperty.RemoveUntypedKeyframe(Duplication);
    }

    #endregion
}