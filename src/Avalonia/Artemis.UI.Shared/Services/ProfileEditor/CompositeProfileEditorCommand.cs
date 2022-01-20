using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.UI.Shared.Services.ProfileEditor;

/// <summary>
///     Represents a profile editor command that can be used to combine multiple commands into one.
/// </summary>
public class CompositeProfileEditorCommand : IProfileEditorCommand, IDisposable
{
    private readonly List<IProfileEditorCommand> _commands;

    /// <summary>
    ///     Creates a new instance of the <see cref="CompositeProfileEditorCommand" /> class.
    /// </summary>
    /// <param name="commands">The commands to execute.</param>
    /// <param name="displayName">The display name of the composite command.</param>
    public CompositeProfileEditorCommand(IEnumerable<IProfileEditorCommand> commands, string displayName)
    {
        if (commands == null)
            throw new ArgumentNullException(nameof(commands));
        _commands = commands.ToList();
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (IProfileEditorCommand profileEditorCommand in _commands)
            if (profileEditorCommand is IDisposable disposable)
                disposable.Dispose();
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public void Execute()
    {
        foreach (IProfileEditorCommand profileEditorCommand in _commands)
            profileEditorCommand.Execute();
    }

    /// <inheritdoc />
    public void Undo()
    {
        // Undo in reverse by iterating from the back
        for (int index = _commands.Count; index >= 0; index--)
            _commands[index].Undo();
    }

    #endregion
}