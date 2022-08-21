using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Shared.Services.ProfileEditor;

/// <summary>
///     Represents the command history of a profile configuration.
/// </summary>
public class ProfileEditorHistory
{
    private readonly Subject<bool> _canRedo = new();
    private readonly Subject<bool> _canUndo = new();
    private readonly Stack<IProfileEditorCommand> _redoCommands = new();
    private readonly Stack<IProfileEditorCommand> _undoCommands = new();

    /// <summary>
    ///     Creates a new instance of the <see cref="ProfileEditorHistory" /> class.
    /// </summary>
    /// <param name="profileConfiguration">The profile configuration the history relates to.</param>
    public ProfileEditorHistory(ProfileConfiguration profileConfiguration)
    {
        ProfileConfiguration = profileConfiguration;

        Execute = ReactiveCommand.Create<IProfileEditorCommand>(ExecuteEditorCommand);
        Undo = ReactiveCommand.Create(ExecuteUndo, CanUndo);
        Redo = ReactiveCommand.Create(ExecuteRedo, CanRedo);
    }

    /// <summary>
    ///     Gets the profile configuration the history relates to.
    /// </summary>
    public ProfileConfiguration ProfileConfiguration { get; }

    /// <summary>
    ///     Gets an observable sequence containing a boolean value indicating whether history can be undone.
    /// </summary>
    public IObservable<bool> CanUndo => _canUndo.AsObservable().DistinctUntilChanged();

    /// <summary>
    ///     Gets an observable sequence containing a boolean value indicating whether history can be redone.
    /// </summary>
    public IObservable<bool> CanRedo => _canRedo.AsObservable().DistinctUntilChanged();

    /// <summary>
    ///     Gets a reactive command that can be executed to execute an instance of a <see cref="IProfileEditorCommand" /> and
    ///     puts it in history.
    /// </summary>
    public ReactiveCommand<IProfileEditorCommand, Unit> Execute { get; }

    /// <summary>
    ///     Gets a reactive command that can be executed to undo history.
    /// </summary>
    public ReactiveCommand<Unit, IProfileEditorCommand?> Undo { get; }

    /// <summary>
    ///     Gets a reactive command that can be executed to redo history.
    /// </summary>
    public ReactiveCommand<Unit, IProfileEditorCommand?> Redo { get; }

    /// <summary>
    ///     Clears the history.
    /// </summary>
    public void Clear()
    {
        ClearRedo();
        ClearUndo();
        UpdateSubjects();
    }

    /// <summary>
    ///     Executes the provided <paramref name="command" /> and puts it in history.
    /// </summary>
    /// <param name="command">The command to execute</param>
    public void ExecuteEditorCommand(IProfileEditorCommand command)
    {
        command.Execute();

        _undoCommands.Push(command);
        ClearRedo();
        UpdateSubjects();
    }

    private void ClearRedo()
    {
        foreach (IProfileEditorCommand profileEditorCommand in _redoCommands)
        {
            if (profileEditorCommand is IDisposable disposable)
                disposable.Dispose();
        }

        _redoCommands.Clear();
    }

    private void ClearUndo()
    {
        foreach (IProfileEditorCommand profileEditorCommand in _undoCommands)
        {
            if (profileEditorCommand is IDisposable disposable)
                disposable.Dispose();
        }

        _undoCommands.Clear();
    }

    private IProfileEditorCommand? ExecuteUndo()
    {
        if (!_undoCommands.TryPop(out IProfileEditorCommand? command))
            return null;

        command.Undo();
        _redoCommands.Push(command);
        UpdateSubjects();

        return command;
    }

    private IProfileEditorCommand? ExecuteRedo()
    {
        if (!_redoCommands.TryPop(out IProfileEditorCommand? command))
            return null;

        command.Execute();
        _undoCommands.Push(command);
        UpdateSubjects();

        return command;
    }

    private void UpdateSubjects()
    {
        _canUndo.OnNext(_undoCommands.Any());
        _canRedo.OnNext(_redoCommands.Any());
    }
}