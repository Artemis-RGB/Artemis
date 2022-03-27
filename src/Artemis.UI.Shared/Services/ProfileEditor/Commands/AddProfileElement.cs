using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
/// Represents a profile editor command that can be used to add a profile element.
/// </summary>
public class AddProfileElement : IProfileEditorCommand, IDisposable
{
    private readonly int _index;
    private readonly RenderProfileElement _subject;
    private readonly ProfileElement _target;
    private bool _isAdded;

    /// <summary>
    /// Creates a new instance of the <see cref="AddProfileElement"/> class.
    /// </summary>
    public AddProfileElement(RenderProfileElement subject, ProfileElement target, int index)
    {
        _subject = subject;
        _target = target;
        _index = index;

        DisplayName = subject switch
        {
            Layer => "Add layer",
            Folder => "Add folder",
            _ => throw new ArgumentException("Type of subject is not supported")
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_isAdded)
            _subject.Dispose();
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public void Execute()
    {
        _isAdded = true;
        _target.AddChild(_subject, _index);

        _subject.Enable();
    }

    /// <inheritdoc />
    public void Undo()
    {
        _isAdded = false;
        _target.RemoveChild(_subject);
    }

    #endregion
}