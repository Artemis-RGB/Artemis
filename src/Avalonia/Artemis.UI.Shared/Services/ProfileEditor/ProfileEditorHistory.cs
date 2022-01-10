using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Shared.Services.ProfileEditor
{
    public class ProfileEditorHistory
    {
        private readonly Subject<bool> _canRedo = new();
        private readonly Subject<bool> _canUndo = new();
        private readonly Stack<IProfileEditorCommand> _redoCommands = new();
        private readonly Stack<IProfileEditorCommand> _undoCommands = new();

        public ProfileEditorHistory(ProfileConfiguration profileConfiguration)
        {
            ProfileConfiguration = profileConfiguration;

            Execute = ReactiveCommand.Create<IProfileEditorCommand>(ExecuteEditorCommand);
            Undo = ReactiveCommand.Create(ExecuteUndo, CanUndo);
            Redo = ReactiveCommand.Create(ExecuteRedo, CanRedo);
        }

        public ProfileConfiguration ProfileConfiguration { get; }
        public IObservable<bool> CanUndo => _canUndo.AsObservable().DistinctUntilChanged();
        public IObservable<bool> CanRedo => _canRedo.AsObservable().DistinctUntilChanged();

        public ReactiveCommand<IProfileEditorCommand, Unit> Execute { get; }
        public ReactiveCommand<Unit, IProfileEditorCommand?> Undo { get; }
        public ReactiveCommand<Unit, IProfileEditorCommand?> Redo { get; }

        public void Clear()
        {
            ClearRedo();
            ClearUndo();
            UpdateSubjects();
        }

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
                if (profileEditorCommand is IDisposable disposable)
                    disposable.Dispose();

            _redoCommands.Clear();
        }

        private void ClearUndo()
        {
            foreach (IProfileEditorCommand profileEditorCommand in _undoCommands)
                if (profileEditorCommand is IDisposable disposable)
                    disposable.Dispose();

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
}