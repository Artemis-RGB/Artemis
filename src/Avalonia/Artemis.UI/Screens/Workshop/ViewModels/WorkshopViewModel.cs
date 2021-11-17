using System.Reactive;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.Interfaces;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.ViewModels
{
    public class WorkshopViewModel : MainScreenViewModel
    {
        private readonly INotificationService _notificationService;

        public WorkshopViewModel(IScreen hostScreen, INotificationService notificationService) : base(hostScreen, "workshop")
        {
            _notificationService = notificationService;

            DisplayName = "Workshop";
            ShowNotification = ReactiveCommand.Create<NotificationSeverity>(ExecuteShowNotification);
        }

        public ReactiveCommand<NotificationSeverity, Unit> ShowNotification { get; set; }

        private void ExecuteShowNotification(NotificationSeverity severity)
        {
            _notificationService.CreateNotification().WithTitle("Test title").WithMessage("Test message").WithSeverity(severity).Show();
        }
    }
}