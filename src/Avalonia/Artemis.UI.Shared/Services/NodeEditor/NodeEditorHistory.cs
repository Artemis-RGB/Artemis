using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Shared.Services.NodeEditor;

public class NodeEditorHistory
{
    private readonly Subject<bool> _canRedo = new();
    private readonly Subject<bool> _canUndo = new();
    private readonly Stack<INodeEditorCommand> _redoCommands = new();
    private readonly Stack<INodeEditorCommand> _undoCommands = new();

    public NodeEditorHistory(INodeScript nodeScript)
    {
        NodeScript = nodeScript;

        Execute = ReactiveCommand.Create<INodeEditorCommand>(ExecuteEditorCommand);
        Undo = ReactiveCommand.Create(ExecuteUndo, CanUndo);
        Redo = ReactiveCommand.Create(ExecuteRedo, CanRedo);
    }

    public INodeScript NodeScript { get; }
    public IObservable<bool> CanUndo => _canUndo.AsObservable().DistinctUntilChanged();
    public IObservable<bool> CanRedo => _canRedo.AsObservable().DistinctUntilChanged();

    public ReactiveCommand<INodeEditorCommand, Unit> Execute { get; }
    public ReactiveCommand<Unit, INodeEditorCommand?> Undo { get; }
    public ReactiveCommand<Unit, INodeEditorCommand?> Redo { get; }

    public void Clear()
    {
        ClearRedo();
        ClearUndo();
        UpdateSubjects();
    }

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