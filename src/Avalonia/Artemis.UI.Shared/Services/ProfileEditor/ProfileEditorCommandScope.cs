using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Artemis.UI.Shared.Services.ProfileEditor;

/// <summary>
///     Represents a scope in which editor commands are executed until disposed.
/// </summary>
public class ProfileEditorCommandScope : IDisposable
{
    private readonly List<IProfileEditorCommand> _commands;

    private readonly ProfileEditorService _profileEditorService;

    internal ProfileEditorCommandScope(ProfileEditorService profileEditorService, string name)
    {
        Name = name;
        _profileEditorService = profileEditorService;
        _commands = new List<IProfileEditorCommand>();
    }

    /// <summary>
    ///     Gets the name of the scope.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets a read only collection of commands in the scope.
    /// </summary>
    public ReadOnlyCollection<IProfileEditorCommand> ProfileEditorCommands => new(_commands);

    internal void AddCommand(IProfileEditorCommand command)
    {
        command.Execute();
        _commands.Add(command);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _profileEditorService.StopCommandScope();
    }
}