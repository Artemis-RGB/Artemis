using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.ProfileEditor;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.MenuBar
{
    public class MenuBarViewModel : ActivatableViewModelBase
    {
        private readonly INotificationService _notificationService;
        private ProfileEditorHistory? _history;
        private Action? _lastMessage;

        public MenuBarViewModel(IProfileEditorService profileEditorService, INotificationService notificationService)
        {
            _notificationService = notificationService;
            this.WhenActivated(d => profileEditorService.History.Subscribe(history => History = history).DisposeWith(d));
            this.WhenAnyValue(x => x.History)
                .Select(h => h?.Undo ?? Observable.Never<IProfileEditorCommand?>())
                .Switch()
                .Subscribe(DisplayUndo);
            this.WhenAnyValue(x => x.History)
                .Select(h => h?.Redo ?? Observable.Never<IProfileEditorCommand?>())
                .Switch()
                .Subscribe(DisplayRedo);
        }

        private void DisplayUndo(IProfileEditorCommand? command)
        {
            if (command == null || History == null)
                return;

            _lastMessage?.Invoke();
            _lastMessage = _notificationService.CreateNotification().WithMessage($"Undid '{command.DisplayName}'.").HavingButton(b => b.WithText("Redo").WithCommand(History.Redo)).Show();
        }

        private void DisplayRedo(IProfileEditorCommand? command)
        {
            if (command == null || History == null)
                return;

            _lastMessage?.Invoke();
            _notificationService.CreateNotification().WithMessage($"Redid '{command.DisplayName}'.").HavingButton(b => b.WithText("Undo").WithCommand(History.Undo)).Show(); ;
        }
        
        public ProfileEditorHistory? History
        {
            get => _history;
            set => this.RaiseAndSetIfChanged(ref _history, value);
        }
    }
}