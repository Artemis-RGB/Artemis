using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Workshop;
using Avalonia.Layout;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class SubmissionReleaseViewModel : RoutableScreen<ReleaseDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly IRouter _router;
    private readonly IWindowService _windowService;
    private readonly INotificationService _notificationService;

    [Notify] private IGetReleaseById_Release? _release;
    [Notify] private string? _changelog;
    [Notify] private bool _hasChanges;

    public SubmissionReleaseViewModel(IWorkshopClient client, IRouter router, IWindowService windowService, INotificationService notificationService)
    {
        _client = client;
        _router = router;
        _windowService = windowService;
        _notificationService = notificationService;
        this.WhenAnyValue(vm => vm.Changelog, vm => vm.Release, (current, release) => current != release?.Changelog).Subscribe(hasChanges => HasChanges = hasChanges);

        Discard = ReactiveCommand.Create(ExecuteDiscard, this.WhenAnyValue(vm => vm.HasChanges));
        Save = ReactiveCommand.CreateFromTask(ExecuteSave, this.WhenAnyValue(vm => vm.HasChanges));
    }

    public ReactiveCommand<Unit, Unit> Discard { get; set; }
    public ReactiveCommand<Unit, Unit> Save { get; set; }

    public override async Task OnNavigating(ReleaseDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        IOperationResult<IGetReleaseByIdResult> result = await _client.GetReleaseById.ExecuteAsync(parameters.ReleaseId, cancellationToken);
        Release = result.Data?.Release;
        Changelog = Release?.Changelog;
    }
    
    public override async Task OnClosing(NavigationArguments args)
    {
        if (!HasChanges)
            return;

        bool confirmed = await _windowService.ShowConfirmContentDialog("You have unsaved changes", "Do you want to discard your unsaved changes?");
        if (!confirmed)
            args.Cancel();
    }

    public async Task DeleteRelease()
    {
        if (Release == null)
            return;

        bool confirmed = await _windowService.ShowConfirmContentDialog(
            "Delete release?",
            "This cannot be undone.\r\n" +
            "Users that have already downloaded this release will keep it.");
        if (!confirmed)
            return;
        
        await _client.RemoveRelease.ExecuteAsync(Release.Id);
        _notificationService.CreateNotification()
            .WithTitle("Deleted release.")
            .WithSeverity(NotificationSeverity.Success)
            .WithHorizontalPosition(HorizontalAlignment.Left)
            .Show();
        
        HasChanges = false;
        await Close();
    }

    public async Task Close()
    {
        await _router.GoUp();
    }
    
    private async Task ExecuteSave(CancellationToken cancellationToken)
    {
        if (Release == null)
            return;

        await _client.UpdateRelease.ExecuteAsync(new UpdateReleaseInput {Id = Release.Id, Changelog = Changelog}, cancellationToken);
        _notificationService.CreateNotification()
            .WithTitle("Saved changelog.")
            .WithSeverity(NotificationSeverity.Success)
            .WithHorizontalPosition(HorizontalAlignment.Left)
            .Show();

        HasChanges = false;
    }

    private void ExecuteDiscard()
    {
        Changelog = Release?.Changelog;
        HasChanges = false;
    }
}