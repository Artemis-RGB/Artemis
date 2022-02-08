using System.Reactive;
using System.Reactive.Linq;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.Interfaces;
using Avalonia.Input;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop
{
    public class WorkshopViewModel : MainScreenViewModel
    {
        private readonly INotificationService _notificationService;
        private StandardCursorType _selectedCursor;
        private readonly ObservableAsPropertyHelper<Cursor> _cursor;

        public WorkshopViewModel(IScreen hostScreen, INotificationService notificationService) : base(hostScreen, "workshop")
        {
            _notificationService = notificationService;
            _cursor = this.WhenAnyValue(vm => vm.SelectedCursor).Select(c => new Cursor(c)).ToProperty(this, vm => vm.Cursor);

            DisplayName = "Workshop";
            ShowNotification = ReactiveCommand.Create<NotificationSeverity>(ExecuteShowNotification);
        }

        public ReactiveCommand<NotificationSeverity, Unit> ShowNotification { get; set; }

        public StandardCursorType SelectedCursor
        {
            get => _selectedCursor;
            set => RaiseAndSetIfChanged(ref _selectedCursor, value);
        }

        public Cursor Cursor => _cursor.Value;

        private void ExecuteShowNotification(NotificationSeverity severity)
        {
            _notificationService.CreateNotification().WithTitle("Test title").WithMessage("Test message").WithSeverity(severity).Show();
        }
    }
}