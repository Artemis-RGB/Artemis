using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to delete a keyframe.
/// </summary>
public class DeleteKeyframe<T> : IProfileEditorCommand
{
    private readonly LayerPropertyKeyframe<T> _keyframe;

    /// <summary>
    ///     Creates a new instance of the <see cref="DeleteKeyframe{T}" /> class.
    /// </summary>
    public DeleteKeyframe(LayerPropertyKeyframe<T> keyframe)
    {
        _keyframe = keyframe;
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Delete keyframe";

    /// <inheritdoc />
    public void Execute()
    {
        _keyframe.LayerProperty.RemoveKeyframe(_keyframe);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _keyframe.LayerProperty.AddKeyframe(_keyframe);
    }

    #endregion
}