using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Shared.Services.NodeEditor;

/// <summary>
///     Represents the command history of a node script.
/// </summary>
public class NodeEditorHistory
{
    private readonly Subject<bool> _canRedo = new();
    private readonly Subject<bool> _canUndo = new();
    private readonly Stack<INodeEditorCommand> _redoCommands = new();
    private readonly Stack<INodeEditorCommand> _undoCommands = new();

    /// <summary>
    ///     Creates a new instance of the <see cref="NodeEditorHistory" /> class.
    /// </summary>
    /// <param name="nodeScript">The node script the history relates to.</param>
    public NodeEditorHistory(INodeScript nodeScript)
    {
        NodeScript = nodeScript;

        Execute = ReactiveCommand.Create<INodeEditorCommand>(ExecuteEditorCommand);
        Undo = ReactiveCommand.Create(ExecuteUndo, CanUndo);
        Redo = ReactiveCommand.Create(ExecuteRedo, CanRedo);
    }

    /// <summary>
    /// Gets the node script the history relates to.
    /// </summary>
    public INodeScript NodeScript { get; }
    
    /// <summary>
    ///     Gets an observable sequence containing a boolean value indicating whether history can be undone.
    /// </summary>
    public IObservable<bool> CanUndo => _canUndo.AsObservable().DistinctUntilChanged();
    
    /// <summary>
    ///     Gets an observable sequence containing a boolean value indicating whether history can be redone.
    /// </summary>
    public IObservable<bool> CanRedo => _canRedo.AsObservable().DistinctUntilChanged();

    /// <summary>
    ///     Gets a reactive command that can be executed to execute an instance of a <see cref="INodeEditorCommand" /> and puts it in history.
    /// </summary>
    public ReactiveCommand<INodeEditorCommand, Unit> Execute { get; }
    
    /// <summary>
    ///     Gets a reactive command that can be executed to undo history.
    /// </summary>
    public ReactiveCommand<Unit, INodeEditorCommand?> Undo { get; }
    
    /// <summary>
    ///     Gets a reactive command that can be executed to redo history.
    /// </summary>
    public ReactiveCommand<Unit, INodeEditorCommand?> Redo { get; }

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
    public void ExecuteEditorCommand(INodeEditorCommand command)
    {
        command.Execute();

        _undoCommands.Push(command);
        ClearRedo();
        UpdateSubjects();
    }

    private void ClearRedo()
    {
        foreach (INodeEditorCommand nodeEditorCommand in _redoCommands)
            if (nodeEditorCommand is IDisposable disposable)
                disposable.Dispose();

        _redoCommands.Clear();
    }

    private void ClearUndo()
    {
        foreach (INodeEditorCommand nodeEditorCommand in _undoCommands)
            if (nodeEditorCommand is IDisposable disposable)
                disposable.Dispose();

        _undoCommands.Clear();
    }

    private INodeEditorCommand? ExecuteUndo()
    {
        if (!_undoCommands.TryPop(out INodeEditorCommand? command))
            return null;

        command.Undo();
        _redoCommands.Push(command);
        UpdateSubjects();

        return command;
    }

    private INodeEditorCommand? ExecuteRedo()
    {
        if (!_redoCommands.TryPop(out INodeEditorCommand? command))
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