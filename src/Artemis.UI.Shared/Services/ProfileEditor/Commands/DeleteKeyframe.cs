using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to delete a keyframe.
/// </summary>
public class DeleteKeyframe : IProfileEditorCommand
{
    private readonly ILayerPropertyKeyframe _keyframe;

    /// <summary>
    ///     Creates a new instance of the <see cref="DeleteKeyframe" /> class.
    /// </summary>
    public DeleteKeyframe(ILayerPropertyKeyframe keyframe)
    {
        _keyframe = keyframe;
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Delete keyframe";

    /// <inheritdoc />
    public void Execute()
    {
        _keyframe.UntypedLayerProperty.RemoveUntypedKeyframe(_keyframe);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _keyframe.UntypedLayerProperty.AddUntypedKeyframe(_keyframe);
    }

    #endregion
}