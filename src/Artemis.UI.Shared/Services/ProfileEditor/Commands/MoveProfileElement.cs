using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to move a profile element.
/// </summary>
public class MoveProfileElement : IProfileEditorCommand
{
    private readonly int _originalIndex;

    private readonly ProfileElement? _originalParent;
    private readonly ProfileElement _subject;
    private readonly ProfileElement _target;
    private readonly int _targetIndex;

    /// <summary>
    ///     Creates a new instance of the <see cref="MoveProfileElement" /> class.
    /// </summary>
    public MoveProfileElement(ProfileElement target, ProfileElement subject, int targetIndex)
    {
        _target = target;
        _subject = subject;
        _targetIndex = targetIndex;

        if (_subject.Parent != null)
        {
            _originalParent = _subject.Parent;
            _originalIndex = _subject.Parent.Children.IndexOf(_subject);
        }

        if (subject is Folder)
            DisplayName = "Move folder";
        DisplayName = "Move layer";
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public void Execute()
    {
        _subject.Parent?.RemoveChild(_subject);
        _target.AddChild(_subject, _targetIndex);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _target.RemoveChild(_subject);
        _originalParent?.AddChild(_subject, _originalIndex);
    }

    #endregion
}