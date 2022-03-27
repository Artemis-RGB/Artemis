using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to rename a profile element.
/// </summary>
public class RenameProfileElement : IProfileEditorCommand
{
    private readonly string? _name;
    private readonly string? _originalName;
    private readonly ProfileElement _subject;

    /// <summary>
    ///     Creates a new instance of the <see cref="RenameProfileElement" /> class.
    /// </summary>
    public RenameProfileElement(ProfileElement subject, string? name)
    {
        _subject = subject;
        _name = name;
        _originalName = subject.Name;

        DisplayName = subject switch
        {
            Layer => "Rename layer",
            Folder => "Rename folder",
            _ => throw new ArgumentException("Type of subject is not supported")
        };
    }


    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public void Execute()
    {
        _subject.Name = _name;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _subject.Name = _originalName;
    }

    #endregion
}